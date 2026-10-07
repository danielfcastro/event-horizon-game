// A-022 Phase 2 frame driver and render state — Assets/Runtime/Bridge/Unity/FrameDriver.cs
// A-005 §3.2 frame-driven loop, A-019 §10 weak-device switch and quality-tier probe.
//
// *** THIS FILE IS COMPILED IN THE HEADLESS BUILD ***
// It lives in Bridge/Unity because that is the bridge layer between the frame
// clock and the sim, and because InputAdapter / LevelLoader / PlatformBridge
// already compile headless there. It is NOT in the FIXED-FORK hashed set
// (main.cs hashes exactly Fixed/ + SimCore/), so adding it cannot invalidate a
// committed golden by itself.
//
// Hard rules honoured here:
//  * The frame driver NEVER re-implements stepping. One frame calls
//    FixedStepDriver.advanceDigest, which owns the accumulator, DT = 1/60,
//    MAX_CATCHUP = 4 and the dropped-step rule (A-019 §11: "there is exactly
//    one simulation loop and both entries reach it"). GameLoop.cs and the
//    harness both reach it through this file.
//  * A hitch perturbs the FRAME CLOCK only: it adds wall-clock debt to one
//    frame. It never touches SimCore, DT, or an intent.
//  * Quality tiers change render state only (A-019 §10 item 2, A-005). Nothing
//    here is readable by SimCore; the invariance is enforced by H-09 comparing
//    SimState bytes across tiers, not by a comment.
//  * No float/double: alpha is Q32.32 raw and render-only.
//
// Per-run state, not globals: H-06/H-07/H-09 compare two or three frame-driven
// runs at once, so the clock (frame index, hitch schedule, tier) is an object
// handed to frame(); static globals would let one run's clock corrupt another's
// comparison.

namespace EH
{
    public static class FrameDriver
    {
        // ---- the per-run frame clock -------------------------------------------
        public class FrameClock
        {
            public int targetFps;          // 60 or 30 (A-019 §10 knob)
            public long timeScaleRaw;      // Q32.32, 1.0 = FixedQ.ONE
            public long frameIndex;        // rendered frames since boot
            public long framesToGoal;      // frames consumed when the goal hit (-1 = not reached)
            public bool goalReached;
            public long lastFrameSteps;    // steps delivered by the most recent frame
            public long maxFrameSteps;     // highmark of steps delivered in one frame
            public long renderAlphaRaw;    // accumulator / DT, Q32.32 in [0,1), render-only
            public int tier;               // 0 = unknown, 1 low, 2 medium, 3 high
            public long pixelsPerUnit;     // render-only mapping for this tier
            public int hitchCount;         // scheduled hitches
            public int hitchConsumed;      // hitches already applied
            public long[] hitchFrame;      // frame indices, ascending
            public long[] hitchMs;         // stall duration in whole milliseconds
        }

        // ---- level-independent constants --------------------------------------
        // A whole millisecond in Q32.32 seconds is not exact (2^32/1000 has a
        // remainder), so the conversion is floor and documented: a hitch is a
        // frame-clock input, never a sim input, so its rounding cannot move a
        // step boundary — the accumulator absorbs the remainder and DT is
        // asserted constant every step.
        public static long msToRaw(long ms)
        {
            return MulDiv.muldiv(ms, FixedQ.ONE, 1000L);
        }

        /// <summary>
        /// The win condition, owned here so the step-driven harness loop and
        /// the frame-driven loop test the SAME predicate (a duplicated check
        /// could drift and make a frame-driven comparison pass vacuously).
        /// </summary>
        public static bool goal(SimState s)
        {
            return (s.holeMass >> 32) >= LevelTable.targetMass;
        }

        /// <summary>
        /// Boot a frame clock. The startup gate is FixedStepDriver's (A-019
        /// §10: a timeScale implying another DT is rejected at startup, and
        /// this file owns no duplicate gate), so an unreachable fps or timeScale
        /// is a hard rejection before the first frame, never a silent pass.
        /// </summary>
        public static FrameClock boot(int targetFps, long timeScaleRaw, int hitchSlots)
        {
            FixedStepDriver.configureStartup(timeScaleRaw, targetFps);
            FrameClock c = new FrameClock();
            c.targetFps = targetFps;
            c.timeScaleRaw = timeScaleRaw;
            c.frameIndex = 0L;
            c.framesToGoal = -1L;
            c.goalReached = false;
            c.lastFrameSteps = 0L;
            c.maxFrameSteps = 0L;
            c.renderAlphaRaw = 0L;
            c.tier = 0;
            c.pixelsPerUnit = PlatformBridge.pixelsPerUnit;
            c.hitchConsumed = 0;
            c.hitchCount = 0;
            c.hitchFrame = new long[hitchSlots];
            c.hitchMs = new long[hitchSlots];
            return c;
        }

        /// <summary>
        /// Schedule deterministic hitches (H-06: `--at 120,341,902 --ms 42`).
        /// Ascending frame indices are required — an out-of-order schedule would
        /// silently skip a hitch, so it is a hard rejection, not a sort.
        /// </summary>
        public static void schedule(FrameClock c, long[] frames, long[] ms, int n)
        {
            Fail.check(n <= c.hitchFrame.Length, "FrameDriver.schedule: more hitches than the clock holds");
            long prev = -1L;
            for (int k = 0; k < n; k++)
            {
                Fail.check(frames[k] > prev, "FrameDriver.schedule: hitch frame indices must be strictly ascending");
                Fail.check(ms[k] > 0L, "FrameDriver.schedule: a hitch must stall a positive number of ms");
                c.hitchFrame[k] = frames[k];
                c.hitchMs[k] = ms[k];
                prev = frames[k];
            }
            c.hitchCount = n;
        }

        /// <summary>
        /// One rendered frame on the DETERMINISTIC path: per-step intents come
        /// from the EIDIG1 channel. This is the path H-01/H-02/H-06/H-07/H-09
        /// and re-simulable playback use.
        /// </summary>
        public static int frame(FrameClock c, SimState s, byte[] digest)
        {
            long wall = beginFrame(c);
            int executed = FixedStepDriver.advanceDigest(s, wall, digest);
            endFrame(c, s, executed);
            return executed;
        }

        /// <summary>
        /// One rendered frame on the LIVE path (Unity GameLoop): the platform
        /// layer hands over one already-resolved intent for the frame. At 30 fps
        /// a frame delivers two steps and both share it — the per-frame to
        /// per-step mapping for live input is A-011's decision, and this file
        /// does not invent one (A-019 §4: InputAdapter holds no input scheme).
        /// </summary>
        public static int frameLive(FrameClock c, SimState s, Intent i)
        {
            long wall = beginFrame(c);
            int executed = FixedStepDriver.advance(s, wall, i);
            endFrame(c, s, executed);
            return executed;
        }

        /// <summary>
        /// The frame clock: target-fps wall budget plus any hitch scheduled for
        /// this frame. Shared by both entries, so a hitch is modelled exactly
        /// once and cannot mean one thing to the harness and another to Unity.
        /// </summary>
        private static long beginFrame(FrameClock c)
        {
            long wall = PlatformBridge.frameWallDeltaFor(c.targetFps, c.timeScaleRaw);
            long extra = takeHitchRaw(c);
            if (extra != 0L)
            {
                wall = FixedQ.add(wall, extra);
            }
            return wall;
        }

        /// <summary>
        /// Frame bookkeeping and the render-only alpha. Owned here so neither
        /// entry keeps its own accounting (a duplicated counter could disagree
        /// with the other entry while both tests stayed green).
        /// </summary>
        private static void endFrame(FrameClock c, SimState s, int executed)
        {
            c.frameIndex += 1L;
            c.lastFrameSteps = executed;
            if (executed > c.maxFrameSteps)
            {
                c.maxFrameSteps = executed;
            }
            // A-005 §3.2: alpha = accumulator / DT, render-only, never fed back.
            c.renderAlphaRaw = MulDiv.muldiv(s.accumulator, FixedQ.ONE, FixedStepDriver.DT);
            if (goal(s))
            {
                c.goalReached = true;
                c.framesToGoal = c.frameIndex;
            }
        }

        /// <summary>
        /// Wall ms of a hitch scheduled for the frame about to run (0 if none).
        /// Consumed once each: a schedule is a property of the run, so re-use
        /// of a consumed hitch is impossible by construction.
        /// </summary>
        static long takeHitchRaw(FrameClock c)
        {
            if (c.hitchConsumed >= c.hitchCount)
            {
                return 0L;
            }
            if (c.hitchFrame[c.hitchConsumed] != c.frameIndex)
            {
                return 0L;
            }
            long ms = c.hitchMs[c.hitchConsumed];
            c.hitchConsumed += 1;
            return msToRaw(ms);
        }

        /// <summary>
        /// Wall-clock frame budget for a level: timerSeconds x targetFps frames.
        /// Owned here so the driver loop and the probe commands agree on when a
        /// run is out of wall clock (a duplicated bound could disagree).
        /// </summary>
        public static long frameBudget(int targetFps)
        {
            return LevelTable.timerSeconds * (long)targetFps;
        }

        /// <summary>
        /// Drive frames until the level is decided. The budget is wall-clock
        /// frames (timerSeconds x targetFps), so a hitching device spends more
        /// frames on the same simulation — which is exactly what H-06 asserts.
        /// Returns true when the goal was reached.
        /// </summary>
        public static bool runFrames(FrameClock c, SimState s, byte[] digest)
        {
            long budget = frameBudget(c.targetFps);
            while (s.stepIndex < LevelTable.timerSeconds * 60L && c.frameIndex < budget)
            {
                frame(c, s, digest);
                if (goal(s))
                {
                    return true;
                }
            }
            return goal(s);
        }

        // ---- render state: quality tier (A-019 §10 item 2) ---------------------
        /// <summary>
        /// Set the quality tier. This changes render state ONLY: the pixel
        /// mapping and the tier tag. Magnitudes are placeholders owned by
        /// A-006/A-010 (art and UI); the probe's job is the invariance, so H-09
        /// asserts SimState bytes are identical across tiers rather than trusting
        /// these numbers. Nothing here is readable by SimCore.
        /// </summary>
        public static void setTier(FrameClock c, int tier)
        {
            Fail.check(tier == 1 || tier == 2 || tier == 3,
                "FrameDriver.setTier: tier must be 1 low, 2 medium, or 3 high");
            c.tier = tier;
            c.pixelsPerUnit = tier == 1 ? 24L << 32 : (tier == 2 ? 32L << 32 : 48L << 32);
            PlatformBridge.pixelsPerUnit = c.pixelsPerUnit;
        }

        /// <summary>
        /// The device tier probe. Honest: this environment has no device, so no
        /// tier is selected and none is guessed (A-019 §10: the probe RETURNS
        /// the selected tier; inventing one would let a quality setting become a
        /// difficulty setting on a real device without any test seeing it).
        /// Device-derived selection belongs to A-023.
        /// </summary>
        public static int selectTier()
        {
            return 0; // 0 = unknown: no device here, never a guessed default
        }
    }
}

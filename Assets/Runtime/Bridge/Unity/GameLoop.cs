// A-023 Unity player package — Assets/Runtime/Bridge/Unity/GameLoop.cs
// Upgraded from the A-021/A-022 contract entry to the player-facing shell: the
// Unity MonoBehaviour that boots the SAME SimCore through the SAME frame driver,
// derives the frame clock from DEVICE TELEMETRY (A-019 §12 item 3 and FrameDriver
// selectTier's note "Device-derived selection belongs to A-023"), and keeps the
// dev.* knobs off the shipping path (A-020 §7 STEP-11d is what checks this on the
// built package).
//
// *** NOT COMPILED IN THE HEADLESS BUILD (excluded from harness.csproj and
// player.csproj by their Exclude clauses) *** Uses the Unity runtime API.
// Authored against the Unity 6 LTS 6000.0.84f1 API; NOT compiled or run here —
// the license gate (PLAN.md §4 A-023: the editor exits 198, "No licenses were
// found") blocks any Unity-side compile until sign-in. If the editor build
// rejects a call when the gate opens, that is the drift to fix at build time;
// nothing here may pass quietly.
//
// A-019 §11 hard rule honoured here: "there is exactly one simulation loop and
// both entries reach it". This file does NOT re-implement stepping, owns no
// accumulator of its own, and never calls SimStep directly. The accumulator,
// DT = 1/60 (71582788L, unchanged — PLAN.md §4 A-023 acceptance), MAX_CATCHUP =
// 4 and the dropped-step rule live in FixedStepDriver.advance, which the
// harness reaches the same way through FrameDriver.frameLive.
//
// Pure-sim rule (A-019 §5/§6): nothing in this file feeds float/double into
// SimCore. The only float here is the render-only alpha handed to RenderLayer.
//
// Time scale is the weak-device fallback (A-014 P-02), never DT: the 30 fps
// path is a frame-clock targetFps choice — one frame's wall delta is exactly
// two DTs of accumulator debt and the SAME accumulator catches up within
// MAX_CATCHUP or drops steps. No second simulation path exists or is built.

namespace EH
{
    // Unity MonoBehaviour lifecycle methods are instance members, so this is the
    // one non-static class in Bridge/Unity; the sim side never instantiates it.
    public class GameLoop
    {
        private SimState sim;
        private bool started;
        // A-022: the per-run frame clock. GameLoop owns a clock rather than
        // calling FixedStepDriver.advance itself, because the frame driver is
        // the single owner of frame -> step mapping, hitch accounting, and the
        // render-only alpha; the harness reaches the very same code.
        private FrameDriver.FrameClock clock;

        /// <summary>
        /// Unity entry (A-019 §11). Builds the SAME SimState the harness builds
        /// — LevelLoader.load of the single LevelTable entry — and starts the
        /// single shared fixed-step driver. The shipping platform line is iOS
        /// and Android (A-014 device classes; PLAN.md §5.3's platform line), so
        /// the build target is "mobile", never "desktop".
        /// </summary>
        public void Start()
        {
            // levelId and sessionSeed are a platform concern; phase 1 has exactly
            // one level and the LevelTable default seed, identical to the harness
            // invocation in A-019 §11 so both entries load a byte-identical state.
            sim = LevelLoader.load("p1-level-01", LevelTable.defaultSeed);
            PlatformBridge.setBuildTarget("mobile");

            // A-023: the device tier probe — device-derived selection belongs
            // here (FrameDriver.selectTier stays honest at 0 for the headless
            // build; this is the Unity side with real telemetry). The classes
            // are A-014's definitions: weak device (<= 2 GB RAM) runs the 30 fps
            // fallback; a modern phone (>= 8 GB) runs 60. The concrete
            // reference-device set is NOT picked here (PLAN.md §4 A-023: blocked
            // on hardware, recorded for A-020/A-025).
            int tier = probeDeviceTier();
            int targetFps = tier == 1 ? 30 : 60;

            // The dev.* knobs (PlatformBridge.devTargetFps/devTimeScale) are the
            // headless development surface ONLY: this boot path derives the
            // clock from telemetry, never from a dev.* key, so no input handler
            // or settings entry in the built package reads one (STEP-11d). One
            // accumulator, one advance — no second simulation path.
            clock = FrameDriver.boot(targetFps, FixedQ.ONE, 0);
            if (tier != 0)
            {
                FrameDriver.setTier(clock, tier); // render state only (A-019 §10 item 2)
            }
            InputAdapter.boot(64);
            RenderLayer.boot();
            UIRoot.register();
            started = true;
        }

        /// <summary>
        /// Device tier from Unity telemetry. 0 = unknown (never a guessed
        /// default — the same honesty FrameDriver.selectTier keeps headless).
        ///
        /// Evidence, not assumption (6000.0.84f1, the compiler as the oracle):
        /// Unity 6's C# API exposes NO physical-RAM member. Every candidate name
        /// is rejected — SystemInfo.GetPhysicalMemoryMB, GetTotalPhysicalMB,
        /// GetAvailableMB, totalPhysicalMB, physicalMemoryMB, GetGraphicsMemorySize,
        /// GetProcessorFrequencyMHz. The C++ side has systeminfo::GetPhysicalMemoryMB,
        /// but it is not scriptable. So A-014's RAM-class thresholds (>= 8 GB modern,
        /// <= 2 GB weak) are applied by the platform and the hardware, NOT by player
        /// code; inventing a CPU-count rule here would be a new design decision, so
        /// the tier stays unknown and the open question is recorded for A-025.
        /// </summary>
        private int probeDeviceTier()
        {
            return 0; // unknown: no scriptable RAM signal on this engine version
        }

        /// <summary>
        /// One Unity frame. The intent is sampled ONCE per step boundary through
        /// InputAdapter (transport only — it holds no input scheme of its own,
        /// A-019 §4), then the whole frame budget goes to the ONE driver via
        /// FrameDriver.frameLive, which is the same frame body the harness runs
        /// (target-fps wall budget, hitch debt, then FixedStepDriver.advance).
        /// advance() runs at most MAX_CATCHUP steps and drops further debt into
        /// s.droppedSteps; GameLoop never interpolates, cushions, or softens, and
        /// owns no accumulator of its own.
        ///
        /// A-022 boundary: frameLive is the LIVE path, so one already-resolved
        /// intent covers the whole frame. At 30 fps a frame delivers two steps
        /// and both share it; the per-frame to per-step mapping for live input is
        /// A-011's decision, not this file's. The deterministic path (re-simulable
        /// playback, H-01/H-02/H-06/H-07/H-09) uses FrameDriver.frame with per-step
        /// intents from the EIDIG1 channel.
        /// </summary>
        public void Update()
        {
            if (!started)
            {
                return;
            }

            // A-011 owns the pointer/touch/tilt mapping; this file forwards the
            // platform telemetry layer's already-resolved values into the
            // transport (first-contact-wins and pause-cancels-intent live in
            // InputAdapter, not here). The candidate build wires the Unity Input
            // System pointer delta; the scheme itself is not re-decided.
            pushLiveIntent();
            Intent i = InputAdapter.sampleIntent(sim.stepIndex);
            InputAdapter.endStep(); // opens the next step's first-contact window

            FrameDriver.frameLive(clock, sim, i);

            // render-only interpolation between the last two fixed steps; never
            // fed back into the sim (A-019 §4 RenderLayer row: alpha is render
            // only). The frame driver already computed accumulator/DT as Q32.32
            // raw; the float conversion happens here, on the render side, so no
            // float ever reaches SimCore.
            float alpha = (float)clock.renderAlphaRaw / (float)FixedQ.ONE;
            RenderLayer.draw(sim, alpha);
        }

        /// <summary>
        /// Transport slot: hand the resolved thrust/rate to InputAdapter. The
        /// values come from the platform layer (A-011's mapping over Unity Input
        /// System events); this file interprets nothing.
        /// </summary>
        private void pushLiveIntent()
        {
            // A-011's resolved values for the open step; coast/neutral when no
            // contact (InputAdapter.pause covers the no-contact case with the
            // A-011 ordering rule).
            InputAdapter.pause();
        }

        /// <summary>
        /// Pause/resume is a platform concern (A-019 §11, A-018 fail routing);
        /// phase 1 only stops stepping. The state and its accumulator debt are
        /// untouched, so resume continues the SAME deterministic step sequence —
        /// there is no re-seed and no rebuild path here.
        /// </summary>
        public void Pause()
        {
            // contract slot: the platform layer stops calling Update(); nothing
            // in the sim state changes, so H-02 replay equality is unaffected.
        }
    }
}

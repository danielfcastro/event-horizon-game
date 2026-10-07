// A-021 Phase 1 prototype — Assets/Runtime/Bridge/Unity/PlatformBridge.cs
// A-019 §4 / §10: PlatformBridge (minimal): build-target switch, `pixelsPerUnit`
// render-only constant (NEVER read by SimCore — A-019 §6), tier probe stub, and
// the dev.targetFps (60/30) + dev.timeScale (default 1.0) knobs that scale
// WALL-CLOCK advance into the FixedStepDriver accumulator ONLY — never DT.
//
// Startup rejection of a timeScale implying another DT is FixedStepDriver's job
// (A-019 §10): this bridge calls FixedStepDriver.configureStartup and owns no
// duplicate gate. A-019 §12 item 3: the shipping gate for dev.targetFps is an
// A-020 ship question — phase 1 keeps the knob here and builds NO second
// simulation path.
//
// Unity-API-free (brief §9: compiles in the headless harness). No float/double
// anywhere: timeScale is Q32.32 raw (1.0 = FixedQ.ONE).
//
// pixelsPerUnit note: this constant is consumed by RenderLayer.cs (Unity side,
// not compiled headless) to map WU -> screen pixels. SimCore never reads it;
// the sim is resolution- and DPI-independent by construction.

namespace EH
{
    public static class PlatformBridge
    {
        // ---- build-target switch -------------------------------------------------
        // "headless" = harness/dotnet build (this environment); "mobile" / "desktop"
        // = Unity shipping targets. Unknown targets are a hard rejection.
        public static string buildTarget = "headless";

        public static void setBuildTarget(string target)
        {
            Fail.check(target == "headless" || target == "mobile" || target == "desktop",
                "PlatformBridge.setBuildTarget: unknown build target");
            buildTarget = target;
        }

        // ---- render-only mapping (A-019 §6: 1 WU = starting hole radius) ----------
        // Q32.32 raw pixels per world unit; RenderLayer reads it, SimCore never does.
        public static long pixelsPerUnit = 32L << 32; // 32 px/WU default, render-only

        // ---- weak-device knobs (A-019 §10) -----------------------------------------
        public static int devTargetFps = 60;      // 60 or 30 only
        public static long devTimeScaleRaw = FixedQ.ONE; // default 1.0, Q32.32 raw

        /// <summary>
        /// Startup gate: delegate the DT/timeScale rejection and the 60/30 check
        /// to FixedStepDriver (its job per A-019 §10 — no duplicated gate here).
        /// Call once before the first advance().
        /// </summary>
        public static void configureStartup()
        {
            FixedStepDriver.configureStartup(devTimeScaleRaw, devTargetFps);
        }

        /// <summary>
        /// Wall-clock advance budget for one frame at the configured targetFps,
        /// scaled by dev.timeScale. This value goes into FixedStepDriver.advance
        /// (the accumulator) ONLY — DT stays the constant 1/60 (asserted there
        /// every step). At 30 fps the frame's wall delta is exactly two DTs of
        /// accumulator debt; the sim catches up within MAX_CATCHUP or drops
        /// steps (droppedSteps counter), never interpolating (no-softening).
        /// Q32.32 raw seconds; integer math only.
        /// </summary>
        public static long frameWallDelta()
        {
            // one frame of wall time at targetFps: 1/60 or 1/30 s as exact
            // multiples of the DT constant (30 fps -> 2 * DT).
            long framesDT = devTargetFps == 30 ? 2L : 1L;
            long wall = FixedStepDriver.DT * framesDT; // exact, no rounding
            return MulDiv.muldiv(wall, devTimeScaleRaw, FixedQ.ONE); // scale wall-clock only
        }

        /// <summary>
        /// Convenience: hand the scaled wall delta straight to the driver's
        /// accumulator (the single place the knobs are allowed to touch).
        /// Returns the number of sim steps executed.
        /// </summary>
        public static int advanceOneFrame(SimState s, Intent i)
        {
            return FixedStepDriver.advance(s, frameWallDelta(), i);
        }

        /// <summary>
        /// Tier probe stub (A-019 §4: tier probe; H-09 `tier --probe`).
        /// Honest stub: phase 1 has no device tiering — returns 0 = "unknown"
        /// and never guesses a cushioning default. A-020/ship fills it.
        /// </summary>
        public static int probeTier()
        {
            return 0; // 0 = unknown (honest stub, identity — no default tier)
        }
    }
}

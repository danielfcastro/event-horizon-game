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
            return frameWallDeltaFor(devTargetFps, devTimeScaleRaw);
        }

        /// <summary>
        /// A-022: the same formula, parameterised, so the frame driver can run
        /// two frame clocks at once (H-07 compares a 30 fps clock against the
        /// step-driven golden) without a global knob deciding both budgets.
        /// One implementation, no duplicated budget math.
        /// </summary>
        public static long frameWallDeltaFor(int targetFps, long timeScaleRaw)
        {
            long framesDT = targetFps == 30 ? 2L : 1L;
            long wall = FixedStepDriver.DT * framesDT; // exact, no rounding
            return MulDiv.muldiv(wall, timeScaleRaw, FixedQ.ONE); // scale wall-clock only
        }

        /// <summary>
        /// A-022: the live-device knob (A-019 §10 keeps it here; the frame
        /// driver's per-run clock is authoritative for probe runs). Setting it
        /// re-runs the startup gate, so an unreachable targetFps is a hard
        /// rejection at the same place FixedStepDriver owns it, never a silent
        /// acceptance.
        /// </summary>
        public static void setDevKnobs(int targetFps, long timeScaleRaw)
        {
            devTargetFps = targetFps;
            devTimeScaleRaw = timeScaleRaw;
            configureStartup();
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

        // ---- A-023 player-package platform slots (A-015..A-017 surfaces) --------
        // Unity-API-free by design: the Unity wiring lives in GameLoop.cs (the
        // excluded shell); these hooks keep the headless build honest and let the
        // values stay owned by A-015 (store), A-016 (monetization/IAP), A-017
        // (privacy/analytics). Monetization never blocks core-game completion
        // (PLAN hard rule): every slot below is inert when unset.

        /// <summary>
        /// SG-7 (A-020 §8): first-party crash reporter feeding A-016's crash
        /// event. The Unity shell installs the handler at boot; the off-state
        /// completion test belongs to the SHIPPED build and is recorded NOT
        /// EXECUTABLE until one exists (PLAN.md §4 A-023).
        /// </summary>
        public static bool crashReporterInstalled;

        public static void setCrashReporter(bool installed)
        {
            crashReporterInstalled = installed;
        }

        /// <summary>
        /// Unity Ads / IAP request slot (A-016 owns placement and values).
        /// Inert when unset; a missing ad never fails a run.
        /// </summary>
        public static void requestAdSlot(string slotId)
        {
            // no-op slot: A-016 fills the monetization surface; the core game
            // completes with this never called (PLAN hard rule).
        }

        /// <summary>
        /// Analytics event slot (A-017's event list; privacy text is A-017/A-024).
        /// Inert when unset.
        /// </summary>
        public static void logAnalyticsEvent(string eventId)
        {
            // no-op slot.
        }
    }
}

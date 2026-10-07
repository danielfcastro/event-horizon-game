// A-021 Phase 1 prototype — Assets/Runtime/Bridge/Unity/GameLoop.cs
// A-019 §3 layout / §4 table / §11 entry points: GameLoop.cs (entry, accumulator,
// MAX_CATCHUP). `Start()` is the Unity entry: build SimState via LevelLoader, then
// call FixedStepDriver.step in the accumulator loop.
//
// *** CONTRACT-ONLY: NOT COMPILED IN THE HEADLESS BUILD ***
// This file is the Unity shipping entry. It is excluded from harness.csproj
// (brief §9: GameLoop/RenderLayer/UIRoot are authored per contract but NOT
// compiled headless — there is no Unity toolchain in this environment). The
// exclusion is the <Compile ... Exclude> clause in harness.csproj; if this file
// ever compiles headless, that clause has drifted and the headless build is
// wrong, not this file.
//
// A-019 §11 hard rule honoured here: "there is exactly one simulation loop and
// both entries reach it". GameLoop does NOT re-implement stepping, owns no
// accumulator of its own, and never calls SimStep directly. The accumulator,
// DT = 1/60, MAX_CATCHUP = 4 and the dropped-step rule live in
// FixedStepDriver.advance, which the harness reaches the same way
// (tools/harness/main.cs -> Commands.runLoop -> FixedStepDriver.step).
// Duplicating the loop here would let a divergence between the two entries
// mask a determinism bug while H-02 still passed.
//
// Pure-sim rule (A-019 §5/§6): nothing in this file feeds float/double into
// SimCore. The only float here is the render-only alpha handed to RenderLayer.

namespace EH
{
    // Unity MonoBehaviour lifecycle methods are instance members, so this is the
    // one non-static class in Bridge/Unity; the sim side never instantiates it.
    public class GameLoop
    {
        private SimState sim;
        private bool started;

        /// <summary>
        /// Unity entry (A-019 §11). Builds the SAME SimState the harness builds
        /// — LevelLoader.load of the single LevelTable entry — and starts the
        /// single shared fixed-step driver through PlatformBridge's startup gate
        /// (which delegates the DT/timeScale rejection to FixedStepDriver; this
        /// file owns no duplicate gate).
        /// </summary>
        public void Start()
        {
            // levelId and sessionSeed are a platform concern; phase 1 has exactly
            // one level and the LevelTable default seed, identical to the harness
            // invocation in A-019 §11 so both entries load a byte-identical state.
            sim = LevelLoader.load("p1-level-01", LevelTable.defaultSeed);
            PlatformBridge.setBuildTarget("desktop");
            PlatformBridge.configureStartup();
            InputAdapter.boot(64);
            started = true;
        }

        /// <summary>
        /// One Unity frame. The intent is sampled ONCE per step boundary through
        /// InputAdapter (transport only — it holds no input scheme of its own,
        /// A-019 §4), then the whole frame budget goes to the ONE driver via
        /// PlatformBridge.advanceOneFrame, which is a straight call to
        /// FixedStepDriver.advance(s, frameWallDelta(), i). advance() runs at
        /// most MAX_CATCHUP steps and drops further debt into s.droppedSteps;
        /// GameLoop never interpolates, cushions, or softens.
        /// </summary>
        public void Update()
        {
            if (!started)
            {
                return;
            }

            Intent i = InputAdapter.sampleIntent(sim.stepIndex);
            PlatformBridge.advanceOneFrame(sim, i);

            // render-only interpolation between the last two fixed steps; never
            // fed back into the sim (A-019 §4 RenderLayer row: alpha is render
            // only). accumulator/DT in [0,1).
            float alpha = (float)sim.accumulator / (float)FixedStepDriver.DT;
            RenderLayer.draw(sim, alpha);
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

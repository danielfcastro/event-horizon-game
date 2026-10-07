// A-021 Phase 1 prototype — Assets/Runtime/SimCore/FixedStepDriver.cs
// A-019 §4: implemented. `void step(SimState s, long stepIndex, Intent i)`;
// owns DT = 1/60, the accumulator, MAX_CATCHUP = 4, and the dropped-step rule.
//
// A-019 §10 weak-device knob: dev.targetFps (60/30) and dev.timeScale (1.0)
// scale WALL-CLOCK advance into the accumulator, never DT. FixedStepDriver
// asserts DT is the constant 1/60 every step and rejects a timeScale that
// implies another DT at startup. A-019 §12 item 3: the shipping gate for
// dev.targetFps is an A-020 ship question — phase 1 keeps the knob here and
// builds NO second simulation path.
//
// S-09 (A-013): fixed DT steps exact — the accumulator is Q32.32 so step
// boundaries are exact rationals of 1/60 s.
// Pure C#, zero Unity API, no float/double.

namespace EH
{
    public static class FixedStepDriver
    {
        // DT = 1/60 s in Q32.32 raw: floor(2^32 / 60) = 71582788.
        // The driver treats this exact raw constant as the contract DT; the
        // assert below pins it every step (A-019 §10).
        public static readonly long DT = 71582788L;
        public static readonly int MAX_CATCHUP = 4;

        /// <summary>
        /// Startup gate (A-019 §10): reject a timeScale that would imply a DT
        /// other than the constant 1/60. timeScale scales wall-clock advance
        /// into the accumulator only; any value other than exactly 1.0 raw
        /// (ONE) is refused in phase 1 because it would change effective DT.
        /// dev.targetFps (60/30) is accepted here but only ever feeds the
        /// accumulator budget, never DT.
        /// </summary>
        public static void configureStartup(long timeScaleRaw, int targetFps)
        {
            Fail.check(timeScaleRaw == FixedQ.ONE, "FixedStepDriver.configureStartup: timeScale implies another DT (phase 1 accepts 1.0 only)");
            Fail.check(targetFps == 60 || targetFps == 30, "FixedStepDriver.configureStartup: targetFps must be 60 or 30");
        }

        /// <summary>
        /// Execute exactly one simulation step at stepIndex with the intent
        /// sampled for that step (A-019 §11: GameLoop and harness both reach
        /// this single loop). Asserts DT constant every step.
        /// </summary>
        public static void step(SimState s, long stepIndex, Intent i)
        {
            Fail.check(s.dt == DT, "FixedStepDriver.step: DT must be the constant 1/60 every step");
            Fail.check(stepIndex == s.stepIndex, "FixedStepDriver.step: stepIndex must equal state.stepIndex");
            Fail.check(s.intent.thrustX == i.thrustX && s.intent.thrustY == i.thrustY && s.intent.rate == i.rate,
                "FixedStepDriver.step: state.intent must equal the sampled intent");

            SimStep.run(s, stepIndex, i);

            s.stepIndex += 1;
        }

        /// <summary>
        /// Advance the accumulator by a wall-clock delta (Q32.32 seconds,
        /// already scaled by dev.timeScale/targetFps by the caller — the knob
        /// never touches DT). Runs at most MAX_CATCHUP steps; any further
        /// debt is dropped via the dropped-step rule (steps counted into
        /// s.droppedSteps, never simulated late, never softened).
        /// Returns the number of steps actually executed.
        /// </summary>
        public static int advance(SimState s, long wallDeltaRaw, Intent i)
        {
            Fail.check(wallDeltaRaw >= 0, "FixedStepDriver.advance: negative wall delta (hard-assert)");

            s.accumulator = addChecked(s.accumulator, wallDeltaRaw);

            int executed = 0;
            while (s.accumulator >= DT)
            {
                if (executed >= MAX_CATCHUP)
                {
                    // dropped-step rule: one step's debt is consumed per
                    // hitching frame beyond the catch-up budget; the sim
                    // never runs more than MAX_CATCHUP steps per advance and
                    // never interpolates or cushions.
                    s.accumulator -= DT;
                    s.droppedSteps += 1;
                    break;
                }
                step(s, s.stepIndex, i);
                s.accumulator -= DT;
                executed += 1;
            }
            return executed;
        }

        static long addChecked(long a, long b)
        {
            return FixedQ.add(a, b);
        }
    }
}

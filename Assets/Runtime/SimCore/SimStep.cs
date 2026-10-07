// A-021 Phase 1 prototype — Assets/Runtime/SimCore/SimStep.cs
// A-019 §4: SimStep fixes the system order ONCE:
//   PlayerController -> AttractionSystem -> AbsorptionSystem -> GrowthSystem
//   -> HazardSystem -> StabilitySystem -> ComboSystem -> ScoringSystem
// each system iterates over ASCENDING body index (determinism, A-005).
//
// This is the single simulation step body. FixedStepDriver.step (file 7) calls
// run(); GameLoop (Bridge, not compiled headless) and tools/harness/main.cs
// both reach the same step (A-019 §11: exactly one simulation loop).
//
// Signatures fixed here (stage 2 implements them exactly):
//   PlayerController.apply(SimState s, Intent i)        — movement intent only
//   AttractionSystem.apply(SimState s)                  — A-005 formula verbatim
//   AbsorptionSystem.apply(SimState s) -> bool          — true if absorbed this step
//   GrowthSystem.apply(SimState s)                      — mass -> eventRadius
//   HazardSystem.applyInfluence(SimState s)             — stub no-op (phase 1)
//   HazardSystem.onContact(SimState s)                  — stub no-op (phase 1)
//   StabilitySystem.apply(SimState s)                   — stub slot, identity
//   ComboSystem.apply(SimState s, bool absorbedThisStep)— stub slot, identity
//   ScoringSystem.apply(SimState s)                     — stub slot, identity
//   ScoringSystem.score(SimState s) -> long             — stub slot, identity
//
// No-softening (A-021 acceptance criterion 6): stubs return identity; they
// never apply aim assists, magnets, catch-up, mass buffers, undo, auto-absorb,
// or difficulty scaling.
// Pure C#, zero Unity API, no float/double.

namespace EH
{
    public static class SimStep
    {
        /// <summary>
        /// One fixed simulation step. Pure function of (state, stepIndex,
        /// intent): the state is mutated in place, stepIndex is the step
        /// ordinal, intent was sampled once for this step (quantized to steps,
        /// A-019 §4). No wall clock, no Unity API, no unordered iteration.
        /// </summary>
        public static void run(SimState s, long stepIndex, Intent intent)
        {
            // stepIndex is owned by FixedStepDriver; assert monotonicity here so
            // every subsystem can trust it.
            Fail.check(stepIndex == s.stepIndex, "SimStep.run: stepIndex disagrees with state.stepIndex");

            // 1) PlayerController — black hole is body index 0; movement intent only
            s.intent = intent;
            PlayerController.apply(s);

            // 2) AttractionSystem — A-005 formula verbatim, attractor-index order
            AttractionSystem.apply(s);

            // 3) AbsorptionSystem — distance < absorptionRadius, once per step;
            //    bodies do not collide with each other (A-019 §4)
            uint absorbedBefore = s.absorbedTotal;
            AbsorptionSystem.apply(s);
            bool absorbedThisStep = s.absorbedTotal != absorbedBefore;

            // 4) GrowthSystem — mass -> eventRadius = baseRadius + a*isqrt(mass)
            GrowthSystem.apply(s);

            // 5) HazardSystem — stub no-ops in phase 1 (A-019 §4: stub)
            HazardSystem.applyInfluence(s);
            HazardSystem.onContact(s);

            // 6) StabilitySystem — stub slot, identity (A-007 fills values later)
            StabilitySystem.apply(s);

            // 7) ComboSystem — stub slot, identity; receives absorb flag only
            ComboSystem.apply(s, absorbedThisStep);

            // 8) ScoringSystem — stub slot, identity
            ScoringSystem.apply(s);
            s.score = ScoringSystem.score(s);

            // bookkeeping counters (snapshot §7 counters block)
            s.stepsRun += 1;
        }
    }
}

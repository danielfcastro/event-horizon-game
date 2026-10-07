// A-021 Phase 1 prototype — Assets/Runtime/SimCore/Scoring.cs
// A-019 §4: ScoringSystem is a stub slot in phase 1 (A-007 fills values later).
// Signatures fixed by SimStep.cs:
//   ScoringSystem.apply(SimState s)        — stub slot, identity
//   ScoringSystem.score(SimState s) -> long — stub slot, returns 0
// Stub returns identity / zero slot value; no cushioning default, no softening
// (A-021 acceptance criterion 6).
// Pure C#, zero Unity API, no float/double.

namespace EH
{
    public static class ScoringSystem
    {
        /// <summary>
        /// Stub slot: scoring rules. Phase 1: identity — does not read or
        /// write state.score. Values arrive with A-007 later.
        /// </summary>
        public static void apply(SimState s)
        {
            // slot only: identity — state unchanged.
        }

        /// <summary>
        /// Stub slot: score readout. Phase 1: returns 0 (slot only).
        /// </summary>
        public static long score(SimState s)
        {
            return 0;
        }
    }
}

// A-021 Phase 1 prototype — Assets/Runtime/SimCore/Stability.cs
// A-019 §4: StabilitySystem is a stub slot in phase 1 (A-007 fills values
// later). Signature fixed by SimStep.cs: StabilitySystem.apply(SimState s).
// Stub returns identity: state.stability is left untouched; no cushioning
// default, no softening (A-021 acceptance criterion 6).
// Pure C#, zero Unity API, no float/double.

namespace EH
{
    public static class StabilitySystem
    {
        /// <summary>
        /// Stub slot: stability rules. Phase 1: identity — does not read or
        /// write state.stability. Values arrive with A-007 later.
        /// </summary>
        public static void apply(SimState s)
        {
            // slot only: identity — state unchanged.
        }
    }
}

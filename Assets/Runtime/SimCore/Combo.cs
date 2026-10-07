// A-021 Phase 1 prototype — Assets/Runtime/SimCore/Combo.cs
// A-019 §4: ComboSystem is a stub slot in phase 1 (A-007 fills values later).
// Signature fixed by SimStep.cs: ComboSystem.apply(SimState s, bool absorbedThisStep).
// Stub returns identity: state.combo is left untouched; no cushioning default,
// no softening (A-021 acceptance criterion 6).
// Pure C#, zero Unity API, no float/double.

namespace EH
{
    public static class ComboSystem
    {
        /// <summary>
        /// Stub slot: combo rules. Phase 1: identity — the absorb flag is
        /// received but unused; state.combo is not read or written.
        /// </summary>
        public static void apply(SimState s, bool absorbedThisStep)
        {
            // slot only: identity — state unchanged.
        }
    }
}

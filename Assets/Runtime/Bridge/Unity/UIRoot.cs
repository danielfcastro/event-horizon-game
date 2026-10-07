// A-021 Phase 1 prototype — Assets/Runtime/Bridge/Unity/UIRoot.cs
// A-019 §3 layout / §4 table: UIRoot.cs (HUD slots, no layout) — status "stub
// slots": HUD slot REGISTRATION only, no layout (A-010 owns layout, sizing,
// type, contrast, and the mass-meter design).
//
// *** CONTRACT-ONLY: NOT COMPILED IN THE HEADLESS BUILD ***
// Excluded from harness.csproj by the <Compile ... Exclude> clause (brief §9).
// Uses the Unity UI API. Authored so the shipping target has the slots the
// harness assumes exist; the headless build never touches them.
//
// Why registration-only matters: A-021 acceptance is the headless sim (H-01/H-02).
// Inventing layout, fonts, or anchor math here would put phase-1 code in charge
// of a decision A-010 owns, and later artifacts would have to hunt for it. The
// slots are named, ordered, and empty.
//
// Slots carry NO values in phase 1: Stability/Combo/Scoring are stub slots with
// no values (A-019 §4, A-007 fills them), so the HUD registers the slot and
// marks it valueless rather than drawing a fake number.

namespace EH
{
    public static class UIRoot
    {
        // Registration order is fixed and append-only, so A-010 can fill slots
        // without renumbering existing ones (same discipline A-019 §7 requires
        // of the snapshot field order).
        public static readonly string[] slotNames =
        {
            "massMeter",        // A-010 owns layout; reads holeMass (implemented)
            "targetMass",       // A-010 owns layout; reads LevelTable.targetMass
            "stability",        // stub slot: no value in phase 1 (A-007 fills)
            "combo",            // stub slot: no value in phase 1 (A-007 fills)
            "score",            // stub slot: no value in phase 1 (A-007 fills)
        };

        public static readonly bool[] slotHasValue =
        {
            true,   // massMeter
            true,   // targetMass
            false,  // stability  (stub, no value — never drawn as 0)
            false,  // combo      (stub, no value)
            false,  // score      (stub, no value)
        };

        /// <summary>
        /// Register the HUD slots. No layout, no sizing, no anchors, no fonts,
        /// no contrast decisions — all of that is A-010 (brief §9 scope
        /// boundary). A slot with slotHasValue == false is registered but not
        /// drawn, so a phase-1 player never sees a fabricated number.
        /// </summary>
        public static void register()
        {
            for (int k = 0; k < slotNames.Length; k++)
            {
                UnityEngine.Canvas.registerSlot(slotNames[k], slotHasValue[k]);
            }
        }

        /// <summary>
        /// Named absences (so later artifacts find the slot instead of inventing
        /// a parallel one): mass-meter layout, pause/menu screens, and the
        /// accessibility surface (large-text, high-contrast, reduced-motion) are
        /// A-010 and A-012 owned. Phase 1 registers nothing for them.
        /// </summary>
        public static void registerAbsentPhase1()
        {
            // no-op: phase 1 ships no menu, no pause screen, no a11y presets.
        }
    }
}

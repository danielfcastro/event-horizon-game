// A-023 Unity player package — Assets/Runtime/Bridge/Unity/UIRoot.cs
// Upgraded from the A-021 "stub slots" contract to the player-facing UI root:
// HUD slot REGISTRATION on a real Unity UI Canvas. Still no layout, no sizing,
// no anchors, no fonts, no contrast decisions — A-010 owns all of that (brief
// §9 scope boundary: "HUD layout and menus: A-010").
//
// *** NOT COMPILED IN THE HEADLESS BUILD (excluded from harness.csproj and
// player.csproj by their Exclude clauses) *** Uses the Unity UI API. Authored
// against the Unity 6 LTS 6000.0.84f1 API; NOT compiled or run here — the
// license gate (PLAN.md §4 A-023) blocks any Unity-side compile until sign-in.
//
// Why registration-only matters (unchanged from A-021): A-023's acceptance is
// the player package over the same SimCore, not a UI design. Inventing layout
// here would put this artifact in charge of a decision A-010 owns. The slots
// are named, ordered, and empty.
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

        private static UnityEngine.GameObject canvasRoot;
        private static bool registered;

        /// <summary>
        /// Register the HUD slots on a screen-space Canvas. No layout, no sizing,
        /// no anchors, no fonts, no contrast decisions — all of that is A-010
        /// (brief §9 scope boundary). A slot with slotHasValue == false is
        /// registered but not drawn, so a phase-1 player never sees a fabricated
        /// number.
        /// </summary>
        public static void register()
        {
            Fail.check(!registered, "UIRoot.register: slots exist to be registered once");
            canvasRoot = new UnityEngine.GameObject("eh-hud");
            canvasRoot.AddComponent<UnityEngine.Canvas>();
            for (int k = 0; k < slotNames.Length; k++)
            {
                UnityEngine.GameObject slot = new UnityEngine.GameObject("eh-hud-" + slotNames[k]);
                // Parenting is a Transform operation (GameObject has no SetParent,
                // and SetParent takes a Transform, not a GameObject).
                slot.GetComponent<UnityEngine.Transform>().SetParent(canvasRoot.GetComponent<UnityEngine.Transform>());
                // Every slot is an empty Transform: no Text component is created
                // for any slot, value-bearing or not — A-010 owns the drawing,
                // so nothing here can render a fabricated number.
            }
            registered = true;
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

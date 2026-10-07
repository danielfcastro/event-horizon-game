// A-021 Phase 1 prototype — Assets/Runtime/SimCore/HazardSystem.cs
// A-019 §4: HazardSystem is a stub in phase 1 (hazard pool exists, empty).
// Signatures fixed by SimStep.cs (stage 2 implements them exactly):
//   HazardSystem.applyInfluence(SimState s)  — stub no-op
//   HazardSystem.onContact(SimState s)       — stub no-op
// No-softening (A-021 acceptance criterion 6): no-op bodies mutate nothing;
// they never apply aim assists, magnets, catch-up, mass buffers, undo,
// auto-absorb, or difficulty scaling.
// Pure C#, zero Unity API, no float/double.

namespace EH
{
    public static class HazardSystem
    {
        /// <summary>
        /// Stub: hazard influence on bodies. Phase 1: no-op (hazard pool is
        /// empty; A-019 §4 stub rule). Mutates nothing.
        /// </summary>
        public static void applyInfluence(SimState s)
        {
            // no-op: phase 1 has no hazards. Identity — state unchanged.
        }

        /// <summary>
        /// Stub: hazard contact resolution. Phase 1: no-op. Mutates nothing.
        /// </summary>
        public static void onContact(SimState s)
        {
            // no-op: phase 1 has no hazards. Identity — state unchanged.
        }
    }
}

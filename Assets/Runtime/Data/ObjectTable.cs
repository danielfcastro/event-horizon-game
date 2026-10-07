// A-021 Phase 1 prototype — Assets/Runtime/Data/ObjectTable.cs
// Generated C# constants (A-019 §3 Data/, §12 item 2: keep tables as generated
// constants — numbers identical, never renumbered).
//
// Entry shape (A-019 §3): {typeId, category, mass, radius, absorbable, hazardFlags}.
// API pinned by SpawnDirector.cs (do not change):
//   category(int typeId)   -> 0 small / 1 medium / 2 large (BodyPool tier)
//   mass(int typeId)       -> integer mass (SpawnDirector stores it << 32 = Q32.32)
//   radius(int typeId)     -> integer WU  (SpawnDirector stores it << 32 = Q32.32)
//   absorbable(int typeId) -> gates AbsorptionSystem (bit1 of bodyFlags)
//   hazard(int typeId)     -> hazardFlags != 0; routes to hazard pool
//
// Masses are A-008 catalog values (brief §6), phase-1 set only:
//   Dust 3–5, Ice fragment 4–10, Pebble 15–24, Small debris 20–40,
//   Rock 60–90, Asteroid 90–150.  Canonical numbers, never renumbered.
// Category tiers per brief §5 / BodyPool budgets (small<=220 / medium<=56 /
// large<=24): Dust + Ice fragment = small, Pebble + Small debris = medium,
// Rock + Asteroid = large.
// All absorbable = true, hazardFlags = 0 in phase 1 (brief §7: no hazards).
// Radii are integer WU (A-019 §6: 1 WU = starting hole radius); PLACEHOLDER-
// adjacent visual tuning is render-side, SimCore only needs positive WU.
// Pure C#, zero Unity API, no float/double.

namespace EH
{
    public static class ObjectTable
    {
        // ---- per-type constants (typeId 1..6; 0 = no type) -------------------
        // typeId | category | mass | radius(WU) | absorbable | hazardFlags
        //   1      0 small      4        1          true         0   Dust
        //   2      0 small      7        1          true         0   Ice fragment
        //   3      1 medium    20        2          true         0   Pebble
        //   4      1 medium    30        2          true         0   Small debris
        //   5      2 large     75        3          true         0   Rock
        //   6      2 large    120        3          true         0   Asteroid

        public static int category(int typeId)
        {
            switch (typeId)
            {
                case 1: return 0; // Dust — small tier
                case 2: return 0; // Ice fragment — small tier
                case 3: return 1; // Pebble — medium tier
                case 4: return 1; // Small debris — medium tier
                case 5: return 2; // Rock — large tier
                case 6: return 2; // Asteroid — large tier
                default: return -1; // SpawnDirector hard-asserts 0/1/2
            }
        }

        public static long mass(int typeId)
        {
            switch (typeId)
            {
                case 1: return 4L;   // Dust, A-008 range 3–5
                case 2: return 7L;   // Ice fragment, A-008 range 4–10
                case 3: return 20L;  // Pebble, A-008 range 15–24
                case 4: return 30L;  // Small debris, A-008 range 20–40
                case 5: return 75L;  // Rock, A-008 range 60–90
                case 6: return 120L; // Asteroid, A-008 range 90–150
                default: return 0L;  // SpawnDirector hard-asserts mass > 0
            }
        }

        public static long radius(int typeId)
        {
            switch (typeId)
            {
                case 1: return 1L;
                case 2: return 1L;
                case 3: return 2L;
                case 4: return 2L;
                case 5: return 3L;
                case 6: return 3L;
                default: return 0L; // SpawnDirector hard-asserts radius > 0
            }
        }

        public static bool absorbable(int typeId)
        {
            // phase 1: every catalog body is absorbable (brief §7)
            return typeId >= 1 && typeId <= 6;
        }

        public static bool hazard(int typeId)
        {
            // hazardFlags == 0 for the whole phase-1 set (brief §7: no hazards)
            return false;
        }
    }
}

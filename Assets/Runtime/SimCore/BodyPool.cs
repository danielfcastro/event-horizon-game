// A-021 Phase 1 prototype — Assets/Runtime/SimCore/BodyPool.cs
// A-019 §4: implemented. SoA pool, capacity 300 with tier budgets
// small<=220 / medium<=56 / large<=24, plus a hazard pool <=24.
// alloc/free via an ASCENDING freeList (snapshot §7 emits it ascending);
// preallocated at boot, ZERO allocation in the hot loop (PLAN 5.16).
//
// Determinism rules honored here:
//  - alloc always takes the LOWEST free index (deterministic index assignment)
//  - freeList stays sorted ascending (binary-search insert)
//  - body index 0 is reserved for the black hole (A-019 §4 PlayerController)
//  - freed records are zeroed (inactive = zero record in EHSNAP1)
// Pure C#, zero Unity API, no float/double.

namespace EH
{
    public static class BodyPool
    {
        // category encoding (i8 in snapshot): 0 small, 1 medium, 2 large
        public static readonly int CAT_SMALL = 0;
        public static readonly int CAT_MEDIUM = 1;
        public static readonly int CAT_LARGE = 2;

        /// <summary>
        /// Boot the pool: every index except 0 (the hole) is free, ascending.
        /// Called once by LevelLoader/harness before any step.
        /// </summary>
        public static void boot(SimState s)
        {
            SimState.boot(s); // allocates the SoA arrays once
            s.freeCount = 0;
            for (int i = 1; i < SimState.BODY_CAPACITY; i++)
            {
                s.freeList[s.freeCount++] = i;
            }
            s.highmarkSmall = 0;
            s.highmarkMedium = 0;
            s.highmarkLarge = 0;
            s.highmarkHazard = 0;
        }

        /// <summary>
        /// Live bodies in a tier, derived by ascending scan (no extra state,
        /// no allocation; 300-entry scan only happens on alloc).
        /// </summary>
        public static int tierCount(SimState s, int category)
        {
            int n = 0;
            for (int i = 0; i < SimState.BODY_CAPACITY; i++)
            {
                // active gate: boot zero-fills the arrays and free() zeroes the
                // record, so an inactive slot reads category 0 (small); counting
                // it would inflate the small tier to 299 at boot and hard-assert
                // the first small spawn. "Live bodies in a tier" per the doc.
                if ((s.bodyFlags[i] & 0x01) != 0 && s.bodyCategory[i] == (byte)category)
                {
                    n++;
                }
            }
            return n;
        }

        /// <summary>
        /// Allocate the lowest free index for a body of `category` (0/1/2) with
        /// ObjectTable `typeId`. Tier budget exceeded or pool exhausted is a
        /// hard-assert, never a silent reuse (no-softening rule).
        /// Returns the body index.
        /// </summary>
        public static int alloc(SimState s, int category, int typeId)
        {
            Fail.check(category == CAT_SMALL || category == CAT_MEDIUM || category == CAT_LARGE,
                "BodyPool.alloc: unknown category");
            int cap = category == CAT_SMALL ? SimState.SMALL_CAPACITY
                  : category == CAT_MEDIUM ? SimState.MEDIUM_CAPACITY
                                            : SimState.LARGE_CAPACITY;
            Fail.check(tierCount(s, category) < cap, "BodyPool.alloc: tier budget exceeded");
            Fail.check(s.freeCount > 0, "BodyPool.alloc: pool exhausted (hard-assert, no reuse)");

            // lowest free index keeps index assignment deterministic
            int idx = s.freeList[0];
            for (int k = 1; k < s.freeCount; k++)
            {
                s.freeList[k - 1] = s.freeList[k];
            }
            s.freeCount--;

            s.bodyFlags[idx] = 0x01; // bit0 active
            s.bodyCategory[idx] = (byte)category;
            s.bodyTypeId[idx] = (byte)typeId;
            s.bodyX[idx] = 0;
            s.bodyY[idx] = 0;
            s.bodyVX[idx] = 0;
            s.bodyVY[idx] = 0;
            s.bodyMass[idx] = 0;
            s.bodyRadius[idx] = 0;

            // highmark = peak live bodies per tier (snapshot §7 counters)
            int live = tierCount(s, category);
            if (category == CAT_SMALL && live > s.highmarkSmall)
            {
                s.highmarkSmall = live;
            }
            else if (category == CAT_MEDIUM && live > s.highmarkMedium)
            {
                s.highmarkMedium = live;
            }
            else if (category == CAT_LARGE && live > s.highmarkLarge)
            {
                s.highmarkLarge = live;
            }
            return idx;
        }

        /// <summary>
        /// Release a body index; freeList re-inserts it in ascending order
        /// (binary-search insert). Index 0 (the hole) can never be freed.
        /// The record is zeroed so EHSNAP1 emits an inactive zero record.
        /// </summary>
        public static void free(SimState s, int idx)
        {
            Fail.check(idx > 0 && idx < SimState.BODY_CAPACITY, "BodyPool.free: index out of pool range");
            Fail.check((s.bodyFlags[idx] & 0x01) != 0, "BodyPool.free: index not active");

            s.bodyFlags[idx] = 0;
            s.bodyCategory[idx] = 0;
            s.bodyTypeId[idx] = 0;
            s.bodyX[idx] = 0;
            s.bodyY[idx] = 0;
            s.bodyVX[idx] = 0;
            s.bodyVY[idx] = 0;
            s.bodyMass[idx] = 0;
            s.bodyRadius[idx] = 0;

            // ascending insert
            int lo = 0;
            int hi = s.freeCount;
            while (lo < hi)
            {
                int mid = (lo + hi) >> 1;
                if (s.freeList[mid] < idx)
                {
                    lo = mid + 1;
                }
                else
                {
                    hi = mid;
                }
            }
            for (int k = s.freeCount; k > lo; k--)
            {
                s.freeList[k] = s.freeList[k - 1];
            }
            s.freeList[lo] = idx;
            s.freeCount++;
        }

        /// <summary>
        /// Hazard pool alloc (capacity 24, ascending scan — deterministic,
        /// no extra state). Empty in phase 1 (A-009: no hazards on p1-level-01);
        /// present so HazardSystem can be wired without allocation later.
        /// </summary>
        public static int allocHazard(SimState s, int typeId)
        {
            for (int i = 0; i < SimState.HAZARD_CAPACITY; i++)
            {
                if ((s.hazardFlags[i] & 0x01) == 0)
                {
                    s.hazardFlags[i] = 0x01;
                    s.hazardTypeId[i] = (byte)typeId;
                    if (liveHazards(s) > s.highmarkHazard)
                    {
                        s.highmarkHazard = liveHazards(s);
                    }
                    return i;
                }
            }
            Fail.fail("BodyPool.allocHazard: hazard pool exhausted (hard-assert)");
            return -1;
        }

        public static void freeHazard(SimState s, int idx)
        {
            Fail.check(idx >= 0 && idx < SimState.HAZARD_CAPACITY, "BodyPool.freeHazard: index out of range");
            s.hazardFlags[idx] = 0;
            s.hazardCategory[idx] = 0;
            s.hazardTypeId[idx] = 0;
            s.hazardX[idx] = 0;
            s.hazardY[idx] = 0;
            s.hazardVX[idx] = 0;
            s.hazardVY[idx] = 0;
            s.hazardMass[idx] = 0;
            s.hazardRadius[idx] = 0;
        }

        public static int liveHazards(SimState s)
        {
            int n = 0;
            for (int i = 0; i < SimState.HAZARD_CAPACITY; i++)
            {
                if ((s.hazardFlags[i] & 0x01) != 0)
                {
                    n++;
                }
            }
            return n;
        }

        public static bool isActive(SimState s, int idx)
        {
            return (s.bodyFlags[idx] & 0x01) != 0;
        }
    }
}

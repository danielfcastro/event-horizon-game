// A-021 Phase 1 prototype — Assets/Runtime/SimCore/SpawnDirector.cs
// A-019 §4: stub level — replays the FIXED LevelTable.spawnEvents list
// (step, typeId, x, y) in ascending step order; consumes NO PRNG draws
// (determinism: the spawn stream is a pure constant table, so S-01/S-05
// replay equality never depends on the xoshiro stream here). Per-absorb
// respawn (A-007 §3.2) is Phase 3 content — out of scope (brief §9).
//
// Pool rule: every spawn allocates via BodyPool.alloc, which hard-asserts
// the tier budgets (small<=220 / medium<=56 / large<=24) and total capacity
// (300) — "assert pool capacity not exceeded" is enforced there plus the
// table-size precheck below. No silent drop, no reuse (no-softening).
//
// Data-table contract this file expects (the Data stage must match exactly):
//   LevelTable.cs   : public static int spawnCount;
//                     public static long[] spawnStep;   // stepIndex, ascending
//                     public static byte[] spawnTypeId; // ObjectTable typeId
//                     public static long[] spawnX;      // Q32.32 raw WU
//                     public static long[] spawnY;      // Q32.32 raw WU
//   ObjectTable.cs  : public static int category(int typeId);  // 0/1/2 tier
//                     public static long mass(int typeId);     // integer mass
//                     public static long radius(int typeId);   // integer WU
//                     public static bool absorbable(int typeId);
//                     public static bool hazard(int typeId);   // hazardFlags
// Pure C#, zero Unity API, no float/double, Q32.32 raw int64 (A-019 §5).

namespace EH
{
    public static class SpawnDirector
    {
        /// <summary>
        /// Called by SimStep before AttractionSystem so a body spawned this
        /// step is eligible for attraction/absorption checks in the same step.
        /// Stateless scan of the fixed table: the list is short (phase-1 level
        /// needs ~25-40 absorbs worth of bodies, brief §7) and the scan is a
        /// pure read of generated constants — zero allocation, zero PRNG.
        /// </summary>
        public static void apply(SimState s)
        {
            int n = LevelTable.spawnCount;
            Fail.check(n >= 0 && n < SimState.BODY_CAPACITY,
                "SpawnDirector: spawn table exceeds body pool capacity (hard-assert)");

            // ascending-step order is a table invariant; assert it every call
            // (cheap, pure) so an out-of-order table can never silently
            // reorder the deterministic spawn stream.
            for (int k = 1; k < n; k++)
            {
                Fail.check(LevelTable.spawnStep[k] >= LevelTable.spawnStep[k - 1],
                    "SpawnDirector: spawnEvents not in ascending step order");
            }

            for (int k = 0; k < n; k++)
            {
                if (LevelTable.spawnStep[k] != s.stepIndex)
                {
                    continue;
                }

                int typeId = LevelTable.spawnTypeId[k] & 0xFF;

                // hazard spawns go to the hazard pool; phase-1 p1-level-01 has
                // no hazards (brief §7), so this branch stays cold — present so
                // the wiring exists without hot-loop allocation later.
                if (ObjectTable.hazard(typeId))
                {
                    int h = BodyPool.allocHazard(s, typeId);
                    s.hazardX[h] = LevelTable.spawnX[k];
                    s.hazardY[h] = LevelTable.spawnY[k];
                    s.hazardMass[h] = ObjectTable.mass(typeId) << 32;
                    s.hazardRadius[h] = ObjectTable.radius(typeId) << 32;
                    continue;
                }

                int cat = ObjectTable.category(typeId);
                Fail.check(cat == BodyPool.CAT_SMALL || cat == BodyPool.CAT_MEDIUM
                           || cat == BodyPool.CAT_LARGE,
                    "SpawnDirector: ObjectTable category invalid");

                // BodyPool.alloc hard-asserts tier budget + capacity + lowest
                // free index (deterministic assignment); zero hot-loop alloc.
                int idx = BodyPool.alloc(s, cat, typeId);

                // positions are Q32.32 raw in the table (authored via FixedQ)
                s.bodyX[idx] = LevelTable.spawnX[k];
                s.bodyY[idx] = LevelTable.spawnY[k];

                // integer mass/radius stored Q32.32 raw; static debris (phase-1
                // spawnEvents carry no velocity — attractor physics moves them)
                long m = ObjectTable.mass(typeId);
                long r = ObjectTable.radius(typeId);
                Fail.check(m > 0L && m <= 800000L, "SpawnDirector: mass out of A-007 tier range");
                Fail.check(r > 0L, "SpawnDirector: radius non-positive");
                s.bodyMass[idx] = m << 32;
                s.bodyRadius[idx] = r << 32;

                // absorbable flag (bit1) — AbsorptionSystem gates on it
                if (ObjectTable.absorbable(typeId))
                {
                    s.bodyFlags[idx] |= 0x02;
                }

                // consumes NO PRNG draws: nothing here touches s.s0..s3
            }
        }
    }
}

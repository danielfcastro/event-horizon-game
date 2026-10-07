// A-021 Phase 1 prototype — Assets/Runtime/SimCore/SpatialHash.cs
// A-019 §4: implemented. Uniform grid, counting sort, ASCENDING-INDEX query
// sets; cellSize = sqrt(worldArea / N) asserted 1..32 WU (A-019 §4).
//
// Zero hot-loop allocation: grid arrays are preallocated at boot; build()
// reuses them with a counting sort over ascending body indices, which keeps
// every cell's member list ascending. queryAscending() emits candidates in
// ascending body-index order (determinism: no unordered-iteration reads).
// Pure C#, zero Unity API, no float/double.

namespace EH
{
    public static class SpatialHash
    {
        /// <summary>
        /// Boot the grid from world bounds. cellSize = sqrt(worldArea / N) with
        /// N = BODY_CAPACITY (stable across the run; live-count-based N would
        /// rebuild the grid mid-run and is out of scope for phase 1).
        /// Asserted 1..32 WU per A-019 §4.
        /// </summary>
        public static void boot(SimState s)
        {
            Fail.check(s.boundsW > 0 && s.boundsH > 0, "SpatialHash.boot: world bounds must be positive");

            // area = boundsW * boundsH in Q32.32 raw (muldiv keeps intermediates in range)
            long area = MulDiv.muldiv(s.boundsW, s.boundsH, FixedQ.ONE);
            // area / N as Q32.32 raw
            long areaPerCell = MulDiv.muldiv(area, FixedQ.ONE, (long)SimState.BODY_CAPACITY << 32);
            long cell = ISqrtQ.isqrt(areaPerCell);

            Fail.check(cell >= FixedQ.ONE && cell <= 32L * FixedQ.ONE,
                "SpatialHash.boot: cellSize outside 1..32 WU (A-019 §4 assert)");

            s.cellSize = cell;
            s.gridW = (int)((s.boundsW + cell - 1) / cell);
            s.gridH = (int)((s.boundsH + cell - 1) / cell);
            int cells = s.gridW * s.gridH;
            Fail.check(cells > 0 && cells <= 4096, "SpatialHash.boot: grid too large");

            s.cellCount = new int[cells];
            s.cellPrefix = new int[cells + 1];
            s.sortedBodies = new int[SimState.BODY_CAPACITY];
            s.bodyCell = new int[SimState.BODY_CAPACITY];
        }

        /// <summary>
        /// Rebuild the grid for the current positions via counting sort over
        /// ASCENDING body index (stable: each cell's member list stays
        /// ascending). Called once per step by the driver before systems that
        /// query it; reuses preallocated arrays, zero allocation.
        /// </summary>
        public static void build(SimState s)
        {
            int cells = s.gridW * s.gridH;
            for (int c = 0; c < cells; c++)
            {
                s.cellCount[c] = 0;
            }

            // assign cells (clamp to grid; out-of-bounds bodies still bucket)
            for (int i = 0; i < SimState.BODY_CAPACITY; i++)
            {
                if ((s.bodyFlags[i] & 0x01) == 0)
                {
                    s.bodyCell[i] = -1;
                    continue;
                }
                int cx = (int)(s.bodyX[i] / s.cellSize);
                int cy = (int)(s.bodyY[i] / s.cellSize);
                if (cx < 0)
                {
                    cx = 0;
                }
                if (cx >= s.gridW)
                {
                    cx = s.gridW - 1;
                }
                if (cy < 0)
                {
                    cy = 0;
                }
                if (cy >= s.gridH)
                {
                    cy = s.gridH - 1;
                }
                int cell = cy * s.gridW + cx;
                s.bodyCell[i] = cell;
                s.cellCount[cell]++;
            }

            // prefix offsets
            s.cellPrefix[0] = 0;
            for (int c = 0; c < cells; c++)
            {
                s.cellPrefix[c + 1] = s.cellPrefix[c] + s.cellCount[c];
            }

            // counting sort, ascending index order (stable within cells)
            // reuse cellPrefix tail as running offsets without extra arrays
            for (int i = 0; i < SimState.BODY_CAPACITY; i++)
            {
                int cell = s.bodyCell[i];
                if (cell < 0)
                {
                    continue;
                }
                int slot = s.cellPrefix[cell]++;
                s.sortedBodies[slot] = i;
            }
            // restore prefix offsets (cellPrefix[c] now points past cell c's end)
            for (int c = cells; c >= 1; c--)
            {
                s.cellPrefix[c] = s.cellPrefix[c - 1];
            }
            // cellPrefix[c]..cellPrefix[c+1] is cell c's range in sortedBodies
        }

        /// <summary>
        /// Query bodies whose cell overlaps the AABB (x±r, y±r), then emit
        /// candidates in ASCENDING body-index order via visitor. The mark pass
        /// uses a monotonic stamp (no allocation, no state mutation visible to
        /// systems). Distance filtering is the caller's job (systems own their
        /// own radius semantics).
        /// Returns the number of candidates emitted.
        /// </summary>
        public static int queryAscending(SimState s, long x, long y, long r, System.Func<int, System.Object> visitor)
        {
            Fail.check(r >= 0, "SpatialHash.queryAscending: negative radius (hard-assert)");

            int minX = (int)((x - r) / s.cellSize);
            int maxX = (int)((x + r) / s.cellSize);
            int minY = (int)((y - r) / s.cellSize);
            int maxY = (int)((y + r) / s.cellSize);
            if (minX < 0)
            {
                minX = 0;
            }
            if (minY < 0)
            {
                minY = 0;
            }
            if (maxX >= s.gridW)
            {
                maxX = s.gridW - 1;
            }
            if (maxY >= s.gridH)
            {
                maxY = s.gridH - 1;
            }

            s.queryStamp++;
            int marked = 0;
            for (int cy = minY; cy <= maxY; cy++)
            {
                int rowBase = cy * s.gridW;
                for (int cx = minX; cx <= maxX; cx++)
                {
                    int cell = rowBase + cx;
                    int from = s.cellPrefix[cell];
                    int to = s.cellPrefix[cell + 1];
                    for (int k = from; k < to; k++)
                    {
                        int idx = s.sortedBodies[k];
                        if (s.queryMark[idx] != s.queryStamp)
                        {
                            s.queryMark[idx] = s.queryStamp;
                            marked++;
                        }
                    }
                }
            }

            // ascending-index emission
            int emitted = 0;
            for (int i = 0; i < SimState.BODY_CAPACITY; i++)
            {
                if (s.queryMark[i] == s.queryStamp)
                {
                    visitor(i);
                    emitted++;
                }
            }
            Fail.check(emitted == marked, "SpatialHash.queryAscending: mark/emit mismatch");
            return emitted;
        }
    }
}

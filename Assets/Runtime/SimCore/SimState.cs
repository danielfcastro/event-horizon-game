// A-021 Phase 1 prototype — Assets/Runtime/SimCore/SimState.cs
// A-019 §12 item 4: SimState stays ONE struct; §7 snapshot field order may only
// be appended to. Every subsystem is a pure function of (state, stepIndex,
// intent) — no wall clock, no Unity API, no unordered-iteration reads.
//
// All positional/kinetic/mass/force fields are Q32.32 raw int64 (A-019 §5).
// Field layout mirrors the EHSNAP1 snapshot order (A-019 §7) so the snapshot
// writer (stage 3) emits this struct's fields append-only.
// Pure C#, zero Unity API, no float/double.

namespace EH
{
    /// <summary>
    /// Intent channel value (A-019 §4 / §8): { Q thrustX, Q thrustY, Q rate }.
    /// No device identifiers, timestamps, or pointers (digest purity, S-family).
    /// thrustX/thrustY are Q32.32 direction components (unit-ish, normalized by
    /// PlayerController); rate is Q32.32 in [0,1] (5% quantized codes 0..20,
    /// code 10 = neutral 50%).
    /// </summary>
    public class Intent
    {
        public long thrustX; // Q32.32
        public long thrustY; // Q32.32
        public long rate;    // Q32.32, 0..ONE
    }

    /// <summary>
    /// The single simulation state struct. One instance per run; preallocated
    /// at boot (zero hot-loop allocation, A-019 §4 BodyPool rule / PLAN 5.16).
    /// </summary>
    public class SimState
    {
        // ---- pool capacities (A-019 §4 BodyPool) ------------------------------
        public static readonly int BODY_CAPACITY = 300;      // small<=220 / medium<=56 / large<=24
        public static readonly int SMALL_CAPACITY = 220;
        public static readonly int MEDIUM_CAPACITY = 56;
        public static readonly int LARGE_CAPACITY = 24;
        public static readonly int HAZARD_CAPACITY = 24;     // hazard pool
        public static readonly int ATTRACTOR_MAX = 32;       // attractor list <= 32 (A-019 §4)

        // ---- run identity (A-005: a run is (sessionSeed, inputDigest)) -------
        public ulong sessionSeed;   // u64, from LevelLoader/CLI
        public ulong seed;          // u64 = hash(levelId, sessionSeed) — PRNG.cs
        public ulong s0;            // xoshiro256+ state words (snapshot §7: 4×u64)
        public ulong s1;
        public ulong s2;
        public ulong s3;

        // ---- step clock -------------------------------------------------------
        public long stepIndex;      // i64, monotonic, never rewound
        public long accumulator;    // Q32.32 wall-clock debt owned by FixedStepDriver
        public long dt;             // Q32.32; FixedStepDriver asserts constant 1/60

        // ---- hole (body index 0; A-019 §4 PlayerController) -------------------
        // Snapshot §7 emits hole as 7 i64s: posX posY velX velY mass radius +
        // the seventh. A-019 ambiguity decision (documented in Snapshot.cs,
        // stage 3): the seventh is eventRadius. Kept here so state and format
        // agree; §7 order is append-only.
        public long holeX;
        public long holeY;
        public long holeVX;
        public long holeVY;
        public long holeMass;       // integer mass, stored Q32.32 raw
        public long holeRadius;     // collision radius (Q32.32)
        public long holeEventRadius; // eventRadius = baseRadius + a*isqrt(mass)

        // ---- attractor list (attraction order = attractor-index order) --------
        public int attractorCount;                       // <= ATTRACTOR_MAX
        public int[] attractorIndex;                     // body indices, iteration order fixed
        public long[] attractorRadius;                   // i64 per attractor (snapshot radii)

        // ---- body pool, SoA (A-019 §4 BodyPool; snapshot §7 record order) ----
        public byte[] bodyFlags;    // u8: bit0 active, bit1 absorbable, bit2 tier bits
        public byte[] bodyCategory; // i8: 0 small, 1 medium, 2 large, -1 inactive
        public byte[] bodyTypeId;   // i8: ObjectTable typeId
        public long[] bodyX;
        public long[] bodyY;
        public long[] bodyVX;
        public long[] bodyVY;
        public long[] bodyMass;     // integer mass (Q32.32 raw)
        public long[] bodyRadius;   // collision radius (Q32.32)

        // ---- hazard pool, SoA, same record shape (empty in phase 1) ----------
        public byte[] hazardFlags;
        public byte[] hazardCategory;
        public byte[] hazardTypeId;
        public long[] hazardX;
        public long[] hazardY;
        public long[] hazardVX;
        public long[] hazardVY;
        public long[] hazardMass;
        public long[] hazardRadius;

        // ---- freeList: ascending u16 indices (snapshot §7) -------------------
        public int freeCount;               // u16 count
        public int[] freeList;              // ascending body indices available for alloc

        // ---- counters (snapshot §7 counters block) ---------------------------
        public long score;                  // stub slot (ScoringSystem)
        public long stability;              // stub slot (StabilitySystem)
        public long combo;                  // stub slot (ComboSystem)
        public uint absorbedTotal;          // u32
        public int highmarkSmall;           // u16 pool highmarks
        public int highmarkMedium;
        public int highmarkLarge;
        public int highmarkHazard;
        public long stepsRun;               // i64
        public long droppedSteps;           // i64 (FixedStepDriver dropped-step rule)

        // ---- world bounds (LevelTable; PLACEHOLDER owned by A-009) -----------
        public long boundsW;                // Q32.32 WU
        public long boundsH;                // Q32.32 WU

        // ---- spatial hash (SpatialHash.cs; uniform grid, counting sort) ------
        public long cellSize;               // Q32.32 WU, asserted 1..32 by boot
        public int gridW;                   // cells per axis
        public int gridH;
        public int[] cellCount;             // per-cell live counts (reused each build)
        public int[] cellPrefix;            // counting-sort prefix offsets
        public int[] sortedBodies;          // ascending-index query order
        public int[] bodyCell;              // per-body cell assignment
        public int[] queryMark;             // stamp marks for ascending queries
        public int queryStamp;              // monotonic query stamp

        // ---- per-step intent (quantized to steps; A-019 §4) ------------------
        public Intent intent;

        // ---- boot: preallocate everything, zero hot-loop allocation ----------
        public static void boot(SimState s)
        {
            s.bodyFlags = new byte[BODY_CAPACITY];
            s.bodyCategory = new byte[BODY_CAPACITY];
            s.bodyTypeId = new byte[BODY_CAPACITY];
            s.bodyX = new long[BODY_CAPACITY];
            s.bodyY = new long[BODY_CAPACITY];
            s.bodyVX = new long[BODY_CAPACITY];
            s.bodyVY = new long[BODY_CAPACITY];
            s.bodyMass = new long[BODY_CAPACITY];
            s.bodyRadius = new long[BODY_CAPACITY];

            s.hazardFlags = new byte[HAZARD_CAPACITY];
            s.hazardCategory = new byte[HAZARD_CAPACITY];
            s.hazardTypeId = new byte[HAZARD_CAPACITY];
            s.hazardX = new long[HAZARD_CAPACITY];
            s.hazardY = new long[HAZARD_CAPACITY];
            s.hazardVX = new long[HAZARD_CAPACITY];
            s.hazardVY = new long[HAZARD_CAPACITY];
            s.hazardMass = new long[HAZARD_CAPACITY];
            s.hazardRadius = new long[HAZARD_CAPACITY];

            s.freeList = new int[BODY_CAPACITY];
            s.attractorIndex = new int[ATTRACTOR_MAX];
            s.attractorRadius = new long[ATTRACTOR_MAX];
            s.queryMark = new int[BODY_CAPACITY];

            s.intent = new Intent();
            // grid arrays sized by SpatialHash.boot once bounds are known
        }
    }
}

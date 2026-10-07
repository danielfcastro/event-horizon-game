// A-021 Phase 1 prototype — Assets/Runtime/Data/LevelTable.cs
// Generated C# constants (A-019 §3 Data/, §12 item 2: numbers identical,
// never renumbered). Single entry: p1-level-01 (Dust Belt L1, T(1)=1).
//
// API pinned by SpawnDirector.cs (do not change):
//   LevelTable.spawnCount          int
//   LevelTable.spawnStep[]         long — stepIndex, ascending (asserted)
//   LevelTable.spawnTypeId[]       byte — ObjectTable typeId
//   LevelTable.spawnX[] / spawnY[] long — Q32.32 RAW (WU integer << 32).
//   Conversion rule: tables are authored in WU integers (A-019 §6); the raw
//   Q32.32 value is (wu << 32), matching SpawnDirector's mass/radius << 32.
//
// A-009 level-1 parameters (brief §7): target mass 1,000; timer 155 s
// (informational only — SimCore has no wall clock); no secondary goal; no
// hazards, no modifiers. worldBounds 128x96 WU is a PLACEHOLDER owned by
// A-009. Live-body budget 110–140 (A-007 §3.2): 50 static spawns peak well
// under it. Fixed list total spawn mass = 1,482 (absorbed at efficiency 0.85
// = 1,259 >= 1,000 target; comfortably above 1000/0.85 = 1,177).
//
// Teaching beat (brief §7): one Asteroid (typeId 6, mass 120) sits on the
// starting drift line (hole starts at (64,48); drift line = +X row y=48) at
// (72,48) — the skip-until-mass≈400 beat.
//
// Pure C#, zero Unity API, no float/double.

namespace EH
{
    public static class LevelTable
    {
        // ---- p1-level-01 header ----------------------------------------------
        public static readonly string levelId = "p1-level-01";
        public static readonly ulong defaultSeed = 0x1F4AUL; // fixed u64 (brief)
        public static readonly long boundsW = 128L; // WU — PLACEHOLDER (A-009 owns)
        public static readonly long boundsH = 96L;  // WU — PLACEHOLDER (A-009 owns)
        public static readonly long targetMass = 1000L; // primary goal
        public static readonly long timerSeconds = 155L; // informational, not sim

        // ---- spawnEvents: (step, typeId, x, y), ascending step order ---------
        public static readonly int spawnCount = 50;

        public static readonly long[] spawnStep = new long[]
        {
            0,    60,   120,   180,   240,   300,   360,   420,   480,   540,
            600,  660,   720,   780,   840,   900,   960,  1020,  1080,  1140,
            1200, 1260,  1320,  1380,  1440,  1500,  1560,  1620,  1680,  1740,
            1800, 1860,  1920,  1980,  2040,  2100,  2160,  2220,  2280,  2340,
            2400, 2460,  2520,  2580,  2640,  2700,  2760,  2820,  2880,  2940
        };

        public static readonly byte[] spawnTypeId = new byte[]
        {
            6, 1, 2, 3, 4, 5, 1, 2, 3, 4,
            5, 1, 2, 3, 4, 5, 1, 2, 3, 4,
            5, 1, 2, 3, 4, 5, 1, 2, 3, 4,
            1, 2, 3, 4, 1, 2, 3, 6, 1, 2,
            3, 4, 6, 1, 3, 4, 6, 1, 3, 4
        };

        // Q32.32 raw = WU << 32. Asteroid k0 at (72,48) = drift line beat.
        public static readonly long[] spawnX = new long[]
        {
            72L << 32, 12L << 32, 30L << 32, 45L << 32, 55L << 32, 90L << 32,
            100L << 32, 14L << 32, 25L << 32, 38L << 32,
            110L << 32, 60L << 32, 70L << 32, 80L << 32, 95L << 32, 20L << 32,
            115L << 32, 48L << 32, 66L << 32, 84L << 32,
            105L << 32, 16L << 32, 52L << 32, 74L << 32, 102L << 32, 34L << 32,
            120L << 32, 22L << 32, 42L << 32, 68L << 32,
            58L << 32, 88L << 32, 112L << 32, 30L << 32, 78L << 32, 98L << 32,
            50L << 32, 108L << 32, 86L << 32, 62L << 32,
            118L << 32, 44L << 32, 26L << 32, 92L << 32, 36L << 32, 76L << 32,
            116L << 32, 54L << 32, 64L << 32, 40L << 32
        };

        public static readonly long[] spawnY = new long[]
        {
            48L << 32, 20L << 32, 70L << 32, 30L << 32, 60L << 32, 40L << 32,
            80L << 32, 12L << 32, 85L << 32, 15L << 32,
            25L << 32, 10L << 32, 85L << 32, 18L << 32, 75L << 32, 50L << 32,
            55L << 32, 42L << 32, 33L << 32, 66L << 32,
            70L << 32, 36L << 32, 78L << 32, 12L << 32, 14L << 32, 62L << 32,
            30L << 32, 72L << 32, 24L << 32, 52L << 32,
            68L << 32, 32L << 32, 60L << 32, 44L << 32, 82L << 32, 20L << 32,
            56L << 32, 84L << 32, 14L << 32, 74L << 32,
            46L << 32, 80L << 32, 28L << 32, 58L << 32, 38L << 32, 26L << 32,
            76L << 32, 34L << 32, 88L << 32, 64L << 32
        };
    }
}

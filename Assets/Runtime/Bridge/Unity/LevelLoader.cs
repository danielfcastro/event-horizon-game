// A-021 Phase 1 prototype — Assets/Runtime/Bridge/Unity/LevelLoader.cs
// A-019 §4: LevelLoader (minimal): loads the SINGLE LevelTable entry for a
// levelId into a fresh SimState. Unity-API-FREE on request of the headless
// build (brief §9: LevelLoader/InputAdapter/PlatformBridge compile headless).
//
// Contract (A-019 §11): the harness (tools/harness/main.cs) and GameLoop both
// build the SAME SimState through this loader and then call the same
// FixedStepDriver.step — exactly one simulation loop.
//
// Load rule (brief §4): seed via PRNG.seedFrom(levelId, sessionSeed); spawn
// events are replayed by SpawnDirector (fixed LevelTable.spawnEvents, no PRNG
// draws — it is called per step by the entry loop before FixedStepDriver.step);
// hole start per LevelTable (its header comment pins the p1-level-01 hole
// start at (64,48) WU — the drift line y=48 the Asteroid beat sits on).
//
// Starting hole (A-007 canonical, brief §5 — never renumbered):
//   startMass = 100 × T(n); p1-level-01 is Dust Belt L1, T(1) = 1 -> 100.
//   hole radius = 1 WU (A-019 §6: 1 WU = visual radius of starting hole at
//   starting mass).
//   holeEventRadius = baseRadius + a*isqrt(mass) = 8 + 2*isqrt(100) = 28 WU,
//   computed through ISqrtQ so GrowthSystem's staleness inversion (stored
//   eventRadius implies isqrt exactly) holds from step 0 — no recompute
//   surprise, no softening.
//
// Pure C#, zero Unity API, no float/double, Q32.32 raw int64 (A-019 §5).

namespace EH
{
    public static class LevelLoader
    {
        // p1-level-01 hole start (WU integers; LevelTable.cs header pins this
        // value — the drift line y=48 the skip-until-mass≈400 beat sits on).
        public static readonly long HOLE_START_X_WU = 64L;
        public static readonly long HOLE_START_Y_WU = 48L;

        /// <summary>
        /// Build a fresh SimState for `levelId` under `sessionSeed`. A run is
        /// identified by (sessionSeed, inputDigest) (A-005); the levelId only
        /// selects the single LevelTable entry.
        /// </summary>
        public static SimState load(string levelId, ulong sessionSeed)
        {
            // single-entry table: unknown levelId is a hard rejection, never a
            // silent default-to-level-1 (no-softening).
            Fail.check(levelId == LevelTable.levelId,
                "LevelLoader: unknown levelId (phase 1 has exactly one entry: p1-level-01)");

            SimState s = new SimState();

            // pool boot: allocates the SoA arrays once and fills the ascending
            // freeList (index 0 reserved for the hole — BodyPool.boot rule).
            BodyPool.boot(s);

            // ---- run identity ---------------------------------------------------
            s.sessionSeed = sessionSeed;
            ulong seed = PRNG.seedFrom(levelId, sessionSeed); // seed = hash(levelId, sessionSeed)
            PRNG.seedState(s, seed);

            // ---- step clock: DT is FixedStepDriver's constant 1/60 --------------
            s.dt = FixedStepDriver.DT;
            s.stepIndex = 0L;
            s.accumulator = 0L;
            s.stepsRun = 0L;
            s.droppedSteps = 0L;

            // ---- world bounds (LevelTable WU integers -> Q32.32 raw; PLACEHOLDER
            // owned by A-009, brief §7) -------------------------------------------
            s.boundsW = LevelTable.boundsW << 32;
            s.boundsH = LevelTable.boundsH << 32;
            SpatialHash.boot(s); // asserts cellSize 1..32 WU (A-019 §4)

            // ---- hole (body index 0) ---------------------------------------------
            s.holeX = HOLE_START_X_WU << 32;
            s.holeY = HOLE_START_Y_WU << 32;
            s.holeVX = 0L;
            s.holeVY = 0L;

            // startMass = 100 × T(1) = 100, stored Q32.32 raw
            long startMass = 100L;
            s.holeMass = startMass << 32;

            // collision radius: 1 WU (A-019 §6)
            s.holeRadius = 1L << 32;

            // eventRadius = baseRadius + a * isqrt(mass) — same expression GrowthSystem
            // uses, so its staleness inversion is exact at boot.
            long isqrtStart = ISqrtQ.isqrtMass(startMass); // Q raw, = 10 << 32
            s.holeEventRadius = FixedQ.add(GrowthSystem.BASE_RADIUS, isqrtStart << 1);

            // ---- attractor list: the hole is the only attractor (index order
            // fixed, <= 32; radii slot = eventRadius per AttractionSystem gate) ----
            s.attractorCount = 1;
            s.attractorIndex[0] = 0;
            s.attractorRadius[0] = s.holeEventRadius;

            // ---- counters (snapshot §7 block) -------------------------------------
            s.score = 0L;
            s.stability = 0L;
            s.combo = 0L;
            s.absorbedTotal = 0u;
            s.highmarkSmall = 0;
            s.highmarkMedium = 0;
            s.highmarkLarge = 0;
            s.highmarkHazard = 0;

            // ---- initial intent: coast/neutral (code 16 / code 10) ---------------
            s.intent = new Intent();
            s.intent.thrustX = 0L;
            s.intent.thrustY = 0L;
            s.intent.rate = InputDigest.rateFromCode(InputDigest.RATE_NEUTRAL);

            // spawnEvents are NOT applied at load: SpawnDirector.apply(s) is called
            // once per step by the entry loop (harness main.cs / GameLoop) before
            // FixedStepDriver.step, replaying the fixed table at matching stepIndex
            // with zero PRNG draws. Loading step-0 spawns here would double-apply
            // them (no-softening: exactly one application path).
            return s;
        }
    }
}

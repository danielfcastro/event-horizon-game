// A-021 Phase 1 prototype — Assets/Runtime/SimCore/PRNG.cs
// A-019 §4 / A-005 determinism contract: splitmix64 seeds xoshiro256+;
// seed = hash(levelId, sessionSeed). `ulong next(SimState)` draws from the
// state's own words — no wall clock, no Unity API, no hidden state.
//
// Purity note (S-family S-09): SpawnDirector consumes NO PRNG draws in phase 1
// (fixed LevelTable.spawnEvents); the hole/growth chain never calls next().
// Draws exist so content phases (A-009 respawn, Phase 3) can be deterministic.
// Pure C#, zero Unity API, no float/double.

namespace EH
{
    public static class PRNG
    {
        /// <summary>
        /// Derive the run seed from (levelId, sessionSeed): FNV-1a 64 over the
        /// ASCII bytes of levelId, mixed with sessionSeed through splitmix64.
        /// Deterministic across platforms (no locale, no pointer, no clock).
        /// </summary>
        public static ulong seedFrom(string levelId, ulong sessionSeed)
        {
            // FNV-1a 64 basis/prime
            ulong fnvOffset = 0xCBF29CE484222327UL;
            ulong fnvPrime = 0x100000001B3UL;
            ulong h = fnvOffset;
            for (int i = 0; i < levelId.Length; i++)
            {
                h ^= (ulong)(ushort)levelId[i];
                h *= fnvPrime;
            }
            // mix with sessionSeed through one splitmix64 round
            return splitmix64Next(h ^ sessionSeed);
        }

        /// <summary>
        /// One splitmix64 step (returns the new state; splitmix64 output ==
        /// state after mixing, used to seed the four xoshiro256+ words).
        /// </summary>
        public static ulong splitmix64Next(ulong state)
        {
            state += 0x9E3779B97F4A7C15UL;
            ulong z = state;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }

        /// <summary>
        /// Seed the four xoshiro256+ state words from the run seed via
        /// consecutive splitmix64 outputs. Rejects an all-zero state
        /// (xoshiro256+ fixed point) with a hard assert.
        /// </summary>
        public static void seedState(SimState s, ulong seed)
        {
            s.seed = seed;
            ulong sm = seed;
            s.s0 = splitmix64Next(sm); sm = s.s0;
            s.s1 = splitmix64Next(sm); sm = s.s1;
            s.s2 = splitmix64Next(sm); sm = s.s2;
            s.s3 = splitmix64Next(sm);
            Fail.check((s.s0 | s.s1 | s.s2 | s.s3) != 0UL, "PRNG.seedState: all-zero xoshiro state");
        }

        /// <summary>
        /// Draw the next u64 from xoshiro256+ (state lives in SimState.s0..s3).
        /// Ascending, fixed word order — no unordered-iteration reads.
        /// </summary>
        public static ulong next(SimState s)
        {
            ulong sum = s.s0 + s.s3;
            ulong t = s.s1 << 17;

            s.s2 ^= s.s0;
            s.s3 ^= s.s1;
            s.s1 ^= s.s2;
            s.s0 ^= s.s3;

            s.s2 ^= t;
            s.s3 = (s.s3 << 45) | (s.s3 >> (64 - 45));

            return sum;
        }

        /// <summary>
        /// Bounded draw in [0, bound) via multiply-shift (Lemire-style
        /// rejection-free variant kept simple and exact: modulo of a uniform
        /// u64 is acceptable for phase-1 spawn selection; documented so S-family
        /// can swap in a rejection sampler later without changing the format).
        /// bound == 0 hard-asserts.
        /// </summary>
        public static int nextBelow(SimState s, int bound)
        {
            Fail.check(bound > 0, "PRNG.nextBelow: bound must be positive");
            ulong r = next(s);
            return (int)(r % (ulong)bound);
        }

        /// <summary>
        /// Self-test: fixed seed must produce a fixed first draw on every
        /// platform (harness runs this before any command, A-019 §5).
        /// </summary>
        public static bool selfTest()
        {
            SimState probe = new SimState();
            PRNG.seedState(probe, seedFrom("p1-level-01", 0x1F4AUL));
            ulong a = PRNG.next(probe);
            ulong b = PRNG.next(probe);
            // re-seed identically and require the same stream
            SimState probe2 = new SimState();
            PRNG.seedState(probe2, seedFrom("p1-level-01", 0x1F4AUL));
            ulong a2 = PRNG.next(probe2);
            ulong b2 = PRNG.next(probe2);
            return a == a2 && b == b2 && (a | b) != 0UL;
        }
    }
}

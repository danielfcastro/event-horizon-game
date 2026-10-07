// A-021 Phase 1 prototype — Assets/Runtime/SimCore/GrowthSystem.cs
// A-019 §4: implemented. Growth rule (brief §5, A-007 canonical numbers —
// never renumbered):
//   eventRadius = baseRadius + a * sqrt(mass)   (baseRadius = 8.0, a = 2.0;
//                     sqrt via ISqrtQ.isqrtMass, fixed-point aware floor)
//   absorptionRadius = absorbFactor * eventRadius  (absorbFactor = 0.30 —
//                     derived on demand by AbsorptionSystem, never stored)
//   massCeiling = 2^24 = 16,777,216 — hole mass SATURATES there (A-013 S-09:
//                     overflow saturates at 2^24, never wraps, never asserts)
// "Recompute only when mass changed this step": no mutable cache is allowed
// (SimState stays one struct, A-019 §12 item 4, and subsystems must be pure),
// so staleness is detected by inversion: stored eventRadius implies
// isqrt(mass) = (eventRadius - baseRadius) / a exactly (a = 2.0, exact shift).
// If the stored implication disagrees with isqrt(current mass), the mass
// changed enough to matter and eventRadius is recomputed; otherwise the
// recompute is skipped. pullStrength is derived on demand from holeMass by
// AttractionSystem (never cached), absorptionRadius likewise by
// AbsorptionSystem — the growth chain is recomputed on absorb (via this
// system, which runs right after AbsorptionSystem in SimStep order), never
// per frame.
// Pure C#, zero Unity API, no float/double, Q32.32 raw int64 (A-019 §5).

namespace EH
{
    public static class GrowthSystem
    {
        // ---- A-007 canonical numbers (brief §5) --------------------------------
        public static readonly long BASE_RADIUS = 8L << 32;   // baseRadius = 8.0 WU
        public static readonly long A_COEF = 2L << 32;        // a = 2.0
        public static readonly long MASS_CEILING = 16777216L; // 2^24 integer mass
        public static readonly long MASS_CEILING_Q = 16777216L << 32; // same, Q32.32 raw

        /// <summary>
        /// Runs after AbsorptionSystem (SimStep order): it sees the hole mass
        /// already grown by this step's absorbs and refreshes eventRadius once.
        /// </summary>
        public static void apply(SimState s)
        {
            // ---- saturate hole mass at massCeiling (S-09: saturate, not wrap) --
            if (s.holeMass > MASS_CEILING_Q)
            {
                s.holeMass = MASS_CEILING_Q;
            }
            Fail.check(s.holeMass >= 0L, "GrowthSystem: negative hole mass (hard-assert)");

            long massInt = s.holeMass >> 32; // integer mass count

            // ---- staleness check by exact inversion of the growth formula -----
            // stored eventRadius = BASE_RADIUS + 2 * isqrt(massAtLastGrowth);
            // a = 2.0 so the division back to isqrt is exact (>> 1 on raw).
            long storedIsqrt = FixedQ.sub(s.holeEventRadius, BASE_RADIUS) >> 1;
            long currentIsqrt = ISqrtQ.isqrtMass(massInt); // floor(sqrt(mass)), Q raw
            if (storedIsqrt == currentIsqrt)
            {
                return; // mass did not change eventRadius: no recompute
            }

            // ---- recompute eventRadius = baseRadius + a * isqrt(mass) ---------
            // mass <= 2^24 => isqrt <= 4096 WU => a*isqrt <= 8192 WU, in Q range.
            long grown = currentIsqrt << 1; // a = 2.0: exact raw doubling
            s.holeEventRadius = FixedQ.add(BASE_RADIUS, grown);

            // boot/consistency assert: eventRadius stays positive and ordered
            Fail.check(s.holeEventRadius > 0L, "GrowthSystem: eventRadius non-positive");
        }
    }
}

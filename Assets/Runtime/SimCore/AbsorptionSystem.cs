// A-021 Phase 1 prototype — Assets/Runtime/SimCore/AbsorptionSystem.cs
// A-019 §4: implemented. Absorption rule (brief §3 / PLAN 5.6, verbatim):
//   if distance(body, blackHole) < absorptionRadius: absorb(body)
//   absorptionRadius = absorbFactor * eventRadius   (absorbFactor = 0.30, A-007 §5)
//   massGain = bodyMass * efficiency_total          (phase 1: comboMult = 1,
//                                                    efficiency_total = 0.85)
// Once per step (each body is examined exactly once per step, ascending index);
// bodies never collide with each other (A-019 §4). On absorb: massGain added to
// the hole, body released via BodyPool.free (ascending freeList, zeroed record),
// absorbedTotal counted. eventRadius/absorptionRadius/pullStrength are NOT
// recomputed per frame: eventRadius is recomputed by GrowthSystem (runs
// immediately after this system in SimStep order) only when holeMass changed;
// absorptionRadius is derived on demand from holeEventRadius here; pullStrength
// is derived on demand from holeMass in AttractionSystem. No cache, no drift.
// Pure C#, zero Unity API, no float/double, Q32.32 raw int64 (A-019 §5).

namespace EH
{
    public static class AbsorptionSystem
    {
        // ---- A-007 canonical numbers (brief §5 — never renumbered) ------------
        // absorbFactor = 0.30 -> exact rational 3/10 (no float literal).
        public static readonly long ABSORB_NUM = 3L << 32;
        public static readonly long ABSORB_DEN = 10L << 32;
        // efficiency_total = 0.85 (phase 1: comboMult = 1) -> exact rational 85/100.
        // Masses are integers (A-007 §5), so the gain is taken as the integer
        // floor: gain = (bodyMassInt * 85) / 100. No fractional mass ever exists.
        public static readonly long EFF_NUM = 85L;
        public static readonly long EFF_DEN = 100L;

        /// <summary>
        /// Runs after AttractionSystem, before GrowthSystem (SimStep order).
        /// Iterates bodies in ascending index order (determinism contract).
        /// </summary>
        public static void apply(SimState s)
        {
            // absorptionRadius = absorbFactor * eventRadius (eventRadius is
            // GrowthSystem-owned; derived here, never stored — no per-frame
            // recompute of the growth chain happens in this system).
            long absorptionRadius = MulDiv.muldiv(s.holeEventRadius, ABSORB_NUM, ABSORB_DEN);
            long radSq = MulDiv.muldiv(absorptionRadius, absorptionRadius, FixedQ.ONE);

            for (int i = 1; i < SimState.BODY_CAPACITY; i++)
            {
                if ((s.bodyFlags[i] & 0x01) == 0)
                {
                    continue; // inactive
                }
                if ((s.bodyFlags[i] & 0x02) == 0)
                {
                    continue; // not absorbable (ObjectTable bit; hazards never absorb)
                }

                // direction = hole.pos - body.pos
                long dx = FixedQ.sub(s.holeX, s.bodyX[i]);
                long dy = FixedQ.sub(s.holeY, s.bodyY[i]);

                // distance check without epsilon: the absorption rule is a plain
                // distance test (epsilon belongs to the force denominator only).
                long distSq = FixedQ.add(
                    MulDiv.muldiv(dx, dx, FixedQ.ONE),
                    MulDiv.muldiv(dy, dy, FixedQ.ONE));

                if (FixedQ.cmp(distSq, radSq) >= 0)
                {
                    continue; // outside absorptionRadius
                }

                // ---- absorb(body) ------------------------------------------------
                long massInt = s.bodyMass[i] >> 32; // integer mass count (Q32.32 raw)
                Fail.check(massInt > 0L, "AbsorptionSystem: active absorbable body has zero mass");

                // massGain = bodyMass * efficiency_total, integer floor (masses
                // are integers, A-007 §5; body mass <= 800,000 so *85 is in range)
                long gainInt = (massInt * EFF_NUM) / EFF_DEN;

                // hole mass grows; saturation at massCeiling is GrowthSystem's
                // job (it runs next and owns the ceiling rule). Overflow-checked.
                s.holeMass = FixedQ.add(s.holeMass, gainInt << 32);

                // absorbedTotal counter (snapshot §7 counters block, u32)
                Fail.check(s.absorbedTotal < 0xFFFFFFFFu, "AbsorptionSystem: absorbedTotal u32 overflow");
                s.absorbedTotal = s.absorbedTotal + 1u;

                // free the body: ascending freeList insert, record zeroed
                // (inactive = zero record in EHSNAP1). Index 0 (the hole) is
                // never touched — loop starts at 1.
                BodyPool.free(s, i);
            }
        }
    }
}

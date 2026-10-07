// A-021 Phase 1 prototype — Assets/Runtime/SimCore/PlayerController.cs
// A-019 §4: black hole is body index 0; movement intent only.
// A-007 canonical (never renumbered): baseSpeed = 26;
//   speed(mass) = baseSpeed / (1 + mass/100000)^0.25   (two sqrt + one fixed-point division)
// Hole position/velocity live in SimState (holeX/holeY/holeVX/holeVY/holeMass).
// Pure function of (state, stepIndex, intent); no wall clock, no Unity API,
// no float/double. Q32.32 raw int64 throughout (A-019 §5).
//
// Note: speed is recomputed from holeMass every sim step. The A-007 "never per
// frame" note targets the Unity render loop; a sim step IS the fixed frame, and
// SimState (one struct, §12 item 4) has no speed slot to cache into.

namespace EH
{
    public static class PlayerController
    {
        // ---- A-007 canonical numbers ------------------------------------------
        public static readonly long BASE_SPEED = 26L << 32;      // 26 WU/s at mass 0
        public static readonly long SPEED_DIVISOR = 100000L;     // mass/100000

        public static void apply(SimState s)
        {
            // black hole is body index 0 (A-019 §4). Movement intent only:
            // intent.thrustX/thrustY = quantized 16-way unit direction
            // (InputDigest table), intent.rate = Q32.32 in [0,1].
            long tx = s.intent.thrustX;
            long ty = s.intent.thrustY;
            long rate = s.intent.rate;
            Fail.check(rate >= 0L && rate <= FixedQ.ONE,
                "PlayerController: intent.rate out of [0,1]");

            // speed(mass) = baseSpeed / (1 + mass/100000)^0.25
            // ^0.25 = sqrt(sqrt(x)); two isqrt + one fixed-point division.
            long massOver = MulDiv.muldiv(s.holeMass, FixedQ.ONE, SPEED_DIVISOR << 32);
            long x = FixedQ.add(FixedQ.ONE, massOver); // 1 + mass/100000
            long root = ISqrtQ.isqrt(ISqrtQ.isqrt(x));
            Fail.check(root > 0L, "PlayerController: speed denominator zero");
            long speed = MulDiv.muldiv(BASE_SPEED, FixedQ.ONE, root);

            // velocity = thrust * speed * rate (arcade: intent sets velocity,
            // no inertia buffer — no-softening)
            long move = MulDiv.muldiv(speed, rate, FixedQ.ONE);
            s.holeVX = MulDiv.muldiv(tx, move, FixedQ.ONE);
            s.holeVY = MulDiv.muldiv(ty, move, FixedQ.ONE);

            // integrate by dt (FixedStepDriver owns dt = 1/60, asserted constant)
            s.holeX = FixedQ.add(s.holeX, MulDiv.muldiv(s.holeVX, s.dt, FixedQ.ONE));
            s.holeY = FixedQ.add(s.holeY, MulDiv.muldiv(s.holeVY, s.dt, FixedQ.ONE));
        }
    }
}

// A-021 Phase 1 prototype — Assets/Runtime/SimCore/AttractionSystem.cs
// A-005 formula VERBATIM (brief §3/§5) with A-007 canonical constants (never renumbered):
//   pullStrength    = pullScale * mass^0.75   (pullScale = 2.0;
//                      mass^0.75 = sqrt(mass * sqrt(mass)), two integer sqrt)
//   direction       = blackHole.pos - body.pos
//   distanceSquared = direction.lengthSquared() + epsilon   (epsilon = 1.0)
//   force           = pullStrength * bodyMass / distanceSquared
//                     -> division-first ordering (pullStrength / distanceSquared) * bodyMass
//                        to keep intermediates in range (A-007)
//   body.velocity  += direction.normalized() * force * deltaTime
// Attractor iteration = attractor-index order (A-019 §4); attractor list <= 32;
// bodies iterated ascending index (SimStep rule). Attraction is gated to the
// attractor's eventRadius (attractorRadius slot, snapshot §7 radii) — decision
// documented here: eventRadius is the "event radius" at which the pull event
// fires; beyond it no force is applied. Bodies never collide with each other.
// Pure C#, zero Unity API, no float/double, Q32.32 raw int64.

namespace EH
{
    public static class AttractionSystem
    {
        // ---- A-007 canonical numbers ------------------------------------------
        public static readonly long PULL_SCALE = 2L << 32;  // pullScale = 2.0
        public static readonly long EPSILON = 1L << 32;     // epsilon = 1.0

        public static void apply(SimState s)
        {
            Fail.check(s.attractorCount >= 0 && s.attractorCount <= SimState.ATTRACTOR_MAX,
                "AttractionSystem: attractor list exceeds 32 (A-019 §4)");

            for (int k = 0; k < s.attractorCount; k++)
            {
                int aIdx = s.attractorIndex[k];

                // attractor mass: body index 0 is the black hole
                long massRaw;
                if (aIdx == 0)
                {
                    massRaw = s.holeMass;
                }
                else
                {
                    Fail.check(aIdx > 0 && aIdx < SimState.BODY_CAPACITY,
                        "AttractionSystem: attractor index out of pool range");
                    Fail.check((s.bodyFlags[aIdx] & 1) != 0,
                        "AttractionSystem: attractor body inactive");
                    massRaw = s.bodyMass[aIdx];
                }

                // pullStrength = pullScale * mass^0.75, mass^0.75 = sqrt(mass*sqrt(mass)),
                // two integer sqrts on the integer mass count.
                long mass = massRaw >> 32; // integer mass (masses are integers, A-007)
                long r1 = FixedQ.toIntTrunc(ISqrtQ.isqrtMass(mass));      // floor(sqrt(mass))
                long prod = mass * r1;                                    // <= 8e5 * 894, in range
                long r2 = FixedQ.toIntTrunc(ISqrtQ.isqrtMass(prod));      // floor(sqrt(mass*r1))
                Fail.check(r2 <= 0x7FFFFFFFL >> 2, "AttractionSystem: pullStrength overflow");
                long pullStrength = (r2 << 32) * 2L;                      // pullScale = 2.0

                // attractor position
                long ax, ay;
                if (aIdx == 0)
                {
                    ax = s.holeX;
                    ay = s.holeY;
                }
                else
                {
                    ax = s.bodyX[aIdx];
                    ay = s.bodyY[aIdx];
                }

                // eventRadius gate: distSq <= radius^2 (radius stored Q32.32)
                long rad = s.attractorRadius[k];
                long radSq = MulDiv.muldiv(rad, rad, FixedQ.ONE);

                // bodies over ascending index; bodies do not collide with each other
                for (int i = 1; i < SimState.BODY_CAPACITY; i++)
                {
                    if (i == aIdx)
                    {
                        continue;
                    }
                    if ((s.bodyFlags[i] & 1) == 0)
                    {
                        continue; // inactive
                    }

                    // direction = attractor.pos - body.pos
                    long dx = FixedQ.sub(ax, s.bodyX[i]);
                    long dy = FixedQ.sub(ay, s.bodyY[i]);

                    // distanceSquared = lengthSquared + epsilon (muldiv keeps WU^2 in Q)
                    long distSq = FixedQ.add(
                        FixedQ.add(MulDiv.muldiv(dx, dx, FixedQ.ONE),
                                   MulDiv.muldiv(dy, dy, FixedQ.ONE)),
                        EPSILON);

                    if (FixedQ.cmp(distSq, radSq) > 0)
                    {
                        continue; // outside eventRadius: no pull event
                    }

                    // division-first ordering: (pullStrength / distSq) * bodyMass
                    long f1 = MulDiv.muldiv(pullStrength, FixedQ.ONE, distSq);
                    long force = MulDiv.muldiv(f1, s.bodyMass[i], FixedQ.ONE);

                    // direction.normalized() = dir / sqrt(distSq); epsilon=1.0 => len >= 1
                    long len = ISqrtQ.isqrt(distSq);
                    Fail.check(len > 0L, "AttractionSystem: normalization length zero");
                    long nx = MulDiv.muldiv(dx, FixedQ.ONE, len);
                    long ny = MulDiv.muldiv(dy, FixedQ.ONE, len);

                    // velocity += normalized(dir) * force * deltaTime
                    long dvx = MulDiv.muldiv(MulDiv.muldiv(nx, force, FixedQ.ONE), s.dt, FixedQ.ONE);
                    long dvy = MulDiv.muldiv(MulDiv.muldiv(ny, force, FixedQ.ONE), s.dt, FixedQ.ONE);
                    s.bodyVX[i] = FixedQ.add(s.bodyVX[i], dvx);
                    s.bodyVY[i] = FixedQ.add(s.bodyVY[i], dvy);
                }
            }
        }
    }
}

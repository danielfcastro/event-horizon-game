// A-021 Phase 1 prototype — Assets/Runtime/Fixed/MulDiv.cs
// muldiv(a, b, d) = (a * b) / d for Q32.32 raw values, computed through a full
// 128-bit intermediate assembled from 32-bit limbs (Hodgson-style), half-away-
// from-zero rounding, d == 0 hard-asserts (A-019 §5).
//
// A-019 §12 item 1: the limb decomposition below IS the scaffold's reference
// implementation — keep it. Any later optimization must stay inside the P-
// family performance budget and be re-verified by the S-family determinism
// tests; the bit-for-bit result of this function is the contract.
//
// Fixed-point math: (aRaw/2^32 * bRaw/2^32) / (dRaw/2^32) = (aRaw*bRaw)/dRaw / 2^32,
// so the raw result is exactly (aRaw * bRaw) / dRaw with fixed-point rounding.
// Pure C#, zero Unity API, no float/double.

namespace EH
{
    public static class MulDiv
    {
        public static long muldiv(long a, long b, long d)
        {
            if (d == 0L)
            {
                Fail.fail("MulDiv.muldiv: divisor zero (hard-assert per A-019 §5)");
            }

            // ---- sign split: work on unsigned magnitudes ----------------------
            // Two's-complement negation of LONG_MIN wraps to LONG_MIN, whose
            // unsigned reading is the correct magnitude 2^63, so this is exact.
            bool negative = (a < 0) ^ (b < 0) ^ (d < 0);
            ulong ua = a < 0 ? (ulong)(-a) : (ulong)a;
            ulong ub = b < 0 ? (ulong)(-b) : (ulong)b;
            ulong ud = d < 0 ? (ulong)(-d) : (ulong)d;

            // ---- full 128-bit product via 32-bit limbs (Hodgson-style) -------
            ulong m32 = 0xFFFFFFFFUL;
            ulong a0 = ua & m32;
            ulong a1 = ua >> 32;
            ulong b0 = ub & m32;
            ulong b1 = ub >> 32;

            ulong p0 = a0 * b0; // < 2^64
            ulong p1 = a0 * b1; // < 2^64
            ulong p2 = a1 * b0; // < 2^64
            ulong p3 = a1 * b1; // < 2^64

            // mid < 3 * 2^32 fits comfortably in 64 bits
            ulong mid = (p0 >> 32) + (p1 & m32) + (p2 & m32);
            ulong lo = ((mid & m32) << 32) | (p0 & m32);
            // ua, ub < 2^63 (signed magnitudes) => p3 < 2^62, so hi cannot wrap
            ulong hi = p3 + (p1 >> 32) + (p2 >> 32) + (mid >> 32);

            // ---- divide (hi:lo) by ud, quotient must fit in 64 bits ----------
            if (hi >= ud)
            {
                Fail.fail("MulDiv.muldiv: 128/64 quotient overflow");
            }

            // Shift-subtract long division over the 128-bit dividend. The
            // remainder is kept in rLo alone; seeded with hi and guarded by
            // hi < ud, the invariant r < ud < 2^64 holds at every iteration,
            // so a single conditional subtract per bit is exact.
            ulong q = 0L;
            // Seed the remainder with hi: the guard above proves hi < ud, so the
            // 64 iterations over lo's bits are the last 64 of a 128-bit long
            // division whose first 64 (hi's) bits all produce zero quotient bits.
            ulong rLo = hi;
            for (int i = 63; i >= 0; i--)
            {
                bool bit = ((lo >> i) & 1UL) != 0UL;
                bool nextCarry = (rLo >> 63) != 0UL;
                rLo = (rLo << 1) | (bit ? 1UL : 0UL);
                // candidate remainder = (nextCarry ? 2^64 : 0) + rLo; compare vs ud
                if (nextCarry || rLo >= ud)
                {
                    // candidate < 2*ud, so candidate - ud < 2^64; the unsigned
                    // wrap of (rLo - ud) is exactly candidate - ud when nextCarry.
                    rLo -= ud;
                    q = (q << 1) | 1UL;
                }
                else
                {
                    q <<= 1;
                }
            }
            // rLo is the final remainder, < ud, because r < ud after the last step.

            // ---- rounding: half away from zero -------------------------------
            // round up iff 2*r >= ud; test without overflowing 2*r.
            if (rLo != 0UL && rLo >= ud - rLo)
            {
                q += 1UL;
                if (q == 0UL)
                {
                    Fail.fail("MulDiv.muldiv: rounding pushed quotient to 2^64");
                }
            }

            // ---- re-apply sign, range-check into signed 64-bit raw -----------
            if (negative)
            {
                if (q > (ulong)0x8000000000000000UL)
                {
                    Fail.fail("MulDiv.muldiv: signed result overflow");
                }
                return q == 0x8000000000000000UL ? (-9223372036854775807L - 1) : -(long)q;
            }
            if (q > (ulong)0x7FFFFFFFFFFFFFFFUL)
            {
                Fail.fail("MulDiv.muldiv: signed result overflow");
            }
            return (long)q;
        }

        // ---- self-test vectors (harness runs these before any command) -------
        // Harness (stage 3) calls MulDiv.selfTest() and H-01 refuses to start
        // on failure (A-019 §5). Vectors are exact Q32.32 expectations.
        public static bool selfTest()
        {
            // 1) (2.0 * 3.0) / 1.5 = 4.0
            if (muldiv(2L << 32, 3L << 32, (3L << 32) / 2L) != 4L << 32)
            {
                return false;
            }
            // 2) rounding: (1.0 * 1.0) / 3.0 -> raw 0x55555555 (0.33333...),
            //    half-away-from-zero on the discarded low bits
            long third = muldiv(1L << 32, 1L << 32, 3L << 32);
            if (third != 0x55555555L)
            {
                return false;
            }
            // 3) negative leg: (-2.0 * 3.0) / 1.5 = -4.0
            if (muldiv(-(2L << 32), 3L << 32, (3L << 32) / 2L) != -(4L << 32))
            {
                return false;
            }
            // 4) large leg: (2^31-1 WU * 2^31-1 WU) / 1.0 stays in range
            long big = (2147483647L << 32);
            if (muldiv(big, 1L << 32, 1L << 32) != big)
            {
                return false;
            }
            // 5) physics-shaped: (pullStrength / distSq) * bodyMass with
            //    pull=2.0, distSq=4.0, mass=100 -> 50.0
            if (muldiv(2L << 32, 100L << 32, 4L << 32) != 50L << 32)
            {
                return false;
            }
            return true;
        }
    }
}

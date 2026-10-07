// A-021 Phase 1 prototype — Assets/Runtime/Fixed/ISqrtQ.cs
// isqrt(x) = floor(sqrt(x)) fixed-point aware (A-019 §5).
//
// For a Q32.32 raw x (value = x / 2^32):
//   sqrt(value) = sqrt(x) / 2^16
//   floor(sqrt(value)) in Q32.32 raw form = floor(sqrt(x)) << 16
// The integer sqrt runs on the raw magnitude with the classic base-4 digit
// algorithm (integer only, no float/double). Result resolution is 2^-16 of a
// world unit, which is the tightest exact floor representable from a 64-bit
// intermediate — this is the "fixed-point aware" floor the scaffold specifies.
//
// Used by GrowthSystem (eventRadius = baseRadius + a * isqrt(mass)),
// AttractionSystem (mass^0.75 = sqrt(mass * sqrt(mass)), two integer sqrts),
// speed(mass) (two sqrt + one fixed-point division), and SpatialHash.boot
// (cellSize = sqrt(worldArea / N)).
// Pure C#, zero Unity API, no float/double.

namespace EH
{
    public static class ISqrtQ
    {
        /// <summary>
        /// floor(sqrt(x)) for a non-negative Q32.32 raw value, returned as a
        /// Q32.32 raw value. Negative input hard-asserts (no silent clamp —
        /// no-softening rule, A-021 acceptance criterion 6).
        /// </summary>
        public static long isqrt(long x)
        {
            Fail.check(x >= 0, "ISqrtQ.isqrt: negative input (hard-assert, no clamp)");
            if (x == 0)
            {
                return 0L;
            }

            ulong n = (ulong)x;

            // largest power of four <= n
            ulong bit = 1UL << 62;
            while (bit > n)
            {
                bit >>= 2;
            }

            // base-4 digit sqrt with an explicit remainder: at each digit two bits
            // of n are brought down, and root-digit 1 is tried; its cost is
            // t = 4*r + 1. The invariant rem < 4*r + 2 <= 2^34 keeps every
            // intermediate in range for n < 2^63.
            ulong r = 0UL;
            ulong rem = 0UL;
            while (bit != 0UL)
            {
                rem = (rem << 2) | ((n / bit) & 3UL);
                ulong t = (r << 2) + 1UL;
                if (rem >= t)
                {
                    rem -= t;
                    r = (r << 1) | 1UL;
                }
                else
                {
                    r <<= 1;
                }
                bit >>= 2;
            }

            // r = floor(sqrt(x_raw)); shift into Q32.32 raw form.
            // x_raw <= 2^63-1 => r < 2^32 => r << 16 < 2^48, always in range.
            return (long)(r << 16);
        }

        /// <summary>
        /// Convenience: floor(sqrt(mass)) for an integer mass count, returned as
        /// a Q32.32 raw value (mass is a whole number, so raw = mass << 32).
        /// Kept because AttractionSystem/GrowthSystem feed integer masses.
        /// </summary>
        public static long isqrtMass(long mass)
        {
            Fail.check(mass >= 0, "ISqrtQ.isqrtMass: negative mass (hard-assert)");
            Fail.check(mass <= 0x7FFFFFFFL, "ISqrtQ.isqrtMass: mass beyond 2^31");
            return isqrt(mass << 32);
        }

        /// <summary>
        /// Self-test vectors for the harness (A-019 §5: harness self-tests both
        /// helpers before any command; H-01 refuses to start on failure).
        /// </summary>
        public static bool selfTest()
        {
            // sqrt(4.0) = 2.0 exactly
            if (isqrt(4L << 32) != 2L << 32)
            {
                return false;
            }
            // sqrt(2.0) = 1.4142135... -> raw floor: floor(sqrt(2*2^32))<<16
            // sqrt(2*2^32) = 92681.99... -> floor 92681 -> 92681<<16 = 6073942016
            if (isqrt(2L << 32) != 92681L << 16)
            {
                return false;
            }
            // sqrt(0) = 0
            if (isqrt(0L) != 0L)
            {
                return false;
            }
            // sqrt(1.0) = 1.0
            if (isqrt(1L << 32) != 1L << 32)
            {
                return false;
            }
            // mass leg: sqrt(100) = 10.0
            if (isqrtMass(100L) != 10L << 32)
            {
                return false;
            }
            // mass leg: sqrt(99) = 9.94987... -> floor(sqrt(99*2^32))<<16
            // sqrt(99*2^32) = 652074.99... -> 652074<<16
            if (isqrtMass(99L) != 652074L << 16)
            {
                return false;
            }
            return true;
        }
    }
}

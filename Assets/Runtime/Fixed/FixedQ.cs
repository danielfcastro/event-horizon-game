// A-021 Phase 1 prototype — Assets/Runtime/Fixed/FixedQ.cs
// Fixed-point 32.32 signed 64-bit per A-019 §5 / A-005 determinism contract.
// Pure C#, zero Unity API, no float/double anywhere.
//
// Representation: a FixedQ value is stored as its raw int64 where value = raw / 2^32.
// Range ±2^31 WU (integer part is a signed 32-bit count of world units),
// resolution 2^-32 (A-019 §5). All SimCore state uses this raw long form.

namespace EH
{
    /// <summary>
    /// Hard-assert helper shared by the whole runtime. A failure is a named,
    /// loud rejection — never a silent fallback, never a cushioning default
    /// (A-021 acceptance criterion 6: no-softening).
    /// </summary>
    public static class Fail
    {
        public static void fail(string reason)
        {
            throw new SystemicFailure(reason);
        }

        public static void check(bool condition, string reason)
        {
            if (!condition)
            {
                throw new SystemicFailure(reason);
            }
        }
    }

    public class SystemicFailure : System.Exception
    {
        public string reason;

        public SystemicFailure(string why)
        {
            reason = why;
        }
    }

    /// <summary>
    /// FixedQ: signed 64-bit Q32.32 fixed-point helpers (A-019 §5).
    /// The "value" of a FixedQ is always the raw int64; helpers convert and
    /// operate on raw values only. No heap allocation, no float/double.
    /// </summary>
    public static class FixedQ
    {
        // ---- format constants -------------------------------------------------
        public static readonly int FRAC_BITS = 32;
        public static readonly long ONE = 1L << 32;                 // raw value of 1.0
        public static readonly long MASK32 = 0xFFFFFFFFL;           // one limb mask
        public static readonly long MAX = 0x7FFFFFFFFFFFFFFFL;      // raw max (+2^31 - 2^-32)
        public static readonly long MIN = -9223372036854775807L - 1; // raw min (-2^31) = Long.MinValue

        // ---- conversions ------------------------------------------------------
        public static long fromInt(int whole)
        {
            // exact: whole * 2^32, range-checked (±2^31 WU)
            long r = (long)whole << 32;
            Fail.check(whole >= -2147483647 && whole <= 2147483647, "FixedQ.fromInt: out of ±2^31 WU range");
            return r;
        }

        public static long fromLong(long whole)
        {
            Fail.check(whole >= -2147483647L && whole <= 2147483647L, "FixedQ.fromLong: out of ±2^31 WU range");
            return whole << 32;
        }

        public static long toIntTrunc(long q)
        {
            // truncate toward zero to whole world units
            if (q >= 0)
            {
                return q >> 32;
            }
            return -((-q) >> 32);
        }

        public static long toIntRound(long q)
        {
            // round half away from zero to whole world units
            if (q >= 0)
            {
                return (q + (1L << 31)) >> 32;
            }
            return -(((-q) + (1L << 31)) >> 32);
        }

        // ---- arithmetic -------------------------------------------------------
        public static long add(long a, long b)
        {
            long r = a + b;
            // signed overflow detection
            bool overflow = ((a ^ r) & (b ^ r)) < 0;
            Fail.check(!overflow, "FixedQ.add: overflow");
            return r;
        }

        public static long sub(long a, long b)
        {
            long r = a - b;
            bool overflow = ((a ^ b) & (a ^ r)) < 0;
            Fail.check(!overflow, "FixedQ.sub: overflow");
            return r;
        }

        public static long neg(long a)
        {
            Fail.check(a != FixedQ.MIN, "FixedQ.neg: cannot negate MIN");
            return -a;
        }

        public static long abs(long a)
        {
            Fail.check(a != FixedQ.MIN, "FixedQ.abs: MIN has no positive counterpart");
            return a < 0 ? -a : a;
        }

        public static int sign(long a)
        {
            if (a > 0)
            {
                return 1;
            }
            if (a < 0)
            {
                return -1;
            }
            return 0;
        }

        public static long min(long a, long b)
        {
            return a < b ? a : b;
        }

        public static long max(long a, long b)
        {
            return a > b ? a : b;
        }

        public static bool isZero(long a)
        {
            return a == 0;
        }

        public static bool eq(long a, long b)
        {
            return a == b;
        }

        public static int cmp(long a, long b)
        {
            if (a < b)
            {
                return -1;
            }
            if (a > b)
            {
                return 1;
            }
            return 0;
        }

        // ---- debug ------------------------------------------------------------
        public static string debug(long q)
        {
            // integer + fractional rendering without float
            long whole = toIntTrunc(q);
            long frac = q - (whole << 32);
            if (frac < 0)
            {
                frac = -frac;
            }
            // scale fraction to 5 decimal digits using integer math only
            long scaled = (frac * 100000L) >> 32;
            string fracStr = scaled.ToString();
            while (fracStr.Length < 5)
            {
                fracStr = "0" + fracStr;
            }
            string s = whole.ToString() + "." + fracStr;
            return s;
        }
    }
}

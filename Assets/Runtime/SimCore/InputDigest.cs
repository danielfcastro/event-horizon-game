// A-021 Phase 1 prototype — Assets/Runtime/SimCore/InputDigest.cs
// EIDIG1 codec per A-019 §8 (brief §8). Codec only — no simulation logic.
//
// Format (all LE, no padding):
//   magic "EIDIG1" | ver u16 = 1 | steps uvarint | RLE pairs {u16 value, uvarint runLength >= 1}
//   | trailing u64 FNV-1a over magic..last pair.
// Per-step pair: byte0 = angle code 0..15 (16-way) or 16 = coast;
//               byte1 = rate code 0..20 (5% steps, 10 = neutral 50%).
// u16 value = byte0<<8 | byte1. Zero-length runs illegal; sum(runLength) == steps.
// Trailing coast/neutral run (angle==16 && rate==10) is trimmed to the last
// non-neutral step on encode.
// decode(): unknown magic/version/sum/code>16 = HARD rejection with named
// reason, never silent fill (no-softening, A-021 acceptance criterion 6).
// Pure C#, zero Unity API, no float/double.

namespace EH
{
    public static class InputDigest
    {
        // ---- format constants (A-019 §8, canonical, never renumbered) --------
        public static readonly int ANGLE_COAST = 16;      // coast code
        public static readonly int RATE_NEUTRAL = 10;     // neutral 50%
        public static readonly int RATE_MAX = 20;         // 5% steps
        public static readonly long FNV_OFFSET = -3746987768968851579L; // 0xcbf29ce484222325UL as signed two's-complement
        public static readonly long FNV_PRIME = 0x100000001b3L;

        // ---- 16-way angle table, raw Q32.32 cos/sin (deterministic constants)
        // code k -> angle k*22.5 degrees counter-clockwise from +X.
        // cos(pi/8)=0.9238795325 -> 3968032378; sin(pi/8)=0.3838832324 -> 1648765929;
        // sqrt(2)/2 -> 3037000500.
        public static long[] ANGLE_X = new long[] {
            FixedQ.ONE, 3968032378L, 3037000500L, 1648765929L,
            0L, -1648765929L, -3037000500L, -3968032378L,
            -FixedQ.ONE, -3968032378L, -3037000500L, -1648765929L,
            0L, 1648765929L, 3037000500L, 3968032378L,
            0L // code 16 = coast -> zero thrust
        };
        public static long[] ANGLE_Y = new long[] {
            0L, 1648765929L, 3037000500L, 3968032378L,
            FixedQ.ONE, 3968032378L, 3037000500L, 1648765929L,
            0L, -1648765929L, -3037000500L, -3968032378L,
            -FixedQ.ONE, -3968032378L, -3037000500L, -1648765929L,
            0L // coast
        };

        // ---- recorded per-step codes -----------------------------------------
        // Codec state: one byte-pair per step, appended by push.
        public class Codes
        {
            public int capacity;
            public int count;
            public byte[] angle;
            public byte[] rate;

            public static void boot(Codes c, int cap)
            {
                c.capacity = cap;
                c.count = 0;
                c.angle = new byte[cap];
                c.rate = new byte[cap];
            }

            public static void push(Codes c, int angleCode, int rateCode)
            {
                Fail.check(angleCode >= 0 && angleCode <= ANGLE_COAST,
                    "InputDigest.push: angle code out of 0..16 (16=coast)");
                Fail.check(rateCode >= 0 && rateCode <= RATE_MAX,
                    "InputDigest.push: rate code out of 0..20 (5% steps)");
                if (c.count >= c.capacity)
                {
                    // grow (codec is not in the sim hot loop; replay digests are
                    // built once per run)
                    int cap = c.capacity * 2;
                    byte[] a = new byte[cap];
                    byte[] r = new byte[cap];
                    for (int i = 0; i < c.count; i++)
                    {
                        a[i] = c.angle[i];
                        r[i] = c.rate[i];
                    }
                    c.angle = a;
                    c.rate = r;
                    c.capacity = cap;
                }
                c.angle[c.count] = (byte)angleCode;
                c.rate[c.count] = (byte)rateCode;
                c.count += 1;
            }
        }

        // ---- uvarint helpers --------------------------------------------------
        public static void writeUvarint(byte[] buf, int[] at, long v)
        {
            Fail.check(v >= 0, "InputDigest.writeUvarint: negative value");
            do
            {
                int b = (int)(v & 0x7FL);
                v >>= 7;
                if (v != 0L)
                {
                    b |= 0x80;
                }
                buf[at[0]] = (byte)b;
                at[0] += 1;
            }
            while (v != 0L);
        }

        public static int[] readUvarint(byte[] data, int[] at, int limit)
        {
            long v = 0L;
            int shift = 0;
            while (true)
            {
                if (at[0] >= limit)
                {
                    Fail.fail("EIDIG1: truncated uvarint (named reason: TRUNCATED_UVARINT)");
                }
                int b = data[at[0]];
                at[0] += 1;
                v |= (long)(b & 0x7F) << shift;
                if ((b & 0x80) == 0)
                {
                    break;
                }
                shift += 7;
                if (shift > 63)
                {
                    Fail.fail("EIDIG1: uvarint overflow (named reason: UVARINT_OVERFLOW)");
                }
            }
            return new int[] { (int)v };
        }

        // ---- FNV-1a 64 over a byte range --------------------------------------
        public static ulong fnv1a(byte[] data, int len)
        {
            ulong h = 0xcbf29ce484222325UL;
            for (int i = 0; i < len; i++)
            {
                h ^= (ulong)data[i];
                h *= 0x100000001b3UL;
            }
            return h;
        }

        // ---- encode: magic|ver|steps|RLE pairs|FNV-1a u64 ---------------------
        public static byte[] encode(Codes c)
        {
            // trim trailing coast/neutral run to last non-neutral step (A-019 §8)
            int n = c.count;
            while (n > 0 && c.angle[n - 1] == (byte)ANGLE_COAST && c.rate[n - 1] == (byte)RATE_NEUTRAL)
            {
                n -= 1;
            }

            // worst case: header 6+2+10, per distinct run 3+10, trailer 8
            int cap = 6 + 2 + 10 + n * 13 + 8;
            byte[] buf = new byte[cap];
            // A-022 latent-bug fix: the cursor must start AFTER the 6 magic bytes
            // and the 2 version bytes. It started at 0, so writeUvarint overwrote
            // the magic and the version. Nothing in phase 1 ever called encode()
            // (the harness reads a committed EIDIG1 fixture, it never writes one),
            // so the bug was latent; H-08 is the first caller and decode's BAD_MAGIC
            // rejection is what surfaced it. The committed golden fixture shows the
            // intended layout: magic, ver u16, then the uvarint step count.
            int[] at = new int[] { 8 };

            buf[0] = (byte)'E';
            buf[1] = (byte)'I';
            buf[2] = (byte)'D';
            buf[3] = (byte)'I';
            buf[4] = (byte)'G';
            buf[5] = (byte)'1';
            buf[6] = 1;
            buf[7] = 0; // ver u16 = 1 LE
            writeUvarint(buf, at, (long)n);

            int i = 0;
            while (i < n)
            {
                int a = c.angle[i];
                int r = c.rate[i];
                int run = 1;
                while (i + run < n && c.angle[i + run] == (byte)a && c.rate[i + run] == (byte)r)
                {
                    run += 1;
                }
                int value = (a << 8) | r;
                buf[at[0]] = (byte)(value & 0xFF);
                buf[at[0] + 1] = (byte)((value >> 8) & 0xFF);
                at[0] += 2;
                writeUvarint(buf, at, (long)run); // runLength >= 1, zero-length illegal
                i += run;
            }

            ulong h = fnv1a(buf, at[0]); // over magic..last pair
            for (int b = 0; b < 8; b++)
            {
                buf[at[0] + b] = (byte)((h >> (8 * b)) & 0xFFL);
            }
            byte[] result = new byte[at[0] + 8];
            for (int k = 0; k < at[0] + 8; k++)
            {
                result[k] = buf[k];
            }
            return result;
        }

        // ---- decode: Intent for one step, hard rejection with named reasons ---
        public static Intent decode(byte[] data, long step)
        {
            int limit = data.Length;
            if (limit < 6 + 2 + 1 + 8)
            {
                Fail.fail("EIDIG1: too short (named reason: TRUNCATED)");
            }
            if (data[0] != (byte)'E' || data[1] != (byte)'I' || data[2] != (byte)'D'
                || data[3] != (byte)'I' || data[4] != (byte)'G' || data[5] != (byte)'1')
            {
                Fail.fail("EIDIG1: unknown magic (named reason: BAD_MAGIC)");
            }
            int ver = data[6] | (data[7] << 8);
            if (ver != 1)
            {
                Fail.fail("EIDIG1: unknown version (named reason: BAD_VERSION)");
            }

            int[] at = new int[] { 8 };
            int[] stepsR = readUvarint(data, at, limit);
            long steps = (long)stepsR[0];

            // verify RLE stream: sum(runLength) == steps, runLength >= 1, codes valid
            long covered = 0L;
            long target = step < 0L ? -1L : step;
            int hitAngle = ANGLE_COAST;
            int hitRate = RATE_NEUTRAL;
            while (covered < steps)
            {
                if (at[0] + 2 > limit)
                {
                    Fail.fail("EIDIG1: truncated pair (named reason: TRUNCATED_PAIR)");
                }
                int value = data[at[0]] | (data[at[0] + 1] << 8);
                at[0] += 2;
                int a = (value >> 8) & 0xFF;
                int r = value & 0xFF;
                if (a > ANGLE_COAST)
                {
                    Fail.fail("EIDIG1: angle code > 16 (named reason: ANGLE_CODE_RANGE)");
                }
                if (r > RATE_MAX)
                {
                    Fail.fail("EIDIG1: rate code > 20 (named reason: RATE_CODE_RANGE)");
                }
                int[] runR = readUvarint(data, at, limit);
                long run = (long)runR[0];
                if (run < 1L)
                {
                    Fail.fail("EIDIG1: zero-length run (named reason: ZERO_RUN)");
                }
                if (covered <= target && target < covered + run)
                {
                    hitAngle = a;
                    hitRate = r;
                }
                covered += run;
            }
            if (at[0] + 8 > limit)
            {
                Fail.fail("EIDIG1: truncated FNV trailer (named reason: TRUNCATED_FNV)");
            }
            ulong h = fnv1a(data, at[0]);
            ulong stored = 0UL;
            for (int b = 0; b < 8; b++)
            {
                stored |= (ulong)data[at[0] + b] << (8 * b);
            }
            if (h != stored)
            {
                Fail.fail("EIDIG1: FNV-1a mismatch (named reason: FNV_MISMATCH)");
            }
            if (covered != steps)
            {
                Fail.fail("EIDIG1: run sum != steps (named reason: RUN_SUM_MISMATCH)");
            }

            // step beyond the recorded (trimmed) stream = coast/neutral
            Intent i0 = new Intent();
            if (step < 0L || step >= steps)
            {
                i0.thrustX = 0L;
                i0.thrustY = 0L;
                i0.rate = rateFromCode(RATE_NEUTRAL);
                return i0;
            }
            i0.thrustX = ANGLE_X[hitAngle];
            i0.thrustY = ANGLE_Y[hitAngle];
            i0.rate = rateFromCode(hitRate);
            return i0;
        }

        // rate code 0..20 -> Q32.32 in [0,1], 10 = neutral 50%
        public static long rateFromCode(int code)
        {
            Fail.check(code >= 0 && code <= RATE_MAX, "InputDigest.rateFromCode: code out of 0..20");
            return MulDiv.muldiv((long)code << 32, FixedQ.ONE, 20L << 32);
        }

        // ---- quantize helpers (used by InputAdapter: raw intent -> codes) -----
        public static int quantizeAngle(long thrustX, long thrustY)
        {
            if (thrustX == 0L && thrustY == 0L)
            {
                return ANGLE_COAST;
            }
            // nearest of 16 by dot product against the unit table (integer math)
            int best = 0;
            long bestDot = 0L;
            for (int k = 0; k < 16; k++)
            {
                long dot = MulDiv.muldiv(thrustX, ANGLE_X[k], FixedQ.ONE)
                         + MulDiv.muldiv(thrustY, ANGLE_Y[k], FixedQ.ONE);
                if (k == 0 || dot > bestDot)
                {
                    bestDot = dot;
                    best = k;
                }
            }
            return best;
        }

        public static int quantizeRate(long rate)
        {
            Fail.check(rate >= 0L && rate <= FixedQ.ONE, "InputDigest.quantizeRate: rate out of [0,1]");
            long scaled = MulDiv.muldiv(rate, 20L << 32, FixedQ.ONE); // value = rate*20
            long code = FixedQ.toIntRound(scaled);
            if (code < 0L)
            {
                code = 0L;
            }
            if (code > (long)RATE_MAX)
            {
                code = (long)RATE_MAX;
            }
            return (int)code;
        }
    }
}

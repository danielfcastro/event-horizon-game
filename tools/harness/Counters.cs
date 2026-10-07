// A-021 Phase 1 prototype — tools/harness/Counters.cs
// Harness-side helpers only: hex/base64 codecs, the EHSNAP1 field walker
// (A-019 §7: H-04 names the first differing field), and the H-05 histogram.
// Never touches SimState; the sim loop stays allocation-free and harness code
// stays out of the sim.
namespace EH
{
    public static class Counters
    {
        // ---- hex -------------------------------------------------------------
        public static string hexEncode(byte[] b, int from, int to)
        {
            string digits = "0123456789abcdef";
            string res = "";
            for (int i = from; i < to; i++)
            {
                int v = b[i] & 0xFF;
                res += "" + digits[(v >> 4) & 15] + digits[v & 15];
            }
            return res;
        }

        public static byte[] hexDecode(string hex)
        {
            string s = "";
            for (int i = 0; i < hex.Length; i++)
            {
                char c = hex[i];
                if (c != ' ' && c != '\n' && c != '\r' && c != '\t')
                {
                    s += "" + c;
                }
            }
            if (s.Length >= 2 && s.Substring(0, 2) == "0x")
            {
                s = s.Substring(2);
            }
            Fail.check(s.Length % 2 == 0, "hexDecode: odd digit count");
            byte[] res = new byte[s.Length / 2];
            for (int i = 0; i < res.Length; i++)
            {
                int hi = dig(s[2 * i]);
                int lo = dig(s[2 * i + 1]);
                Fail.check(hi >= 0 && lo >= 0, "hexDecode: bad hex digit");
                res[i] = (byte)((hi << 4) | lo);
            }
            return res;
        }

        public static int dig(char c)
        {
            if (c >= '0' && c <= '9')
            {
                return c - '0';
            }
            if (c >= 'a' && c <= 'f')
            {
                return 10 + (c - 'a');
            }
            if (c >= 'A' && c <= 'F')
            {
                return 10 + (c - 'A');
            }
            return -1;
        }

        // ---- base64 ----------------------------------------------------------
        static string B64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

        public static string b64Encode(byte[] b, int len)
        {
            string res = "";
            int i = 0;
            while (i + 2 < len)
            {
                int v = ((b[i] & 0xFF) << 16) | ((b[i + 1] & 0xFF) << 8) | (b[i + 2] & 0xFF);
                res += "" + B64[(v >> 18) & 63] + B64[(v >> 12) & 63] + B64[(v >> 6) & 63] + B64[v & 63];
                i += 3;
            }
            int rem = len - i;
            if (rem == 1)
            {
                int v = (b[i] & 0xFF) << 16;
                res += "" + B64[(v >> 18) & 63] + B64[(v >> 12) & 63] + "==";
            }
            else if (rem == 2)
            {
                int v = ((b[i] & 0xFF) << 16) | ((b[i + 1] & 0xFF) << 8);
                res += "" + B64[(v >> 18) & 63] + B64[(v >> 12) & 63] + B64[(v >> 6) & 63] + "=";
            }
            return res;
        }

        public static byte[] b64Decode(string s)
        {
            int n = s.Length;
            while (n > 0 && s[n - 1] == '=')
            {
                n--;
            }
            byte[] res = new byte[(n * 6) / 8];
            int acc = 0;
            int bits = 0;
            int oi = 0;
            for (int i = 0; i < n; i++)
            {
                int v = b64Val(s[i]);
                Fail.check(v >= 0, "b64Decode: bad character");
                acc = (acc << 6) | v;
                bits += 6;
                if (bits >= 8)
                {
                    bits -= 8;
                    res[oi++] = (byte)((acc >> bits) & 0xFF);
                }
            }
            return res;
        }

        public static int b64Val(char c)
        {
            if (c >= 'A' && c <= 'Z')
            {
                return c - 'A';
            }
            if (c >= 'a' && c <= 'z')
            {
                return 26 + (c - 'a');
            }
            if (c >= '0' && c <= '9')
            {
                return 52 + (c - '0');
            }
            if (c == '+')
            {
                return 62;
            }
            if (c == '/')
            {
                return 63;
            }
            return -1;
        }

        // ---- EHSNAP1 field walker (A-019 §7, H-04 names the field) -----------
        // Region sizes: header 113B (magic 7 + ver 2 + stepIndex 8 + seed 8 +
        // prng 32 + hole 56), attractors 2 + 8*count, bodies 2 + 300*51,
        // hazards 2 + 24*51, freeList 2 + 2*count, counters 52B, trailer 8B.
        public static string fieldName(byte[] blob, int blobLen, int off)
        {
            if (off < 0 || off >= blobLen)
            {
                return "out-of-range";
            }
            if (off < 7)
            {
                return "magic";
            }
            if (off < 9)
            {
                return "ver";
            }
            if (off < 17)
            {
                return "stepIndex";
            }
            if (off < 25)
            {
                return "seed";
            }
            if (off < 57)
            {
                return "prng.s" + ((off - 25) / 8);
            }
            if (off < 113)
            {
                string[] hn = { "posX", "posY", "velX", "velY", "mass", "radius", "eventRadius" };
                return "hole." + hn[(off - 57) / 8];
            }
            int ac = u16(blob, 113);
            if (off < 115)
            {
                return "attractors.count";
            }
            if (off < 115 + 8 * ac)
            {
                return "attractors.radius[" + ((off - 115) / 8) + "]";
            }
            int bStart = 115 + 8 * ac;
            if (off < bStart + 2)
            {
                return "bodies.capacity";
            }
            int bRec = bStart + 2;
            if (off < bRec + SimState.BODY_CAPACITY * 51)
            {
                return recordName("body", bRec, off);
            }
            int hStart = bRec + SimState.BODY_CAPACITY * 51;
            if (off < hStart + 2)
            {
                return "hazards.capacity";
            }
            int hRec = hStart + 2;
            if (off < hRec + SimState.HAZARD_CAPACITY * 51)
            {
                return recordName("hazard", hRec, off);
            }
            int flStart = hRec + SimState.HAZARD_CAPACITY * 51;
            if (off < flStart + 2)
            {
                return "freeList.count";
            }
            int fc = u16(blob, flStart);
            if (off < flStart + 2 + 2 * fc)
            {
                return "freeList[" + ((off - flStart - 2) / 2) + "]";
            }
            int cStart = flStart + 2 + 2 * fc;
            int r = off - cStart;
            if (r < 8)
            {
                return "counters.score";
            }
            if (r < 16)
            {
                return "counters.stability";
            }
            if (r < 24)
            {
                return "counters.combo";
            }
            if (r < 28)
            {
                return "counters.absorbedTotal";
            }
            if (r < 30)
            {
                return "counters.highmarkSmall";
            }
            if (r < 32)
            {
                return "counters.highmarkMedium";
            }
            if (r < 34)
            {
                return "counters.highmarkLarge";
            }
            if (r < 36)
            {
                return "counters.highmarkHazard";
            }
            if (r < 44)
            {
                return "counters.stepsRun";
            }
            if (r < 52)
            {
                return "counters.droppedSteps";
            }
            if (r < 60)
            {
                return "digest";
            }
            return "out-of-range";
        }

        public static int u16(byte[] b, int at)
        {
            return (b[at] & 0xFF) | ((b[at + 1] & 0xFF) << 8);
        }

        public static string recordName(string prefix, int recStart, int off)
        {
            int i = (off - recStart) / 51;
            int r = (off - recStart) % 51;
            string name;
            if (r < 1)
            {
                name = "flags";
            }
            else if (r < 2)
            {
                name = "category";
            }
            else if (r < 3)
            {
                name = "typeId";
            }
            else
            {
                string[] f = { "x", "y", "vx", "vy", "mass", "radius" };
                name = f[(r - 3) / 8];
            }
            return prefix + "[" + i + "]." + name;
        }

        // ---- H-05 histogram --------------------------------------------------
        public static string histogram(long[] samples, int n, long[] edges, string[] labels)
        {
            int[] counts = new int[edges.Length + 1];
            for (int i = 0; i < n; i++)
            {
                int bkt = edges.Length;
                for (int e = 0; e < edges.Length; e++)
                {
                    if (samples[i] < edges[e])
                    {
                        bkt = e;
                        break;
                    }
                }
                counts[bkt]++;
            }
            string res = "";
            for (int e = 0; e <= edges.Length; e++)
            {
                if (e > 0)
                {
                    res += " ";
                }
                res += labels[e] + "=" + counts[e];
            }
            return res;
        }
    }
}

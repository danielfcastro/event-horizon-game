// A-021 Phase 1 prototype — tools/harness/Snapshot.cs
// EHSNAP1 snapshot codec (A-019 §7, restated in brief §4/A-021 §7). Byte-exactness
// is the contract: all little-endian, no padding, append-only field order.
//
// A-019 §7 ambiguity decision (documented here per brief §4): §7 names six hole
// fields (posX posY velX velY mass radius) but specifies 7 i64s; the seventh is
// eventRadius (holeEventRadius in SimState). §7 field order is append-only.
//
// Format (EHSNAP1):
//   magic "EHSNAP1" | ver u16=1 | stepIndex i64 | seed u64 | prng 4×u64 |
//   hole 7×i64 (posX posY velX velY mass radius eventRadius) |
//   attractors u16 count + count×i64 radii (index order) |
//   bodies u16 capacity(300) then per index ascending: u8 flags | i8 category |
//     i8 typeId | 6×i64 posX posY velX velY mass radius (inactive = zero record) |
//   hazards u16 capacity(24) same record |
//   freeList u16 count + ascending u16 indices |
//   counters: score|stability|combo i64, absorbedTotal u32, highmarks 4×u16,
//     stepsRun/droppedSteps i64 |
//   trailing FNV-1a u64 over every preceding byte.
//
// Harness-side codec: pure C#, stdlib only, no Unity API. SimState field names
// per Assets/Runtime/SimCore/SimState.cs (read on disk; not rewritten here).
namespace EH
{
    public static class Snapshot
    {
        // ---- growable byte buffer + writer/reader cursors ---------------------
        public static byte[] buf;
        public static int len;   // writer cursor == bytes written
        public static int pos;   // reader cursor

        public static void reset()
        {
            if (buf == null)
            {
                buf = new byte[4096];
            }
            len = 0;
            pos = 0;
        }

        public static void ensure(int n)
        {
            if (len + n <= buf.Length)
            {
                return;
            }
            int cap = buf.Length;
            while (cap < len + n)
            {
                cap *= 2;
            }
            byte[] grown = new byte[cap];
            for (int i = 0; i < len; i++)
            {
                grown[i] = buf[i];
            }
            buf = grown;
        }

        // ---- little-endian byte primitives ------------------------------------
        public static void putU8(int v)
        {
            ensure(1);
            buf[len++] = (byte)(v & 0xFF);
        }

        public static void putU16(int v)
        {
            ensure(2);
            buf[len++] = (byte)(v & 0xFF);
            buf[len++] = (byte)((v >> 8) & 0xFF);
        }

        public static void putU32(long v)
        {
            ensure(4);
            buf[len++] = (byte)(v & 0xFF);
            buf[len++] = (byte)((v >> 8) & 0xFF);
            buf[len++] = (byte)((v >> 16) & 0xFF);
            buf[len++] = (byte)((v >> 24) & 0xFF);
        }

        public static void putU64(ulong v)
        {
            ensure(8);
            for (int k = 0; k < 8; k++)
            {
                buf[len++] = (byte)((v >> (8 * k)) & 0xFF);
            }
        }

        public static void putI64(long v)
        {
            putU64((ulong)v);
        }

        public static int readU8()
        {
            Fail.check(pos + 1 <= len, "Snapshot.readU8: past end of buffer");
            return buf[pos++] & 0xFF;
        }

        public static int readU16()
        {
            Fail.check(pos + 2 <= len, "Snapshot.readU16: past end of buffer");
            int v = buf[pos] | (buf[pos + 1] << 8);
            pos += 2;
            return v & 0xFFFF;
        }

        public static long readU32()
        {
            Fail.check(pos + 4 <= len, "Snapshot.readU32: past end of buffer");
            long v = (long)(buf[pos] | (buf[pos + 1] << 8) | (buf[pos + 2] << 16))
                   | ((long)(buf[pos + 3] & 0xFF) << 24);
            pos += 4;
            return v & 0xFFFFFFFFL;
        }

        public static ulong readU64()
        {
            Fail.check(pos + 8 <= len, "Snapshot.readU64: past end of buffer");
            ulong v = 0;
            for (int k = 0; k < 8; k++)
            {
                v |= (ulong)(buf[pos + k] & 0xFF) << (8 * k);
            }
            pos += 8;
            return v;
        }

        public static long readI64()
        {
            return (long)readU64();
        }

        // ---- FNV-1a 64 (trailing digest, A-019 §7) -----------------------------
        public static readonly ulong FNV_OFFSET = 0xcbf29ce484222325UL;
        public static readonly ulong FNV_PRIME = 0x100000001b3UL;

        public static ulong fnv1a(int from, int to)
        {
            ulong h = FNV_OFFSET;
            for (int i = from; i < to; i++)
            {
                h ^= (ulong)(buf[i] & 0xFF);
                h *= FNV_PRIME;
            }
            return h;
        }

        // seal: append the trailing FNV-1a u64 over every preceding byte.
        public static void seal()
        {
            ulong h = fnv1a(0, len);
            putU64(h);
        }

        // checkSeal: validate the trailing digest; hard-reject on mismatch.
        public static bool checkSeal()
        {
            Fail.check(len >= 8, "Snapshot.checkSeal: buffer too short");
            ulong stored = 0;
            for (int k = 0; k < 8; k++)
            {
                stored |= (ulong)(buf[len - 8 + k] & 0xFF) << (8 * k);
            }
            return fnv1a(0, len - 8) == stored;
        }

        // ---- region emitters / readers (filled in from §7, one per edit) ------
        // Header: magic "EHSNAP1" | ver u16=1 | stepIndex i64 | seed u64 |
        // prng 4×u64 | hole 7×i64 (seventh = eventRadius, A-019 decision).
        public static void emitHeader(SimState s)
        {
            // "EHSNAP1" magic, 7 ASCII bytes
            byte[] magic = { 0x45, 0x48, 0x53, 0x4E, 0x41, 0x50, 0x31 };
            for (int i = 0; i < magic.Length; i++)
            {
                putU8(magic[i]);
            }
            putU16(1);
            putI64(s.stepIndex);
            putU64(s.seed);
            putU64(s.s0);
            putU64(s.s1);
            putU64(s.s2);
            putU64(s.s3);
            putI64(s.holeX);
            putI64(s.holeY);
            putI64(s.holeVX);
            putI64(s.holeVY);
            putI64(s.holeMass);
            putI64(s.holeRadius);
            putI64(s.holeEventRadius); // seventh hole i64 = eventRadius (A-019 §7 decision)
        }

        // Attractors: u16 count + count×i64 radii, attractor-index order (§7).
        public static void emitAttractors(SimState s)
        {
            Fail.check(s.attractorCount >= 0 && s.attractorCount <= SimState.ATTRACTOR_MAX,
                "Snapshot.emitAttractors: count out of range");
            putU16(s.attractorCount);
            for (int k = 0; k < s.attractorCount; k++)
            {
                putI64(s.attractorRadius[k]);
            }
        }

        // Bodies: u16 capacity(300), then per index ASCENDING the record
        // u8 flags | i8 category | i8 typeId | 6×i64 posX posY velX velY mass
        // radius. Inactive indices emit a zero record (BodyPool.free zeroes it).
        public static void emitBodies(SimState s)
        {
            putU16(SimState.BODY_CAPACITY);
            for (int i = 0; i < SimState.BODY_CAPACITY; i++)
            {
                putU8(s.bodyFlags[i]);
                putU8(s.bodyCategory[i]);
                putU8(s.bodyTypeId[i]);
                putI64(s.bodyX[i]);
                putI64(s.bodyY[i]);
                putI64(s.bodyVX[i]);
                putI64(s.bodyVY[i]);
                putI64(s.bodyMass[i]);
                putI64(s.bodyRadius[i]);
            }
        }

        // Hazards: u16 capacity(24), then the same record per index ascending.
        public static void emitHazards(SimState s)
        {
            putU16(SimState.HAZARD_CAPACITY);
            for (int i = 0; i < SimState.HAZARD_CAPACITY; i++)
            {
                putU8(s.hazardFlags[i]);
                putU8(s.hazardCategory[i]);
                putU8(s.hazardTypeId[i]);
                putI64(s.hazardX[i]);
                putI64(s.hazardY[i]);
                putI64(s.hazardVX[i]);
                putI64(s.hazardVY[i]);
                putI64(s.hazardMass[i]);
                putI64(s.hazardRadius[i]);
            }
        }

        // FreeList: u16 count + count ascending u16 indices (§7).
        public static void emitFreeList(SimState s)
        {
            Fail.check(s.freeCount >= 0 && s.freeCount <= 0xFFFF,
                "Snapshot.emitFreeList: count out of u16 range");
            putU16(s.freeCount);
            for (int k = 0; k < s.freeCount; k++)
            {
                Fail.check(s.freeList[k] >= 0 && s.freeList[k] <= 0xFFFF,
                    "Snapshot.emitFreeList: index out of u16 range");
                putU16(s.freeList[k]);
            }
        }

        // Counters: score|stability|combo i64, absorbedTotal u32,
        // highmarkSmall/Medium/Large/Hazard 4×u16, stepsRun/droppedSteps i64.
        public static void emitCounters(SimState s)
        {
            putI64(s.score);
            putI64(s.stability);
            putI64(s.combo);
            putU32((long)(s.absorbedTotal & 0xFFFFFFFFu));
            putU16(s.highmarkSmall);
            putU16(s.highmarkMedium);
            putU16(s.highmarkLarge);
            putU16(s.highmarkHazard);
            putI64(s.stepsRun);
            putI64(s.droppedSteps);
        }

        public static void readHeader(SimState s)
        {
            // Mirror of emitHeader: same field order, hard-fail named reasons.
            byte[] magic = { 0x45, 0x48, 0x53, 0x4E, 0x41, 0x50, 0x31 };
            for (int i = 0; i < magic.Length; i++)
            {
                Fail.check(readU8() == magic[i], "Snapshot.readHeader: bad magic, not EHSNAP1");
            }
            int ver = readU16();
            Fail.check(ver == 1, "Snapshot.readHeader: unsupported version");
            s.stepIndex = readI64();
            s.seed = readU64();
            s.s0 = readU64();
            s.s1 = readU64();
            s.s2 = readU64();
            s.s3 = readU64();
            s.holeX = readI64();
            s.holeY = readI64();
            s.holeVX = readI64();
            s.holeVY = readI64();
            s.holeMass = readI64();
            s.holeRadius = readI64();
            s.holeEventRadius = readI64(); // seventh hole i64 = eventRadius (A-019 §7 decision)
        }

        public static void readAttractors(SimState s)
        {
            // Mirror of emitAttractors: u16 count + count×i64 radii, index order.
            int count = readU16();
            Fail.check(count >= 0 && count <= SimState.ATTRACTOR_MAX,
                "Snapshot.readAttractors: count out of range");
            s.attractorCount = count;
            for (int k = 0; k < count; k++)
            {
                s.attractorRadius[k] = readI64();
            }
        }

        public static void readBodies(SimState s)
        {
            // Mirror of emitBodies: u16 capacity(300), per index ascending
            // u8 flags | i8 category | i8 typeId | 6×i64.
            int cap = readU16();
            Fail.check(cap == SimState.BODY_CAPACITY, "Snapshot.readBodies: capacity mismatch");
            for (int i = 0; i < cap; i++)
            {
                s.bodyFlags[i] = (byte)readU8();
                s.bodyCategory[i] = (byte)readU8();
                s.bodyTypeId[i] = (byte)readU8();
                s.bodyX[i] = readI64();
                s.bodyY[i] = readI64();
                s.bodyVX[i] = readI64();
                s.bodyVY[i] = readI64();
                s.bodyMass[i] = readI64();
                s.bodyRadius[i] = readI64();
            }
        }

        public static void readHazards(SimState s)
        {
            // Mirror of emitHazards: u16 capacity(24), same record per index.
            int cap = readU16();
            Fail.check(cap == SimState.HAZARD_CAPACITY, "Snapshot.readHazards: capacity mismatch");
            for (int i = 0; i < cap; i++)
            {
                s.hazardFlags[i] = (byte)readU8();
                s.hazardCategory[i] = (byte)readU8();
                s.hazardTypeId[i] = (byte)readU8();
                s.hazardX[i] = readI64();
                s.hazardY[i] = readI64();
                s.hazardVX[i] = readI64();
                s.hazardVY[i] = readI64();
                s.hazardMass[i] = readI64();
                s.hazardRadius[i] = readI64();
            }
        }

        public static void readFreeList(SimState s)
        {
            // Mirror of emitFreeList: u16 count + count ascending u16 indices.
            int count = readU16();
            Fail.check(count >= 0 && count <= 0xFFFF, "Snapshot.readFreeList: count out of u16 range");
            s.freeCount = count;
            int prev = -1;
            for (int k = 0; k < count; k++)
            {
                int idx = readU16();
                Fail.check(idx > prev, "Snapshot.readFreeList: indices not strictly ascending");
                prev = idx;
                s.freeList[k] = idx;
            }
        }

        public static void readCounters(SimState s)
        {
            // Mirror of emitCounters, then validate the trailing FNV-1a seal.
            s.score = readI64();
            s.stability = readI64();
            s.combo = readI64();
            s.absorbedTotal = (uint)readU32();
            s.highmarkSmall = readU16();
            s.highmarkMedium = readU16();
            s.highmarkLarge = readU16();
            s.highmarkHazard = readU16();
            s.stepsRun = readI64();
            s.droppedSteps = readI64();
            Fail.check(checkSeal(), "Snapshot.readCounters: seal mismatch");
        }
    }
}

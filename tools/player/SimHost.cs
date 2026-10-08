// A-026 Non-Unity player package — tools/player/SimHost.cs
//
// The C# half of the two-process player. It owns the simulation and the frame
// clock; tools/player/view.c owns the window and the keyboard. Two processes
// because this .NET 8 toolchain cannot call a C library from C# (probed:
// `#pragma DLI_Import` -> CS1633 "Unrecognized #pragma directive", `extern "C"`
// -> CS1003 "'alias' expected"), so DLI is unavailable and the only honest
// shape is one simulation reached from another process.
//
// Hard rules honoured here, all of them PLAN.md / A-019 rules:
//  * There is exactly ONE simulation loop (A-019 §11). This file never steps.
//    One frame calls FrameDriver.frameLive, which calls FixedStepDriver.advance
//    and owns the accumulator, DT = 1/60, MAX_CATCHUP and the dropped-step rule.
//    Nothing here re-implements any of that.
//  * The renderer (view.c) READS state; it never writes it. The only thing that
//    crosses from the renderer to here is a KEY STATE request (which direction
//    the player is pressing), never a position, a mass, or a step.
//  * pixelsPerUnit is render-only (A-019 §6): it is copied INTO the payload for
//    the renderer's benefit and never read by SimCore.
//  * No float/double anywhere. Every coordinate is the Q32.32 raw value from
//    SimState, copied verbatim, so a payload is byte-comparable to a snapshot.
//  * Live input is quantized through the SAME table the replay channel uses
//    (InputDigest.quantizeAngle -> ANGLE_X/ANGLE_Y), so live and replay agree.
//
// ---------------------------------------------------------------------------
// File protocol (both halves implement it identically; see view.c)
// ---------------------------------------------------------------------------
//
// frame payload file, big-endian, written by this process every frame:
//   0   "EHFRM1"        6 bytes magic
//   6   seq             u32   frame sequence, monotonic
//   10  step            i64   SimState.stepIndex after the frame
//   18  holeX           i64   Q32.32 world units
//   26  holeY           i64   Q32.32 world units
//   34  holeRadius      i64   Q32.32
//   42  holeMass        i64   Q32.32
//   50  bodyCount       u16   active bodies (bodyFlags bit0)
//   52  bodyCount x ( i64 x, i64 y, i64 radius, u8 kind ) = 25 bytes each
//       pixelsPerUnit   u64   Q32.32, render-only
//       resultFlag      u8    0 running, 1 goal, 2 timeout
//       snapshotLen     u32   0 for live frames; a dump frame carries the
//                             canonical snapshot bytes emitted by the ONE
//                             harness emitter (Commands.emitSnapshot)
//       snapshot bytes  snapshotLen
//       checksum        u32   FNV-1a 32 over every preceding byte
//
// key state file, written by view.c, read by this process every frame:
//   0   "EHKEY1"        6 bytes magic
//   6   dx              i32   screen-space direction, -1..1, y is screen-down
//   10  dy              i32
//   14  rateCode        u8    0..20 (InputDigest rate code)
//   15  quitFlag        u8    1 = the player pressed quit
//   16  checksum        u32   FNV-1a 32 over bytes 0..15
//
// A torn read is a render-only glitch, never a simulation effect: both halves
// validate magic + checksum and keep the previous value when validation fails.
//
// Modes:
//   --live (default)  wall-clock paced, reads key state, playable
//   --replay          no pacing, input comes from the blessed EIDIG1 digest
//                     through InputAdapter.sampleFromDigest, so the run must
//                     reproduce H-01 exactly (goal at step 2828)
//   --dump-step N --dump-file P
//                     at step N write one payload carrying the canonical
//                     snapshot bytes to P, for the payload-vs-snapshot check
//
namespace EH
{
    public static class SimHost
    {
        // ---- protocol constants (view.c repeats these; they must agree) ------
        public static readonly string FRAME_MAGIC = "EHFRM1";
        public static readonly string KEY_MAGIC = "EHKEY1";
        public static readonly long FNV32_OFFSET = 2166136261L;
        public static readonly long FNV32_PRIME = 16777619L;
        public static readonly long MASK32 = 0xFFFFFFFFL;

        // 1 second = 10,000,000 ticks (System.DateTime.UtcNow, the same clock
        // the harness reads at Commands.cs:574). Pacing is wall-clock only and
        // lives entirely OUTSIDE the simulation loop.
        public static readonly long TICKS_PER_SECOND = 10000000L;

        // ---- payload scratch, allocated once at boot --------------------------
        public static byte[] pay = new byte[1 << 16];
        public static int payLen = 0;

        // ---- payload writer --------------------------------------------------
        public static int putI64(long v)
        {
            pay[payLen++] = (byte)((v >> 56) & 0xFFL);
            pay[payLen++] = (byte)((v >> 48) & 0xFFL);
            pay[payLen++] = (byte)((v >> 40) & 0xFFL);
            pay[payLen++] = (byte)((v >> 32) & 0xFFL);
            pay[payLen++] = (byte)((v >> 24) & 0xFFL);
            pay[payLen++] = (byte)((v >> 16) & 0xFFL);
            pay[payLen++] = (byte)((v >> 8) & 0xFFL);
            pay[payLen++] = (byte)(v & 0xFFL);
            return payLen;
        }

        public static int putU32(long v)
        {
            pay[payLen++] = (byte)((v >> 24) & 0xFFL);
            pay[payLen++] = (byte)((v >> 16) & 0xFFL);
            pay[payLen++] = (byte)((v >> 8) & 0xFFL);
            pay[payLen++] = (byte)(v & 0xFFL);
            return payLen;
        }

        public static int putU16(int v)
        {
            pay[payLen++] = (byte)((v >> 8) & 0xFF);
            pay[payLen++] = (byte)(v & 0xFF);
            return payLen;
        }

        public static int putU8(int v)
        {
            pay[payLen++] = (byte)(v & 0xFF);
            return payLen;
        }

        public static void putMagic(string m)
        {
            for (int k = 0; k < m.Length; k++)
            {
                pay[payLen++] = (byte)m[k];
            }
        }

        public static long checksum32()
        {
            long h = FNV32_OFFSET;
            for (int k = 0; k < payLen; k++)
            {
                h = ((h ^ (long)(pay[k] & 0xFF)) * FNV32_PRIME) & MASK32;
            }
            return h;
        }

        // ---- key state reader -------------------------------------------------
        // Returns false when the file is missing or fails validation, in which
        // case the caller coasts (a missing key file is "no key pressed", never
        // a simulation failure).
        public static bool readKey(string path, out int dx, out int dy,
            out int rateCode, out int quit)
        {
            dx = 0;
            dy = 0;
            rateCode = InputDigest.RATE_NEUTRAL;
            quit = 0;
            if (!System.IO.File.Exists(path))
            {
                return false;
            }
            byte[] b = System.IO.File.ReadAllBytes(path);
            if (b.Length < 17)
            {
                return false;
            }
            for (int k = 0; k < KEY_MAGIC.Length; k++)
            {
                if (b[k] != (byte)KEY_MAGIC[k])
                {
                    return false;
                }
            }
            long h = FNV32_OFFSET;
            for (int k = 0; k < 16; k++)
            {
                h = ((h ^ (long)(b[k] & 0xFF)) * FNV32_PRIME) & MASK32;
            }
            long want = (((long)b[16] & 0xFF) << 24) | (((long)b[17] & 0xFF) << 16) |
                (((long)b[18] & 0xFF) << 8) | ((long)b[19] & 0xFF);
            if (h != want)
            {
                return false; // torn write: keep coasting, never corrupt a run
            }
            dx = (int)((((long)b[6] & 0xFF) << 24) | (((long)b[7] & 0xFF) << 16) |
                (((long)b[8] & 0xFF) << 8) | ((long)b[9] & 0xFF));
            dy = (int)((((long)b[10] & 0xFF) << 24) | (((long)b[11] & 0xFF) << 16) |
                (((long)b[12] & 0xFF) << 8) | ((long)b[13] & 0xFF));
            rateCode = b[14] & 0xFF;
            quit = b[15] & 0xFF;
            if (rateCode < 0 || rateCode > InputDigest.RATE_MAX)
            {
                rateCode = InputDigest.RATE_NEUTRAL;
            }
            return true;
        }

        // ---- one frame's Intent from live key state -----------------------------
        // The direction is quantized by InputDigest.quantizeAngle, i.e. the SAME
        // 16-way table the replay channel decodes through, so a key held down
        // here and the same angle in a digest produce the same Intent.
        // Screen y is down; world y is up, hence the sign flip.
        public static Intent liveIntent(int dx, int dy, int rateCode)
        {
            long sx = (long)dx * FixedQ.ONE;
            long sy = (long)(-dy) * FixedQ.ONE;
            int code = InputDigest.quantizeAngle(sx, sy);
            Intent i = new Intent();
            i.thrustX = InputDigest.ANGLE_X[code];
            i.thrustY = InputDigest.ANGLE_Y[code];
            i.rate = InputDigest.rateFromCode(rateCode);
            return i;
        }

        // ---- write the payload atomically-enough for a reader -------------------
        // The renderer validates magic + checksum, so a half-written payload is
        // rejected and the previous frame is kept; no rename primitive is
        // available in this toolchain (System.IO.FileSystem.Rename -> CS0234).
        public static void writePayload(string path)
        {
            // The checksum is appended to a fresh array, never into the shared
            // buffer: writing the live frame and then the dump frame from the
            // same buffer must produce identical bytes, and a writer that
            // mutated payLen would give the dump a second checksum.
            long c = checksum32();
            byte[] sized = new byte[payLen + 4];
            for (int k = 0; k < payLen; k++)
            {
                sized[k] = pay[k];
            }
            sized[payLen] = (byte)((c >> 24) & 0xFFL);
            sized[payLen + 1] = (byte)((c >> 16) & 0xFFL);
            sized[payLen + 2] = (byte)((c >> 8) & 0xFFL);
            sized[payLen + 3] = (byte)(c & 0xFFL);
            System.IO.File.WriteAllBytes(path, sized);
        }

        public static int Main(string[] argv)
        {
            // ---- args ----------------------------------------------------------
            string level = LevelTable.levelId;
            ulong seed = LevelTable.defaultSeed;
            bool replay = false;
            byte[] digest = new byte[0];
            string framePath = "artifacts/player/frame.bin";
            string keyPath = "artifacts/player/key.bin";
            long dumpStep = -1L;
            string dumpPath = "";
            long maxFrames = LevelTable.timerSeconds * 60L;

            // argv[0] is the first user argument, not a program name: the
            // harness dispatches on argv[0] == "sim" (main.cs:141), so the
            // player must start scanning at 0 or it eats its first flag.
            int k = 0;
            while (k < argv.Length)
            {
                string a = argv[k];
                if (a == "--level")
                {
                    level = argv[k + 1];
                    k += 2;
                }
                else if (a == "--seed")
                {
                    seed = Commands.parseHexU64(argv[k + 1]);
                    k += 2;
                }
                else if (a == "--digest")
                {
                    digest = Commands.readBytes(argv[k + 1]);
                    k += 2;
                }
                else if (a == "--replay")
                {
                    replay = true;
                    k += 1;
                }
                else if (a == "--live")
                {
                    // usage advertises [--live|--replay]; live is the default
                    // mode, so the flag must be accepted as an explicit no-op
                    // rather than falling through to the usage error (found by
                    // running the windowed test: --live hit the else and exit 2).
                    replay = false;
                    k += 1;
                }
                else if (a == "--frame")
                {
                    framePath = argv[k + 1];
                    k += 2;
                }
                else if (a == "--key")
                {
                    keyPath = argv[k + 1];
                    k += 2;
                }
                else if (a == "--dump-step")
                {
                    dumpStep = Commands.parseLong(argv[k + 1]);
                    k += 2;
                }
                else if (a == "--dump-file")
                {
                    dumpPath = argv[k + 1];
                    k += 2;
                }
                else if (a == "--max-frames")
                {
                    maxFrames = Commands.parseLong(argv[k + 1]);
                    k += 2;
                }
                else
                {
                    System.Console.Write(
                        "usage: player [--live|--replay] [--level <id>] [--seed <hex>]" +
                        " [--digest @file]\n" +
                        "            [--frame <path>] [--key <path>]" +
                        " [--dump-step N --dump-file <path>] [--max-frames N]\n");
                    return 2;
                }
            }
            if (replay && digest.Length == 0)
            {
                System.Console.Write("player: --replay needs --digest @file (no input to replay)\n");
                return 2;
            }

            System.IO.Directory.CreateDirectory("artifacts/player");

            // ---- boot: the ONE simulation, reached through the frame driver ----
            SimState s = LevelLoader.load(level, seed);
            FrameDriver.FrameClock c = FrameDriver.boot(60, FixedQ.ONE, 0);
            long budget = FrameDriver.frameBudget(60);
            long tickStart = System.DateTime.UtcNow.Ticks;

            int result = 0; // 0 running, 1 goal, 2 timeout
            long frames = 0L;
            int executed = 0;
            int maxExecuted = 0;
            bool quit = false;

            while (true)
            {
                Intent i;
                if (replay)
                {
                    i = InputAdapter.sampleFromDigest(digest, s.stepIndex);
                }
                else
                {
                    int dx = 0;
                    int dy = 0;
                    int rateCode = InputDigest.RATE_NEUTRAL;
                    int quitFlag = 0;
                    readKey(keyPath, out dx, out dy, out rateCode, out quitFlag);
                    if (quitFlag != 0)
                    {
                        quit = true;
                        break;
                    }
                    i = liveIntent(dx, dy, rateCode);
                }

                // The documented contract FixedStepDriver.step asserts: the
                // caller installs the intent into state before the frame runs
                // (Commands.cs:211 does the same for the step-driven loop).
                // A simulation failure exits 2 with its reason (A-019 §9),
                // it must not abort the process (exit 134, core dumped).
                s.intent = i;
                try
                {
                    executed = FrameDriver.frameLive(c, s, i);
                }
                catch (SystemicFailure e)
                {
                    System.Console.Write("player: " + e.reason + " (run did not complete)\n");
                    return 2;
                }
                frames += 1L;
                if (executed > maxExecuted)
                {
                    maxExecuted = executed;
                }

                // ---- payload for this frame ------------------------------------
                payLen = 0;
                putMagic(FRAME_MAGIC);
                putU32(frames);
                putI64(s.stepIndex);
                putI64(s.holeX);
                putI64(s.holeY);
                putI64(s.holeRadius);
                putI64(s.holeMass);
                int active = 0;
                for (int b = 0; b < SimState.BODY_CAPACITY; b++)
                {
                    if ((s.bodyFlags[b] & 1) != 0)
                    {
                        active += 1;
                    }
                }
                putU16(active);
                for (int b = 0; b < SimState.BODY_CAPACITY; b++)
                {
                    if ((s.bodyFlags[b] & 1) != 0)
                    {
                        putI64(s.bodyX[b]);
                        putI64(s.bodyY[b]);
                        putI64(s.bodyRadius[b]);
                        putU8(s.bodyFlags[b]);
                    }
                }
                putI64(c.pixelsPerUnit);
                if (c.goalReached)
                {
                    result = 1;
                }
                putU8(result);
                long snapLen = 0L;
                if (dumpStep >= 0L && s.stepIndex == dumpStep && dumpPath.Length > 0)
                {
                    byte[] snap = Commands.emitSnapshot(s);
                    snapLen = (long)snap.Length;
                    putU32(snapLen);
                    for (int z = 0; z < snap.Length; z++)
                    {
                        pay[payLen++] = snap[z];
                    }
                }
                else
                {
                    putU32(0L);
                }
                writePayload(framePath);
                if (snapLen > 0L)
                {
                    writePayload(dumpPath);
                }

                // ---- stop conditions -------------------------------------------
                if (c.goalReached)
                {
                    result = 1;
                    break;
                }
                if (s.stepIndex >= LevelTable.timerSeconds * 60L)
                {
                    result = 2;
                    break;
                }
                if (frames >= maxFrames)
                {
                    result = 2;
                    break;
                }

                // ---- wall-clock pacing, live mode only --------------------------
                // The live device is a real 60 fps device: one frame is due every
                // 1/60 s of WALL time. This is the wall clock, never DT; DT stays
                // 71582788L and is owned by FixedStepDriver.
                if (!replay)
                {
                    long due = tickStart + frames * (TICKS_PER_SECOND / 60L);
                    while (System.DateTime.UtcNow.Ticks < due)
                    {
                    }
                }
            }

            System.Console.Write(
                "player: mode=" + (replay ? "replay" : "live") +
                " frames=" + frames +
                " steps=" + s.stepIndex +
                " goalStep=" + c.framesToGoal +
                " goalReached=" + c.goalReached +
                " maxFrameSteps=" + maxExecuted +
                " droppedSteps=" + s.droppedSteps +
                " quitRequested=" + quit +
                " result=" + (result == 1 ? "goal" : (result == 2 ? "timeout" : "quit")) +
                " exit=0\n");
            return 0;
        }

    }
}

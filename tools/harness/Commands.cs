// A-021 Phase 1 prototype — tools/harness/Commands.cs
// The H-01..H-09 command surface (A-019 §9). H-01..H-05 run real against
// SimCore; H-06..H-09 honestly report that the frame driver and render state
// do not exist in phase 1 (exit 2, never a silent pass). Every command files
// artifacts/harness/<runId>/manifest.json plus <test-id>.json (A-019 §11).
namespace EH
{
    public static class Commands
    {
        // ---- filing identity for the current invocation ----------------------
        public static string testId = "";
        public static string commandName = "";
        public static string seedHex = "0";
        public static byte[] digest = new byte[0];

        public static int run(string[] argv)
        {
            if (argv.Length < 1)
            {
                usage();
                return 2;
            }
            string cmd = argv[0];
            if (cmd == "sim")
            {
                testId = "H-01";
                commandName = "sim";
                return sim(argv, 1);
            }
            if (cmd == "replay")
            {
                testId = "H-02";
                commandName = "replay";
                return replay(argv, 1);
            }
            if (cmd == "snap")
            {
                testId = "H-03";
                commandName = "snap";
                return snap(argv, 1);
            }
            if (cmd == "diff")
            {
                testId = "H-04";
                commandName = "diff";
                return diff(argv, 1);
            }
            if (cmd == "counters")
            {
                testId = "H-05";
                commandName = "counters";
                return counters(argv, 1);
            }
            if (cmd == "hitch")
            {
                testId = "H-06";
                commandName = "hitch";
                if (!HitchInjector.available())
                {
                    return notAvailable(cmd);
                }
                return HitchInjector.inject(argv, 1);
            }
            if (cmd == "fps")
            {
                testId = "H-07";
                commandName = "fps";
                return FrameCommands.fps(argv, 1);
            }
            if (cmd == "pointer")
            {
                testId = "H-08";
                commandName = "pointer";
                return FrameCommands.pointer(argv, 1);
            }
            if (cmd == "tier")
            {
                testId = "H-09";
                commandName = "tier";
                return FrameCommands.tier(argv, 1);
            }
            usage();
            return 2;
        }

        // ---- argument parsing (shared knobs, A-019 §9) -----------------------
        public static ulong pSeed;
        public static byte[] pDigest;
        public static string pLevel;
        public static long pStep;
        public static string pOut;

        public static bool parseCommon(string[] argv, int at)
        {
            pSeed = 0UL;
            pDigest = null;
            pLevel = null;
            pStep = -1L;
            pOut = null;
            int k = at;
            while (k < argv.Length)
            {
                if (argv[k] == "--seed")
                {
                    Fail.check(k + 1 < argv.Length, "missing value after --seed");
                    pSeed = parseHexU64(argv[k + 1]);
                    k += 2;
                }
                else if (argv[k] == "--digest")
                {
                    Fail.check(k + 1 < argv.Length, "missing value after --digest");
                    pDigest = readBytes(argv[k + 1]);
                    k += 2;
                }
                else if (argv[k] == "--level")
                {
                    Fail.check(k + 1 < argv.Length, "missing value after --level");
                    pLevel = argv[k + 1];
                    k += 2;
                }
                else if (argv[k] == "--step")
                {
                    Fail.check(k + 1 < argv.Length, "missing value after --step");
                    pStep = parseLong(argv[k + 1]);
                    k += 2;
                }
                else if (argv[k] == "--out")
                {
                    Fail.check(k + 1 < argv.Length, "missing value after --out");
                    pOut = argv[k + 1];
                    k += 2;
                }
                else
                {
                    return false;
                }
            }
            return pDigest != null && pLevel != null;
        }

        public static byte[] readBytes(string arg)
        {
            if (arg.Length > 0 && arg[0] == '@')
            {
                return System.IO.File.ReadAllBytes(arg.Substring(1));
            }
            return Counters.hexDecode(arg);
        }

        public static long parseLong(string s)
        {
            long v = 0L;
            bool neg = false;
            int k = 0;
            if (s.Length > 0 && s[0] == '-')
            {
                neg = true;
                k = 1;
            }
            for (; k < s.Length; k++)
            {
                int d = s[k] - '0';
                Fail.check(d >= 0 && d <= 9, "parseLong: bad digit in " + s);
                v = v * 10L + (long)d;
            }
            return neg ? -v : v;
        }

        public static ulong parseHexU64(string s)
        {
            string t = s.Trim();
            if (t.Length >= 2 && t.Substring(0, 2) == "0x")
            {
                t = t.Substring(2);
            }
            ulong v = 0UL;
            for (int i = 0; i < t.Length; i++)
            {
                int d = Counters.dig(t[i]);
                Fail.check(d >= 0, "parseHexU64: bad digit in " + s);
                // (ushort) first is load-bearing: int -> ulong is a sign-extending
                // conversion regardless of the mask, which is what CS0675 warns about;
                // probing showed (ushort) then (ulong) compiles warning-free.
                v = (v << 4) | (ulong)(ushort)(d & 0xF);
            }
            return v;
        }

        public static string hexU64(ulong v)
        {
            string digits = "0123456789abcdef";
            string res = "";
            for (int k = 15; k >= 0; k--)
            {
                res += "" + digits[(int)((v >> (4 * k)) & 0xFUL)];
            }
            return res;
        }

        // ---- the one simulation loop (A-019 §11: harness and GameLoop share it) --
        public static int runLoop(SimState s, byte[] dg, long interval,
                                  long[] steps, byte[][] blobs, out int snapCount)
        {
            long maxSteps = LevelTable.timerSeconds * 60L;
            int n = 0;
            n = addSnap(steps, blobs, n, 0L, emitSnapshot(s));
            bool completed = false;
            while (s.stepIndex < maxSteps)
            {
                Intent i = InputAdapter.sampleFromDigest(dg, s.stepIndex);
                s.intent = i;
                // A-022: SpawnDirector.apply moved into SimStep.run (the per-step
                // body), so the step-driven loop and the frame driver get spawns
                // exactly once and cannot drift apart.
                FixedStepDriver.step(s, s.stepIndex, i);
                if (s.stepIndex % interval == 0L)
                {
                    n = addSnap(steps, blobs, n, s.stepIndex, emitSnapshot(s));
                }
                if (FrameDriver.goal(s))
                {
                    completed = true;
                    break;
                }
            }
            n = addSnap(steps, blobs, n, -1L, emitSnapshot(s));
            // the snapshot count must reach the caller: an ehreplay1 written with
            // n == 0 would make H-02 pass vacuously over zero comparisons.
            snapCount = n;
            return completed ? 1 : 0;
        }

        public static int addSnap(long[] steps, byte[][] blobs, int n, long step, byte[] blob)
        {
            Fail.check(n < steps.Length, "snapshot list overflow");
            steps[n] = step;
            blobs[n] = blob;
            return n + 1;
        }

        public static byte[] emitSnapshot(SimState s)
        {
            Snapshot.reset();
            Snapshot.emitHeader(s);
            Snapshot.emitAttractors(s);
            Snapshot.emitBodies(s);
            Snapshot.emitHazards(s);
            Snapshot.emitFreeList(s);
            Snapshot.emitCounters(s);
            Snapshot.seal();
            byte[] res = new byte[Snapshot.len];
            for (int i = 0; i < Snapshot.len; i++)
            {
                res[i] = Snapshot.buf[i];
            }
            return res;
        }

        public static ulong readTrailing(byte[] b)
        {
            int at = b.Length - 8;
            ulong v = 0UL;
            for (int k = 0; k < 8; k++)
            {
                v |= (ulong)(b[at + k] & 0xFF) << (8 * k);
            }
            return v;
        }

        // ---- H-01 sim --------------------------------------------------------
        public static int sim(string[] argv, int at)
        {
            if (!parseCommon(argv, at))
            {
                usage();
                return 2;
            }
            seedHex = hexU64(pSeed);
            digest = pDigest;
            SimState s = LevelLoader.load(pLevel, pSeed);
            FixedStepDriver.configureStartup(FixedQ.ONE, 60);
            long[] steps = new long[64];
            byte[][] blobs = new byte[64][];
            int n = 0;
            bool completed = runLoop(s, pDigest, 600L, steps, blobs, out n) == 1;
            ulong finalDigest = readTrailing(blobs[n == 0 ? 0 : n - 1]);
            System.Console.Write("digest=" + hexU64(finalDigest) + "\n");
            System.Console.Write("score=" + s.score + "\n");
            System.Console.Write("steps=" + s.stepIndex + "\n");
            System.Console.Write("droppedSteps=" + s.droppedSteps + "\n");
            System.Console.Write("result=" + (completed ? "goal" : "timeout") + "\n");
            if (pOut != null)
            {
                writeEhreplay(pOut, pSeed, pLevel, pDigest, steps, blobs, n, finalDigest, s.score);
            }
            fileRecord("pass");
            return 0;
        }

        public static void writeEhreplay(string path, ulong seed, string levelId,
                                         byte[] dg, long[] steps, byte[][] blobs, int n,
                                         ulong finalDigest, long score)
        {
            string json = "";
            json += "{ \"format\": \"ehreplay1\", \"seed\": \"0x" + hexU64(seed) + "\", \"levelId\": \"" + levelId + "\",\n";
            json += "  \"inputDigest\": \"" + Counters.b64Encode(dg, dg.Length) + "\",\n";
            json += "  \"snapshots\": [";
            for (int k = 0; k < n; k++)
            {
                if (k > 0)
                {
                    json += ",";
                }
                json += " {\"step\": " + steps[k] + ", \"b64\": \"" + Counters.b64Encode(blobs[k], blobs[k].Length) + "\" }";
            }
            json += " ],\n";
            json += "  \"digest8\": \"" + hexU64(finalDigest).Substring(0, 8) + "\", \"score\": " + score + " }\n";
            System.IO.File.WriteAllText(path, json);
        }

        // ---- H-02 replay -----------------------------------------------------
        public static int replay(string[] argv, int at)
        {
            string path = null;
            long interval = 600L;
            int k = at;
            while (k < argv.Length)
            {
                if (argv[k] == "--assert-steps")
                {
                    Fail.check(k + 1 < argv.Length, "missing value after --assert-steps");
                    interval = parseLong(argv[k + 1]);
                    k += 2;
                }
                else
                {
                    path = argv[k];
                    k += 1;
                }
            }
            Fail.check(path != null, "H-02: replay file required");
            if (path.Length > 0 && path[0] == '@')
            {
                path = path.Substring(1);
            }
            string json = System.IO.File.ReadAllText(path);
            string seedStr = scanQuoted(json, "\"seed\"");
            string levelId = scanQuoted(json, "\"levelId\"");
            string idB64 = scanQuoted(json, "\"inputDigest\"");
            string wantDigest8 = scanQuoted(json, "\"digest8\"");
            long[] wantSteps = new long[64];
            byte[][] wantBytes = new byte[64][];
            int n = scanSnapshots(json, wantSteps, wantBytes);

            seedHex = hexU64(parseHexU64(seedStr));
            digest = Counters.b64Decode(idB64);
            ulong seed = parseHexU64(seedStr);

            SimState s = LevelLoader.load(levelId, seed);
            FixedStepDriver.configureStartup(FixedQ.ONE, 60);
            // A-019 §11: exactly one simulation loop. H-02 re-runs through the
            // SAME runLoop H-01 used; it must not re-implement stepping, or a
            // divergence between the two loops would mask a determinism bug.
            long[] gotSteps = new long[64];
            byte[][] gotBytes = new byte[64][];
            int m = 0;
            bool completed = runLoop(s, digest, interval, gotSteps, gotBytes, out m) == 1;

            // digest8 is verified against the decoded final snapshot before any
            // diff runs, so a corrupt golden fails clearly (A-019 §7).
            if (n >= 1 && wantSteps[n - 1] == -1L)
            {
                string goldenHex = hexU64(readTrailing(wantBytes[n - 1]));
                if (wantDigest8 != goldenHex.Substring(0, 8))
                {
                    System.Console.Write("FAIL digest8: golden corrupt (golden=" + wantDigest8 +
                        " goldenFile=" + goldenHex.Substring(0, 8) + ")\n");
                    fileRecord("fail");
                    return 1;
                }
            }
            if (n != m)
            {
                System.Console.Write("FAIL step-list: golden=" + n + " replay=" + m + "\n");
                fileRecord("fail");
                return 1;
            }
            for (int q = 0; q < n; q++)
            {
                if (wantSteps[q] != gotSteps[q])
                {
                    System.Console.Write("FAIL step-list golden[" + q + "]=" + wantSteps[q] +
                        " replay[" + q + "]=" + gotSteps[q] + "\n");
                    fileRecord("fail");
                    return 1;
                }
                byte[] a = wantBytes[q];
                byte[] b = gotBytes[q];
                if (a.Length != b.Length)
                {
                    System.Console.Write("FAIL step=" + gotSteps[q] + " byteOffset=" +
                        (a.Length < b.Length ? a.Length : b.Length) + " field=length\n");
                    fileRecord("fail");
                    return 1;
                }
                for (int o = 0; o < b.Length; o++)
                {
                    if (a[o] != b[o])
                    {
                        System.Console.Write("FAIL step=" + gotSteps[q] + " byteOffset=" + o +
                            " field=" + Counters.fieldName(b, b.Length, o) + "\n");
                        fileRecord("fail");
                        return 1;
                    }
                }
            }
            System.Console.Write("PASS snapshotsCompared=" + n + "\n");
            fileRecord("pass");
            return 0;
        }

        public static string scanQuoted(string json, string key)
        {
            int at = json.IndexOf(key);
            Fail.check(at >= 0, "ehreplay1: missing key " + key);
            int colon = json.IndexOf(":", at + key.Length);
            Fail.check(colon >= 0, "ehreplay1: missing colon after " + key);
            int q1 = json.IndexOf("\"", colon + 1);
            Fail.check(q1 >= 0, "ehreplay1: missing value quote for " + key);
            int q2 = json.IndexOf("\"", q1 + 1);
            Fail.check(q2 >= 0, "ehreplay1: unterminated value for " + key);
            return json.Substring(q1 + 1, q2 - q1 - 1);
        }

        public static long scanNumber(string json, string key)
        {
            int at = json.IndexOf(key);
            Fail.check(at >= 0, "ehreplay1: missing key " + key);
            int colon = json.IndexOf(":", at + key.Length);
            Fail.check(colon >= 0, "ehreplay1: missing colon after " + key);
            int k = colon + 1;
            while (k < json.Length && (json[k] == ' ' || json[k] == '\t' || json[k] == '\n'))
            {
                k++;
            }
            bool neg = false;
            if (k < json.Length && json[k] == '-')
            {
                neg = true;
                k++;
            }
            long v = 0L;
            bool any = false;
            while (k < json.Length && json[k] >= '0' && json[k] <= '9')
            {
                v = v * 10L + (json[k] - '0');
                any = true;
                k++;
            }
            Fail.check(any, "ehreplay1: no digits for " + key);
            return neg ? -v : v;
        }

        public static int scanSnapshots(string json, long[] steps, byte[][] blobs)
        {
            int at = json.IndexOf("\"snapshots\"");
            Fail.check(at >= 0, "ehreplay1: missing snapshots");
            int lb = json.IndexOf("[", at);
            Fail.check(lb >= 0, "ehreplay1: snapshots array missing");
            int rb = json.IndexOf("]", lb);
            Fail.check(rb > lb, "ehreplay1: snapshots array unterminated");
            int n = 0;
            int k = lb;
            while (k < rb)
            {
                int ob = json.IndexOf("{", k);
                if (ob < 0 || ob > rb)
                {
                    break;
                }
                int cb = json.IndexOf("}", ob);
                Fail.check(cb > ob && cb <= rb, "ehreplay1: snapshot object unterminated");
                string obj = json.Substring(ob, cb + 1 - ob);
                steps[n] = scanNumber(obj, "\"step\"");
                blobs[n] = Counters.b64Decode(scanQuoted(obj, "\"b64\""));
                n++;
                k = cb + 1;
            }
            return n;
        }

        // ---- H-03 snap -------------------------------------------------------
        public static int snap(string[] argv, int at)
        {
            if (!parseCommon(argv, at))
            {
                usage();
                return 2;
            }
            Fail.check(pStep >= 0L, "H-03: --step required and must be >= 0");
            seedHex = hexU64(pSeed);
            digest = pDigest;
            SimState s = LevelLoader.load(pLevel, pSeed);
            FixedStepDriver.configureStartup(FixedQ.ONE, 60);
            long target = LevelTable.targetMass;
            while (s.stepIndex < pStep)
            {
                Intent i = InputAdapter.sampleFromDigest(pDigest, s.stepIndex);
                s.intent = i;
                SpawnDirector.apply(s);
                FixedStepDriver.step(s, s.stepIndex, i);
                if ((s.holeMass >> 32) >= target)
                {
                    break;
                }
            }
            byte[] blob = emitSnapshot(s);
            System.Console.Write("snap " + Counters.hexEncode(blob, 0, blob.Length) + "\n");
            fileRecord("pass");
            return 0;
        }

        // ---- H-04 diff -------------------------------------------------------
        public static int diff(string[] argv, int at)
        {
            if (at + 1 >= argv.Length)
            {
                usage();
                return 2;
            }
            byte[] a = readBytes(argv[at]);
            byte[] b = readBytes(argv[at + 1]);
            int m = a.Length < b.Length ? a.Length : b.Length;
            for (int o = 0; o < m; o++)
            {
                if (a[o] != b[o])
                {
                    System.Console.Write("DIFF offset=" + o + " field=" +
                        Counters.fieldName(b, b.Length, o) + "\n");
                    fileRecord("pass");
                    return 0;
                }
            }
            if (a.Length != b.Length)
            {
                System.Console.Write("DIFF offset=" + m + " field=length\n");
                fileRecord("pass");
                return 0;
            }
            System.Console.Write("IDENTICAL\n");
            fileRecord("pass");
            return 0;
        }

        // ---- H-05 counters ---------------------------------------------------
        public static int counters(string[] argv, int at)
        {
            if (!parseCommon(argv, at))
            {
                usage();
                return 2;
            }
            seedHex = hexU64(pSeed);
            digest = pDigest;
            SimState s = LevelLoader.load(pLevel, pSeed);
            FixedStepDriver.configureStartup(FixedQ.ONE, 60);
            long target = LevelTable.targetMass;
            long maxSteps = LevelTable.timerSeconds * 60L;
            long[] batchMs = new long[64];
            int batches = 0;
            const long BATCH = 100L;
            while (s.stepIndex < maxSteps)
            {
                long t0 = System.DateTime.UtcNow.Ticks;
                long end = s.stepIndex + BATCH;
                if (end > maxSteps)
                {
                    end = maxSteps;
                }
                while (s.stepIndex < end)
                {
                    Intent i = InputAdapter.sampleFromDigest(pDigest, s.stepIndex);
                    s.intent = i;
                    SpawnDirector.apply(s);
                    FixedStepDriver.step(s, s.stepIndex, i);
                    if ((s.holeMass >> 32) >= target)
                    {
                        break;
                    }
                }
                if (batches < batchMs.Length)
                {
                    batchMs[batches++] = System.DateTime.UtcNow.Ticks - t0;
                }
                if ((s.holeMass >> 32) >= target)
                {
                    break;
                }
            }
            long[] edges = { 1L, 10L, 100L, 1000L };
            string[] labels = { "<1ms", "<10ms", "<100ms", "<1000ms", ">=1000ms" };
            System.Console.Write("histogram(ms per 100 steps): " +
                Counters.histogram(batchMs, batches, edges, labels) + "\n");
            System.Console.Write("highmarks small=" + s.highmarkSmall + " medium=" + s.highmarkMedium +
                " large=" + s.highmarkLarge + " hazard=" + s.highmarkHazard + "\n");
            string rss = "null";
            if (System.IO.File.Exists("/proc/self/status"))
            {
                string status = System.IO.File.ReadAllText("/proc/self/status");
                int at2 = status.IndexOf("VmRSS:");
                if (at2 >= 0)
                {
                    int k2 = at2 + 6;
                    while (k2 < status.Length && (status[k2] == ' ' || status[k2] == '\t'))
                    {
                        k2++;
                    }
                    long v = 0L;
                    bool any = false;
                    while (k2 < status.Length && status[k2] >= '0' && status[k2] <= '9')
                    {
                        v = v * 10L + (status[k2] - '0');
                        any = true;
                        k2++;
                    }
                    if (any)
                    {
                        rss = "" + v;
                    }
                }
            }
            System.Console.Write("rssKB=" + rss + " (desktop-only; null on mobile targets)\n");
            System.Console.Write("perFrameAlloc=not-instrumented (phase 1 has no allocation counter hook; " +
                "the sim loop itself allocates nothing, decode returns one Intent per step)\n");
            fileRecord("pass");
            return 0;
        }

        // ---- H-06..H-09: honest unavailability (exit 2, never a silent pass) --
        public static int notAvailable(string cmd)
        {
            System.Console.Write(cmd + ": not available in phase 1 (frame driver and render state " +
                "are phase 2; A-019 §10)\n");
            fileRecord("not-available");
            return 2;
        }

        // ---- manifest filing (A-019 §11) -------------------------------------
        public static void fileRecord(string result)
        {
            writeRecord(result, false);
        }

        /// <summary>
        /// A-022: A-019 §9 says the weak-device test P-10 keeps a hardware-only
        /// component and "its record carries `hardware-only: true`, visibly split
        /// rather than silently skipped". The flag is a parameter, not a mutable
        /// global, so a caller cannot forget to reset it and leak the flag into
        /// the next command's record.
        /// </summary>
        public static void writeRecord(string result, bool hardwareOnly)
        {
            string d8 = hexU64(InputDigest.fnv1a(digest, digest.Length)).Substring(0, 8);
            string runId = testId + "-" + seedHex + "-" + d8;
            string dir = "artifacts/harness/" + runId;
            System.IO.Directory.CreateDirectory("artifacts/harness");
            System.IO.Directory.CreateDirectory(dir);
            string rec = "{ \"testId\": \"" + testId + "\", \"command\": \"" + commandName +
                "\", \"seed\": \"0x" + seedHex + "\", \"digest\": \"" +
                Counters.hexEncode(digest, 0, digest.Length) +
                "\", \"result\": \"" + result + "\", \"owner\": \"programmer\", " +
                "\"hardwareOnly\": " + (hardwareOnly ? "true" : "false") + " }\n";
            System.IO.File.WriteAllText(dir + "/" + testId + ".json", rec);
            string man = "{ \"format\": \"ehmanifest1\", \"runId\": \"" + runId +
                "\", \"records\": [\"" + testId + ".json\"] }\n";
            System.IO.File.WriteAllText(dir + "/manifest.json", man);
        }

        public static void usage()
        {
            System.Console.Write("usage: harness sim --seed <hex> --digest @file --level <id> [--out <path>]\n" +
                "       harness replay @replays/<file>.json [--assert-steps 600]\n" +
                "       harness snap --seed <hex> --digest @file --level <id> --step <N>\n" +
                "       harness diff <blobA> <blobB>\n" +
                "       harness counters --seed <hex> --digest @file --level <id>\n" +
                "       harness hitch --at <f,f,..> --ms <n> [--seed <hex> --digest @file --level <id>]\n" +
                "       harness fps --target <60|30> [--time-scale <d.d>] [--seed <hex> --digest @file --level <id>]\n" +
                "       harness pointer --script @file [--seed <hex> --level <id>]\n" +
                "       harness tier --probe [--seed <hex> --digest @file --level <id>]\n");
        }
    }
}

// A-022 Phase 2 frame driver and render state — tools/harness/FrameCommands.cs
// H-06..H-09 of A-019 §9, made real against the frame driver (FrameDriver.cs)
// instead of reporting exit 2.
//
// The assertion each command makes is the one A-019 §10 states, and each is
// byte-level rather than a count that can pass vacuously:
//
//  H-06 hitch  — a hitch adds WALL-CLOCK debt to one frame of the frame driver.
//                The assertion is that the simulation is untouched: the goal
//                step index and every observable snapshot are byte-identical,
//                while frame accounting (framesToGoal, droppedSteps) differs.
//                A-019 §9 words this as "changes droppedSteps and nothing else";
//                a hitch smaller than the catch-up budget does NOT change
//                droppedSteps (A-019 §10 says droppedSteps "only grows through
//                the MAX_CATCHUP = 4 rule"), so the honest assertion is stated
//                and printed here rather than assumed. Both a within-budget and
//                an over-budget schedule run, so the drop path is proven live.
//  H-07 fps    — the 30 fps step budget on a strong machine: same accumulator,
//                two steps per frame, byte-identical trajectory, zero dropped
//                steps. Its record carries hardwareOnly: true (A-019 §9: P-10
//                keeps a hardware-only component, "visibly split rather than
//                silently skipped").
//  H-08 pointer— synthetic telemetry through InputAdapter (transport only).
//                Pointer POSITION -> intent resolution is A-011's scheme and is
//                NOT implemented here; the script supplies already-resolved
//                intents, which is InputAdapter's documented contract.
//  H-09 tier   — quality tiers change render state only; the probe asserts
//                SimState byte-identical across tiers for the same (seed,
//                inputDigest). The device-derived tier is NOT invented: this
//                environment has no device, so the probe says so.
//
// Exit codes stay A-019 §9's: 0 pass, 1 fail, 2 harness error.

namespace EH
{
    public static class FrameCommands
    {
        // ---- shared phase-2 identity (Commands files the manifest) ------------
        public static ulong pSeed;
        public static byte[] pDigest;
        public static string pLevel;

        /// <summary>
        /// A-022 H-06: the byte comparison the hitch assertion needs.
        ///
        /// A hitch that exceeds the catch-up budget legitimately changes ONE
        /// state observable: counters.droppedSteps, which is what the MAX_CATCHUP
        /// rule exists to count (A-019 §10: "droppedSteps only grows through the
        /// MAX_CATCHUP = 4 rule"). The snapshot's trailing 8 bytes are an FNV-1a
        /// SEAL over the snapshot, so they move with that field by construction.
        /// Both are therefore excluded, and nothing else is: the state itself is
        /// compared byte-for-byte, which is strictly stronger than the seal the
        /// golden carries.
        ///
        /// The exclusion window is found by ASKING Counters.fieldName what each
        /// trailing byte is, never by recomputing the snapshot's offsets here.
        /// Duplicated layout arithmetic is the drift that would let a real
        /// difference hide inside a wrong window, so the window is also verified
        /// to be exactly the two adjacent fields.
        /// </summary>
        public static bool cmpSnapState(byte[] a, byte[] b, long step)
        {
            if (a.Length != b.Length)
            {
                System.Console.Write("FAIL step=" + step + " byteOffset=" +
                    (a.Length < b.Length ? a.Length : b.Length) + " field=length\n");
                return false;
            }
            int from = b.Length - 128;
            if (from < 0)
            {
                from = 0;
            }
            int lo = b.Length;
            int hi = -1;
            int covered = 0;
            for (int o = from; o < b.Length; o++)
            {
                string nm = Counters.fieldName(b, b.Length, o);
                if (nm == "counters.droppedSteps" || nm == "digest")
                {
                    if (o < lo)
                    {
                        lo = o;
                    }
                    if (o > hi)
                    {
                        hi = o;
                    }
                    covered += 1;
                }
            }
            // the two fields are adjacent and 8 bytes each; anything else means
            // the layout moved and the window cannot be trusted.
            if (lo > hi || covered != 16 || hi - lo + 1 != 16)
            {
                System.Console.Write("FAIL step=" + step +
                    " field=droppedStepsWindow (snapshot layout drifted: the exclusion window is not the two adjacent fields)\n");
                return false;
            }
            for (int o = 0; o < b.Length; o++)
            {
                if (o >= lo && o <= hi)
                {
                    continue;
                }
                if (a[o] != b[o])
                {
                    System.Console.Write("FAIL step=" + step + " byteOffset=" + o +
                        " field=" + Counters.fieldName(b, b.Length, o) + "\n");
                    return false;
                }
            }
            return true;
        }

        // ---- byte comparison with a named field, mirroring H-02 ---------------
        public static bool cmpSnap(byte[] a, byte[] b, long step)
        {
            if (a.Length != b.Length)
            {
                System.Console.Write("FAIL step=" + step + " byteOffset=" +
                    (a.Length < b.Length ? a.Length : b.Length) + " field=length\n");
                return false;
            }
            for (int o = 0; o < b.Length; o++)
            {
                if (a[o] != b[o])
                {
                    System.Console.Write("FAIL step=" + step + " byteOffset=" + o +
                        " field=" + Counters.fieldName(b, b.Length, o) + "\n");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Default run identity: the blessed golden run. H-06/H-07/H-09 compare
        /// a frame-driven run against a reference, and a reference of their own
        /// invention would let both sides drift together; pinning the golden
        /// digest means the comparison is against the committed artifact.
        /// </summary>
        public static void defaultIdentity()
        {
            if (pLevel == null)
            {
                pLevel = LevelTable.levelId;
            }
            if (pSeed == 0UL)
            {
                pSeed = LevelTable.defaultSeed;
            }
            if (pDigest == null)
            {
                pDigest = System.IO.File.ReadAllBytes("replays/p1-level-01.digest.bin");
            }
        }

        // ======================= H-06 hitch ====================================
        /// <summary>
        /// Run one frame-driven run with a hitch schedule and compare EVERY
        /// observable snapshot against an unhitched frame-driven run driven in
        /// lockstep. Returns 0 when no mismatch, 1 when one was found.
        ///
        /// Lockstep (whichever run is behind advances) rather than two separate
        /// runs plus stored snapshots: the golden's snapshots are ~23 KB each,
        /// so a store-then-compare strategy would hold ~65 MB per schedule.
        ///
        /// The comparison excludes exactly two adjacent trailing fields,
        /// counters.droppedSteps and the snapshot's FNV seal over itself, because
        /// a hitch over the catch-up budget legitimately grows the first and the
        /// second is derived from it. Everything else is compared byte-for-byte,
        /// which is stronger than the seal. The two directional invariants on
        /// droppedSteps are asserted separately: a hitch may never drop FEWER
        /// steps, and it may drop MORE only on a frame that exhausted MAX_CATCHUP.
        /// </summary>
        public static int hitchRun(byte[] dg, ulong seed, string level, long[] frames,
                                   long[] ms, int n,
                                   out long framesToGoal, out long dropped,
                                   out long compared, out long maxFrameSteps)
        {
            SimState h = LevelLoader.load(level, seed);
            SimState f = LevelLoader.load(level, seed);
            FrameDriver.FrameClock hc = FrameDriver.boot(60, FixedQ.ONE, n);
            FrameDriver.FrameClock fc = FrameDriver.boot(60, FixedQ.ONE, 0);
            FrameDriver.schedule(hc, frames, ms, n);

            compared = 0;
            long budget = FrameDriver.frameBudget(60);
            long prevStep = -1L;

            while (true)
            {
                FrameDriver.frame(hc, h, dg);
                while (f.stepIndex < h.stepIndex)
                {
                    FrameDriver.frame(fc, f, dg);
                }
                if (f.stepIndex != h.stepIndex)
                {
                    System.Console.Write("FAIL step=" + h.stepIndex +
                        " field=frameClock.stepCount (the two clocks disagree on step count)\n");
                    framesToGoal = hc.framesToGoal;
                    dropped = h.droppedSteps;
                    compared = 0;
                    maxFrameSteps = hc.maxFrameSteps;
                    return 1;
                }
                if (!cmpSnapState(Commands.emitSnapshot(h), Commands.emitSnapshot(f), h.stepIndex))
                {
                    framesToGoal = hc.framesToGoal;
                    dropped = h.droppedSteps;
                    maxFrameSteps = hc.maxFrameSteps;
                    return 1;
                }
                compared += 1;

                if (FrameDriver.goal(h))
                {
                    break;
                }
                // a frame that delivers no step would loop forever; the frame
                // clock guarantees >= 1 step at 60 fps, so this is a hard stop,
                // not a silent continue.
                if (h.stepIndex == prevStep || hc.frameIndex > budget)
                {
                    System.Console.Write("FAIL step=" + h.stepIndex +
                        " field=frameClock.noProgress (a frame delivered no step)\n");
                    framesToGoal = hc.framesToGoal;
                    dropped = h.droppedSteps;
                    maxFrameSteps = hc.maxFrameSteps;
                    return 1;
                }
                prevStep = h.stepIndex;
            }

            // the run must have finished on the goal, not on the frame budget:
            // a hitch that made the level time out is a changed simulation.
            if (hc.framesToGoal < 0L || fc.framesToGoal < 0L)
            {
                System.Console.Write("FAIL a run did not finish on the goal (framesHitched=" +
                    hc.framesToGoal + " framesUnhitched=" + fc.framesToGoal + ")\n");
                framesToGoal = hc.framesToGoal;
                dropped = h.droppedSteps;
                maxFrameSteps = hc.maxFrameSteps;
                return 1;
            }
            // droppedSteps is the ONLY observable a hitch may change, and the
            // rule that may change it is the catch-up budget. So the invariant
            // is directional and conditional, not a predicted value: a hitch
            // may never drop FEWER steps, and it may drop MORE only on a frame
            // that actually exhausted MAX_CATCHUP.
            if (h.droppedSteps < f.droppedSteps)
            {
                System.Console.Write("FAIL the hitched run dropped fewer steps than the unhitched one\n");
                framesToGoal = hc.framesToGoal;
                dropped = h.droppedSteps;
                maxFrameSteps = hc.maxFrameSteps;
                return 1;
            }
            if (h.droppedSteps > f.droppedSteps && hc.maxFrameSteps < FixedStepDriver.MAX_CATCHUP)
            {
                System.Console.Write("FAIL droppedSteps grew without a frame exhausting MAX_CATCHUP=" +
                    FixedStepDriver.MAX_CATCHUP + " (maxStepsPerFrame=" + hc.maxFrameSteps + ")\n");
                framesToGoal = hc.framesToGoal;
                dropped = h.droppedSteps;
                maxFrameSteps = hc.maxFrameSteps;
                return 1;
            }

            framesToGoal = hc.framesToGoal;
            dropped = h.droppedSteps;
            maxFrameSteps = hc.maxFrameSteps;
            return 0;
        }

        public static int hitch(string[] argv, int at)
        {
            long[] frames = new long[64];
            long[] msArr = new long[64];
            int n = 0;
            long ms = 42L;

            int k = at;
            while (k < argv.Length)
            {
                if (argv[k] == "--at")
                {
                    Fail.check(k + 1 < argv.Length, "missing value after --at");
                    n = parseList(argv[k + 1], frames);
                    k += 2;
                }
                else if (argv[k] == "--ms")
                {
                    Fail.check(k + 1 < argv.Length, "missing value after --ms");
                    ms = Commands.parseLong(argv[k + 1]);
                    k += 2;
                }
                else if (argv[k] == "--seed")
                {
                    Fail.check(k + 1 < argv.Length, "missing value after --seed");
                    pSeed = Commands.parseHexU64(argv[k + 1]);
                    k += 2;
                }
                else if (argv[k] == "--digest")
                {
                    Fail.check(k + 1 < argv.Length, "missing value after --digest");
                    pDigest = Commands.readBytes(argv[k + 1]);
                    k += 2;
                }
                else if (argv[k] == "--level")
                {
                    Fail.check(k + 1 < argv.Length, "missing value after --level");
                    pLevel = argv[k + 1];
                    k += 2;
                }
                else
                {
                    usage2();
                    return 2;
                }
            }
            defaultIdentity();
            Commands.seedHex = Commands.hexU64(pSeed);
            Commands.digest = pDigest;
            // the schedule is one duration for every hitched frame (A-019 §9's
            // `--at 120,341,902 --ms 42` form); a zero entry would be a silent
            // no-hitch, so the durations are filled before anything runs.
            for (int q = 0; q < n; q++)
            {
                msArr[q] = ms;
            }

            // ---- reference 1: the committed golden path (step-driven) --------
            SimState g = LevelLoader.load(pLevel, pSeed);
            FixedStepDriver.configureStartup(FixedQ.ONE, 60);
            long[] gSteps = new long[64];
            byte[][] gBlobs = new byte[64][];
            int gn = 0;
            bool gDone = Commands.runLoop(g, pDigest, 600L, gSteps, gBlobs, out gn) == 1;
            long goalStep = g.stepIndex;

            // ---- reference 2: frame-driven, no hitch, must equal reference 1 --
            SimState f = LevelLoader.load(pLevel, pSeed);
            FrameDriver.FrameClock fc = FrameDriver.boot(60, FixedQ.ONE, 0);
            long cp = 0;
            while (true)
            {
                FrameDriver.frame(fc, f, pDigest);
                for (int q = 0; q < gn; q++)
                {
                    if (gSteps[q] == f.stepIndex)
                    {
                        if (!cmpSnap(Commands.emitSnapshot(f), gBlobs[q], f.stepIndex))
                        {
                            Commands.fileRecord("fail");
                            return 1;
                        }
                        cp += 1;
                    }
                }
                if (FrameDriver.goal(f))
                {
                    break;
                }
            }
            // runLoop files the END state with a -1 step sentinel (A-019 §11: the
            // final snapshot is part of the golden), so it is compared once the
            // frame-driven run has finished rather than at a step index.
            for (int q = 0; q < gn; q++)
            {
                if (gSteps[q] == -1L)
                {
                    if (!cmpSnap(Commands.emitSnapshot(f), gBlobs[q], goalStep))
                    {
                        Commands.fileRecord("fail");
                        return 1;
                    }
                    cp += 1;
                }
            }
            long plainFrames = fc.framesToGoal;

            // ---- the schedule as asked --------------------------------------
            long ft1 = 0;
            long dr1 = 0;
            long cp1 = 0;
            long mx1 = 0;
            int r1 = hitchRun(pDigest, pSeed, pLevel, frames, msArr, n,
                              out ft1, out dr1, out cp1, out mx1);

            // ---- the same schedule, forced over the catch-up budget ----------
            // MAX_CATCHUP * DT is 4/60 s = 66.67 ms; 100 ms of stall on a 60 fps
            // frame is 7 DTs of debt, so the dropped-step rule must fire. Running
            // it proves that path is live code, not a branch that never triggers.
            long[] msOver = new long[64];
            for (int q = 0; q < n; q++)
            {
                msOver[q] = 100L;
            }
            long ft2 = 0;
            long dr2 = 0;
            long cp2 = 0;
            long mx2 = 0;
            int r2 = hitchRun(pDigest, pSeed, pLevel, frames, msOver, n,
                              out ft2, out dr2, out cp2, out mx2);

            System.Console.Write("seed=0x" + Commands.seedHex + "\n");
            System.Console.Write("hitchFrames=" + n + " ms=" + ms + "\n");
            System.Console.Write("goalStep=" + goalStep + "\n");
            System.Console.Write("framesUnhitched=" + plainFrames + "\n");
            System.Console.Write("framesHitched=" + ft1 + " droppedStepsHitched=" + dr1 +
                " maxStepsPerFrame=" + mx1 + "\n");
            System.Console.Write("framesOverBudget=" + ft2 + " droppedStepsOverBudget=" + dr2 +
                " maxStepsPerFrame=" + mx2 + "\n");
            System.Console.Write("snapshotsCompared=" + (cp + cp1 + cp2) + "\n");

            if (r1 != 0 || r2 != 0)
            {
                Commands.fileRecord("fail");
                return 1;
            }
            // the hitch must be OBSERVABLE in frame accounting, otherwise the
            // command proved nothing and must not report a pass.
            if (ft1 == plainFrames)
            {
                System.Console.Write("FAIL hitch had no observable effect on frame accounting\n");
                Commands.fileRecord("fail");
                return 1;
            }
            if (dr2 <= 0L)
            {
                System.Console.Write("FAIL the over-budget hitch did not exercise the dropped-step rule\n");
                Commands.fileRecord("fail");
                return 1;
            }
            System.Console.Write("PASS result=" + (gDone ? "goal" : "timeout") + " exit=0\n");
            Commands.fileRecord("pass");
            return 0;
        }

        // ---- comma-separated positive integer list (the --at schedule) -------
        public static int parseList(string s, long[] outArr)
        {
            int n = 0;
            int i = 0;
            while (i < s.Length)
            {
                long v = 0L;
                bool any = false;
                while (i < s.Length && s[i] != ',')
                {
                    int d = s[i] - '0';
                    Fail.check(d >= 0 && d <= 9, "parseList: bad digit in " + s);
                    v = v * 10L + (long)d;
                    any = true;
                    i += 1;
                }
                Fail.check(any, "parseList: empty entry in " + s);
                Fail.check(n < outArr.Length, "parseList: too many hitch frames");
                outArr[n] = v;
                n += 1;
                if (i < s.Length && s[i] == ',')
                {
                    i += 1;
                    Fail.check(i < s.Length, "parseList: trailing comma in " + s);
                }
            }
            Fail.check(n > 0, "parseList: --at needs at least one frame index");
            return n;
        }

        // ======================= H-07 fps ======================================
        public static long parseFixedDecimal(string s)
        {
            // "2.0" / "1.0" -> Q32.32 raw. No float: integer part times ONE plus
            // the fraction scaled by MulDiv.
            string t = s.Trim();
            int dot = t.IndexOf(".");
            string ip = dot < 0 ? t : t.Substring(0, dot);
            string fp = dot < 0 ? "" : t.Substring(dot + 1);
            bool neg = ip.Length > 0 && ip[0] == '-';
            if (neg)
            {
                ip = ip.Substring(1);
            }
            Fail.check(ip.Length > 0, "parseFixedDecimal: missing integer part in " + s);
            long whole = 0L;
            for (int i = 0; i < ip.Length; i++)
            {
                int d = ip[i] - '0';
                Fail.check(d >= 0 && d <= 9, "parseFixedDecimal: bad digit in " + s);
                whole = whole * 10L + (long)d;
            }
            long raw = whole * FixedQ.ONE;
            if (fp.Length > 0)
            {
                long num = 0L;
                long den = 1L;
                for (int i = 0; i < fp.Length; i++)
                {
                    int d = fp[i] - '0';
                    Fail.check(d >= 0 && d <= 9, "parseFixedDecimal: bad digit in " + s);
                    num = num * 10L + (long)d;
                    den = den * 10L;
                }
                raw = raw + MulDiv.muldiv(num, FixedQ.ONE, den);
            }
            return neg ? -raw : raw;
        }

        public static int fps(string[] argv, int at)
        {
            int target = 30;
            long tsRaw = FixedQ.ONE;

            int k = at;
            while (k < argv.Length)
            {
                if (argv[k] == "--target")
                {
                    Fail.check(k + 1 < argv.Length, "missing value after --target");
                    target = (int)Commands.parseLong(argv[k + 1]);
                    k += 2;
                }
                else if (argv[k] == "--time-scale")
                {
                    Fail.check(k + 1 < argv.Length, "missing value after --time-scale");
                    tsRaw = parseFixedDecimal(argv[k + 1]);
                    k += 2;
                }
                else if (argv[k] == "--seed")
                {
                    Fail.check(k + 1 < argv.Length, "missing value after --seed");
                    pSeed = Commands.parseHexU64(argv[k + 1]);
                    k += 2;
                }
                else if (argv[k] == "--digest")
                {
                    Fail.check(k + 1 < argv.Length, "missing value after --digest");
                    pDigest = Commands.readBytes(argv[k + 1]);
                    k += 2;
                }
                else if (argv[k] == "--level")
                {
                    Fail.check(k + 1 < argv.Length, "missing value after --level");
                    pLevel = argv[k + 1];
                    k += 2;
                }
                else
                {
                    usage2();
                    return 2;
                }
            }
            defaultIdentity();
            Commands.seedHex = Commands.hexU64(pSeed);
            Commands.digest = pDigest;

            // A-019 §10 keeps the knob on PlatformBridge; setting it re-runs the
            // startup gate, so an unreachable targetFps or timeScale is a hard
            // rejection here (SystemicFailure -> exit 2), never a silent pass.
            PlatformBridge.setDevKnobs(target, tsRaw);
            System.Console.Write("platformBridgeTargetFps=" + PlatformBridge.devTargetFps + "\n");
            System.Console.Write("timeScaleRaw=" + tsRaw + "\n");

            // ---- reference: the committed golden path ------------------------
            SimState g = LevelLoader.load(pLevel, pSeed);
            FixedStepDriver.configureStartup(FixedQ.ONE, 60);
            long[] gSteps = new long[64];
            byte[][] gBlobs = new byte[64][];
            int gn = 0;
            bool gDone = Commands.runLoop(g, pDigest, 600L, gSteps, gBlobs, out gn) == 1;
            long goalStep = g.stepIndex;

            // ---- the weak-device budget, on this strong machine --------------
            SimState f = LevelLoader.load(pLevel, pSeed);
            FrameDriver.FrameClock fc = FrameDriver.boot(target, tsRaw, 0);
            long cp = 0;
            while (true)
            {
                FrameDriver.frame(fc, f, pDigest);
                for (int q = 0; q < gn; q++)
                {
                    if (gSteps[q] == f.stepIndex)
                    {
                        if (!cmpSnap(Commands.emitSnapshot(f), gBlobs[q], f.stepIndex))
                        {
                            Commands.writeRecord("fail", true);
                            return 1;
                        }
                        cp += 1;
                    }
                }
                if (FrameDriver.goal(f))
                {
                    break;
                }
            }

            // the END state carries the -1 sentinel in the golden, so it is
            // compared once the target-fps run has finished (see H-06).
            for (int q = 0; q < gn; q++)
            {
                if (gSteps[q] == -1L)
                {
                    if (!cmpSnap(Commands.emitSnapshot(f), gBlobs[q], goalStep))
                    {
                        Commands.writeRecord("fail", true);
                        return 1;
                    }
                    cp += 1;
                }
            }

            System.Console.Write("targetFps=" + target + "\n");
            System.Console.Write("goalStep=" + goalStep + " goalStepAtTarget=" + f.stepIndex + "\n");
            System.Console.Write("stepsPerFrame=" + fc.maxFrameSteps + " (expected " +
                (target == 30 ? "2" : "1") + ")\n");
            System.Console.Write("framesToGoal=" + fc.framesToGoal + "\n");
            System.Console.Write("droppedSteps=" + f.droppedSteps + "\n");
            System.Console.Write("snapshotsCompared=" + cp + "\n");

            if (f.stepIndex != goalStep)
            {
                System.Console.Write("FAIL the 30 fps budget changed where the level ends\n");
                Commands.writeRecord("fail", true);
                return 1;
            }
            if (cp == 0L)
            {
                System.Console.Write("FAIL no checkpoint was reachable at this target (vacuous)\n");
                Commands.writeRecord("fail", true);
                return 1;
            }
            if (f.droppedSteps != 0L)
            {
                System.Console.Write("FAIL a steady 30 fps target dropped steps (the budget is " +
                    "2 DT, inside MAX_CATCHUP, so none may drop)\n");
                Commands.writeRecord("fail", true);
                return 1;
            }
            // A-019 §9: P-10 keeps a hardware-only component and its record
            // carries hardware-only: true, visibly split rather than skipped.
            Commands.writeRecord("pass", true);
            System.Console.Write("PASS result=" + (gDone ? "goal" : "timeout") +
                " hardwareOnly=true exit=0\n");
            return 0;
        }

        // ======================= H-08 pointer ==================================
        // Script grammar (ehpointer1), one directive per line, steps ascending:
        //   <step> <thrustX> <thrustY> <rate>   contact (Q32.32 raw decimal)
        //   <step> dup  <thrustX> <thrustY> <rate>  a SECOND contact same step
        //   <step> pause
        // Values are already-resolved intents: InputAdapter is transport only and
        // A-011 owns pointer-position -> intent resolution, which is NOT
        // implemented here.
        public static int pointer(string[] argv, int at)
        {
            string path = null;
            int k = at;
            while (k < argv.Length)
            {
                if (argv[k] == "--script")
                {
                    Fail.check(k + 1 < argv.Length, "missing value after --script");
                    path = argv[k + 1];
                    // the harness's @file convention (A-019 §9): readBytes strips
                    // it, and this reader must agree or the same path means two
                    // different things in two commands.
                    if (path.Length > 0 && path[0] == '@')
                    {
                        path = path.Substring(1);
                    }
                    Fail.check(System.IO.File.Exists(path),
                        "ehpointer1: script file not found: " + path);
                    k += 2;
                }
                else if (argv[k] == "--seed")
                {
                    Fail.check(k + 1 < argv.Length, "missing value after --seed");
                    pSeed = Commands.parseHexU64(argv[k + 1]);
                    k += 2;
                }
                else if (argv[k] == "--level")
                {
                    Fail.check(k + 1 < argv.Length, "missing value after --level");
                    pLevel = argv[k + 1];
                    k += 2;
                }
                else
                {
                    usage2();
                    return 2;
                }
            }
            if (path == null)
            {
                usage2();
                return 2;
            }
            if (pLevel == null)
            {
                pLevel = LevelTable.levelId;
            }
            if (pSeed == 0UL)
            {
                pSeed = LevelTable.defaultSeed;
            }

            // ---- parse the script -------------------------------------------
            string text = System.IO.File.ReadAllText(path);
            int cap = 4096;
            long[] eStep = new long[cap];
            long[] eKind = new long[cap]; // 0 contact, 1 dup, 2 pause
            long[] eX = new long[cap];
            long[] eY = new long[cap];
            long[] eR = new long[cap];
            int evCount = 0;
            long prevStep = -1L;

            int line = 0;
            int pos = 0;
            while (pos <= text.Length)
            {
                int end = pos;
                while (end < text.Length && text[end] != '\n')
                {
                    end += 1;
                }
                string ln = text.Substring(pos, end - pos);
                pos = end + 1;
                line += 1;
                if (ln.Trim().Length == 0 || (ln.Length > 0 && ln.Trim()[0] == '#'))
                {
                    continue;
                }
                string[] tok = splitTokens(ln);
                Fail.check(tok.Length >= 2, "ehpointer1: line " + line + " too short");
                long st = Commands.parseLong(tok[0]);
                Fail.check(st >= prevStep, "ehpointer1: script steps must be non-decreasing (line " + line + ")");
                prevStep = st;
                if (tok[1] == "pause")
                {
                    eStep[evCount] = st; eKind[evCount] = 2; evCount += 1;
                }
                else
                {
                    Fail.check(tok.Length >= 5, "ehpointer1: line " + line + " needs x y rate");
                    long kind = tok[1] == "dup" ? 1L : 0L;
                    Fail.check(tok[1] == "dup" || tok[1] == "contact",
                        "ehpointer1: unknown directive on line " + line + ": " + tok[1]);
                    eStep[evCount] = st;
                    eKind[evCount] = kind;
                    eX[evCount] = Commands.parseLong(tok[2]);
                    eY[evCount] = Commands.parseLong(tok[3]);
                    eR[evCount] = Commands.parseLong(tok[4]);
                    evCount += 1;
                }
            }
            Fail.check(evCount > 0, "ehpointer1: empty script");

            // ---- drive the adapter exactly as the frame driver would ---------
            InputAdapter.boot(1024);
            long maxSteps = LevelTable.timerSeconds * 60L;
            int evAt = 0;
            long scripted = 0;
            long dupSteps = 0;
            long pauseSteps = 0;
            bool ok = true;

            for (long st = 0; st < maxSteps; st++)
            {
                int pushed = 0;
                long firstX = 0;
                long firstY = 0;
                long firstR = 0;
                while (evAt < evCount && eStep[evAt] == st)
                {
                    if (eKind[evAt] == 2L)
                    {
                        InputAdapter.pause();
                        pauseSteps += 1;
                    }
                    else
                    {
                        if (pushed == 0)
                        {
                            firstX = eX[evAt];
                            firstY = eY[evAt];
                            firstR = eR[evAt];
                        }
                        InputAdapter.pushIntent(eX[evAt], eY[evAt], eR[evAt]);
                        if (eKind[evAt] == 1L)
                        {
                            dupSteps += 1;
                        }
                        pushed += 1;
                    }
                    evAt += 1;
                }
                if (pushed == 0)
                {
                    InputAdapter.pause(); // no contact this step -> coast/neutral
                }
                if (pushed > 0)
                {
                    scripted += 1;
                }

                Intent i = InputAdapter.sampleIntent(st);
                InputAdapter.endStep();

                int a = InputAdapter.codes.angle[(int)st];
                int r = InputAdapter.codes.rate[(int)st];
                if (pushed > 0)
                {
                    int ea = InputDigest.quantizeAngle(firstX, firstY);
                    int er = InputDigest.quantizeRate(firstR);
                    if (a != ea || r != er)
                    {
                        System.Console.Write("FAIL step=" + st + " field=inputAdapter.firstContact" +
                            " (got " + a + "/" + r + " expected " + ea + "/" + er + ")\n");
                        ok = false;
                    }
                }
                else if (a != InputDigest.ANGLE_COAST || r != InputDigest.RATE_NEUTRAL)
                {
                    System.Console.Write("FAIL step=" + st + " field=inputAdapter.coast" +
                        " (no contact must record coast/neutral)\n");
                    ok = false;
                }
                if (!ok)
                {
                    break;
                }
            }
            if (!ok)
            {
                Commands.fileRecord("fail");
                return 1;
            }
            Fail.check(evAt == evCount, "ehpointer1: script has events past the run length");

            byte[] dg = InputAdapter.encodeChannel();

            // ---- transport fidelity: decode(encode(channel)) == channel ------
            for (long st = 0; st < maxSteps; st += 100L)
            {
                Intent d = InputDigest.decode(dg, st);
                int da = InputDigest.quantizeAngle(d.thrustX, d.thrustY);
                int dr = InputDigest.quantizeRate(d.rate);
                int a = InputAdapter.codes.angle[(int)st];
                int r = InputAdapter.codes.rate[(int)st];
                if (da != a || dr != r)
                {
                    System.Console.Write("FAIL step=" + st + " field=inputDigest.roundTrip" +
                        " (got " + da + "/" + dr + " expected " + a + "/" + r + ")\n");
                    Commands.fileRecord("fail");
                    return 1;
                }
            }

            // ---- the channel drives a run, and that run is reproducible ------
            Commands.digest = dg;
            Commands.seedHex = Commands.hexU64(pSeed);
            long[] st1 = new long[64];
            byte[][] bl1 = new byte[64][];
            int n1 = 0;
            SimState s1 = LevelLoader.load(pLevel, pSeed);
            bool c1 = Commands.runLoop(s1, dg, 600L, st1, bl1, out n1) == 1;

            long[] st2 = new long[64];
            byte[][] bl2 = new byte[64][];
            int n2 = 0;
            SimState s2 = LevelLoader.load(pLevel, pSeed);
            bool c2 = Commands.runLoop(s2, dg, 600L, st2, bl2, out n2) == 1;

            System.Console.Write("scriptDirectives=" + evCount + " scriptedSteps=" + scripted +
                " dupSteps=" + dupSteps + " pauseSteps=" + pauseSteps + "\n");
            System.Console.Write("digest=" + Counters.hexEncode(dg, 0, dg.Length) + "\n");
            System.Console.Write("stepsSimulated=" + s1.stepIndex + "\n");
            if (n1 != n2 || s1.stepIndex != s2.stepIndex || c1 != c2)
            {
                System.Console.Write("FAIL the same channel replayed differently\n");
                Commands.fileRecord("fail");
                return 1;
            }
            for (int q = 0; q < n1; q++)
            {
                if (st1[q] != st2[q] || !cmpSnap(bl1[q], bl2[q], st1[q]))
                {
                    Commands.fileRecord("fail");
                    return 1;
                }
            }
            System.Console.Write("PASS snapshotsCompared=" + n1 +
                " result=" + (c1 ? "goal" : "timeout") + " exit=0\n");
            Commands.fileRecord("pass");
            return 0;
        }

        public static string[] splitTokens(string s)
        {
            string[] acc = new string[8];
            int n = 0;
            int i = 0;
            while (i < s.Length)
            {
                if (s[i] == ' ' || s[i] == '\t' || s[i] == '\r')
                {
                    i += 1;
                    continue;
                }
                int j = i;
                while (j < s.Length && s[j] != ' ' && s[j] != '\t' && s[j] != '\r')
                {
                    j += 1;
                }
                Fail.check(n < acc.Length, "ehpointer1: too many tokens");
                acc[n] = s.Substring(i, j - i);
                n += 1;
                i = j;
            }
            string[] res = new string[n];
            for (int q = 0; q < n; q++)
            {
                res[q] = acc[q];
            }
            return res;
        }

        // ======================= H-09 tier =====================================
        public static int tier(string[] argv, int at)
        {
            int k = at;
            while (k < argv.Length)
            {
                if (argv[k] == "--probe")
                {
                    k += 1;
                }
                else if (argv[k] == "--seed")
                {
                    Fail.check(k + 1 < argv.Length, "missing value after --seed");
                    pSeed = Commands.parseHexU64(argv[k + 1]);
                    k += 2;
                }
                else if (argv[k] == "--digest")
                {
                    Fail.check(k + 1 < argv.Length, "missing value after --digest");
                    pDigest = Commands.readBytes(argv[k + 1]);
                    k += 2;
                }
                else if (argv[k] == "--level")
                {
                    Fail.check(k + 1 < argv.Length, "missing value after --level");
                    pLevel = argv[k + 1];
                    k += 2;
                }
                else
                {
                    usage2();
                    return 2;
                }
            }
            defaultIdentity();
            Commands.seedHex = Commands.hexU64(pSeed);
            Commands.digest = pDigest;

            // A-019 §10 says the probe RETURNS the selected tier. There is no
            // device here, so no tier is selected and none is guessed: inventing
            // a default would let a quality setting become a difficulty setting
            // on a real device with no test able to see it.
            int dev = FrameDriver.selectTier();
            System.Console.Write("deviceTier=" + (dev == 0 ? "unknown (no device in this environment; device-derived selection belongs to A-023)" : "" + dev) + "\n");

            SimState a = LevelLoader.load(pLevel, pSeed);
            SimState b = LevelLoader.load(pLevel, pSeed);
            SimState c = LevelLoader.load(pLevel, pSeed);
            FrameDriver.FrameClock ca = FrameDriver.boot(60, FixedQ.ONE, 0);
            FrameDriver.FrameClock cb = FrameDriver.boot(60, FixedQ.ONE, 0);
            FrameDriver.FrameClock cc = FrameDriver.boot(60, FixedQ.ONE, 0);
            FrameDriver.setTier(ca, 1);
            FrameDriver.setTier(cb, 2);
            FrameDriver.setTier(cc, 3);

            long cp = 0;
            while (true)
            {
                FrameDriver.frame(ca, a, pDigest);
                FrameDriver.frame(cb, b, pDigest);
                FrameDriver.frame(cc, c, pDigest);
                if (!cmpSnap(Commands.emitSnapshot(a), Commands.emitSnapshot(b), a.stepIndex) ||
                    !cmpSnap(Commands.emitSnapshot(a), Commands.emitSnapshot(c), a.stepIndex))
                {
                    Commands.fileRecord("fail");
                    return 1;
                }
                cp += 1;
                if (FrameDriver.goal(a))
                {
                    break;
                }
            }

            System.Console.Write("tiersProbed=3\n");
            System.Console.Write("renderPixelsPerUnit=" + ca.pixelsPerUnit + "," +
                cb.pixelsPerUnit + "," + cc.pixelsPerUnit + "\n");
            System.Console.Write("goalSteps=" + a.stepIndex + "," + b.stepIndex + "," + c.stepIndex + "\n");
            System.Console.Write("snapshotsCompared=" + cp + "\n");

            if (b.stepIndex != a.stepIndex || c.stepIndex != a.stepIndex)
            {
                System.Console.Write("FAIL a quality tier changed where the level ends\n");
                Commands.fileRecord("fail");
                return 1;
            }
            // non-vacuity: the tiers must actually differ in render state, or
            // the probe compared three identical clocks and proved nothing.
            if (!(ca.pixelsPerUnit != cb.pixelsPerUnit && cb.pixelsPerUnit != cc.pixelsPerUnit))
            {
                System.Console.Write("FAIL the tiers selected no distinct render state\n");
                Commands.fileRecord("fail");
                return 1;
            }
            System.Console.Write("PASS snapshotsCompared=" + cp + " exit=0\n");
            Commands.fileRecord("pass");
            return 0;
        }

        public static void usage2()
        {
            System.Console.Write("usage: harness hitch --at <f,f,..> --ms <n> [--seed <hex> --digest @file --level <id>]\n" +
                "       harness fps --target <60|30> [--time-scale <d.d>] [--seed <hex> --digest @file --level <id>]\n" +
                "       harness pointer --script @file [--seed <hex> --level <id>]\n" +
                "       harness tier --probe [--seed <hex> --digest @file --level <id>]\n");
        }
    }
}

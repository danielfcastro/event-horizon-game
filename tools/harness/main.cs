// A-021 Phase 1 prototype — tools/harness/main.cs
// Entry point (A-019 §11): "tools/harness/main.cs builds the same SimState and calls
// the same step". There is exactly ONE simulation loop in the program: Commands.runLoop,
// which does LevelLoader.load -> FixedStepDriver.step. GameLoop.cs (Unity, phase 2) is
// the other entry and reaches that same loop. main.cs never re-implements stepping.
//
// A-019 §5 requires two guards before any command runs:
//   1. the harness self-tests both fixed-point helpers; H-01 refuses to start on failure;
//   2. the harness hashes the Fixed/ and SimCore/ source sets and fails FIXED-FORK on
//      mismatch, so a harness linked against a sim core that differs from the one the
//      committed goldens were produced with cannot silently pass.
// Exit codes are A-019 §9: 0 pass, 1 fail, 2 harness error.

namespace EH
{
    public static class HarnessEntry
    {
        // System.IO.Directory exposes no List/Walk in this toolchain, so the hashed set
        // is enumerated here; the fork hash covers exactly this list.
        public static string[] fixedFiles =
        {
            "Assets/Runtime/Fixed/FixedQ.cs",
            "Assets/Runtime/Fixed/MulDiv.cs",
            "Assets/Runtime/Fixed/ISqrtQ.cs",
        };
        public static string[] simFiles =
        {
            "Assets/Runtime/SimCore/SimState.cs",
            "Assets/Runtime/SimCore/SimStep.cs",
            "Assets/Runtime/SimCore/FixedStepDriver.cs",
            "Assets/Runtime/SimCore/PRNG.cs",
            "Assets/Runtime/SimCore/BodyPool.cs",
            "Assets/Runtime/SimCore/SpatialHash.cs",
            "Assets/Runtime/SimCore/InputDigest.cs",
            "Assets/Runtime/SimCore/PlayerController.cs",
            "Assets/Runtime/SimCore/AttractionSystem.cs",
            "Assets/Runtime/SimCore/AbsorptionSystem.cs",
            "Assets/Runtime/SimCore/GrowthSystem.cs",
            "Assets/Runtime/SimCore/HazardSystem.cs",
            "Assets/Runtime/SimCore/Stability.cs",
            "Assets/Runtime/SimCore/Combo.cs",
            "Assets/Runtime/SimCore/Scoring.cs",
            "Assets/Runtime/SimCore/SpawnDirector.cs",
        };

        // Recomputed by hand whenever Fixed/ or SimCore/ changes; a stale value is a
        // hard FIXED-FORK failure, never a silent pass. Blessed 2026-10-07 from the
        // 19-file set after the MulDiv/ISqrtQ algorithm fixes and the BodyPool.tierCount
        // active-flag fix (see PROGRESS.md).
        public static ulong forkExpected = 0x99dd49de20c48bc4UL;

        // ---- guard 1: fixed-point helper self-tests (A-019 §5) ----------------
        public static bool selfTestFixed()
        {
            bool md = MulDiv.selfTest();
            bool sq = ISqrtQ.selfTest();
            if (!md || !sq)
            {
                System.Console.Write("FIXED-SELFTEST-FAIL muldiv=" + md + " isqrt=" + sq
                    + " (H-01 refuses to start; A-019 §5)\n");
                return false;
            }
            return true;
        }

        // ---- guard 2: FIXED-FORK over the hashed source sets -------------------
        // FNV-1a 64 over every byte of every hashed file, in the listed order, with
        // each path and length mixed in so a renamed or truncated file changes the
        // hash. Reported value is printed for the run record.
        public static long forkHash(out ulong actual)
        {
            ulong h = 0xcbf29ce484222325UL;
            const ulong prime = 0x100000001b3UL;
            int files = 0;
            int k = 0;
            while (k < fixedFiles.Length + simFiles.Length)
            {
                string path = k < fixedFiles.Length ? fixedFiles[k] : simFiles[k - fixedFiles.Length];
                byte[] data;
                try
                {
                    data = System.IO.File.ReadAllBytes(path);
                }
                catch (System.IO.IOException)
                {
                    System.Console.Write("FIXED-FORK: hashed file missing: " + path + "\n");
                    actual = 0UL;
                    return -1L;
                }
                int p = 0;
                while (p < path.Length)
                {
                    h = (h ^ (ulong)(path[p] & 0xFF)) * prime;
                    p++;
                }
                h = (h ^ (ulong)data.Length) * prime;
                int q = 0;
                while (q < data.Length)
                {
                    h = (h ^ (ulong)(data[q] & 0xFF)) * prime;
                    q++;
                }
                files++;
                k++;
            }
            actual = h;
            return (long)files;
        }

        public static int Main(string[] argv)
        {
            if (!selfTestFixed())
            {
                System.Console.Write("harness: fixed-point helpers failed, no command ran\n");
                return 1;
            }
            ulong actual = 0UL;
            long files = forkHash(out actual);
            if (files < 0L)
            {
                System.Console.Write("harness: FIXED-FORK source set incomplete\n");
                return 1;
            }
            if (forkExpected != 0UL && forkExpected != actual)
            {
                System.Console.Write("FIXED-FORK expected=" + Commands.hexU64(forkExpected)
                    + " actual=" + Commands.hexU64(actual)
                    + " (sim core changed without re-blessing the goldens)\n");
                return 1;
            }
            if (forkExpected == 0UL)
            {
                System.Console.Write("FIXED-FORK not yet blessed: actual="
                    + Commands.hexU64(actual) + " files=" + files + "\n");
            }
            // A-019 §11: goldens are committed fixtures. A missing fixture is a
            // harness error (exit 2), not a simulation failure (exit 1), and it
            // must be reported before the command reads the file and throws.
            // sim requires the recorded digest; replay requires the golden JSON
            // (which sim --out produces, so sim must not require it).
            if (argv.Length > 0 && (argv[0] == "sim" || argv[0] == "replay"))
            {
                int missing = Replays.checkCommitted(argv[0]);
                if (missing > 0)
                {
                    System.Console.Write("harness: " + missing +
                        " committed golden fixture(s) missing, command did not run\n");
                    return 2;
                }
            }
            // A malformed argument (e.g. --seed 0xZZZ) throws SystemicFailure from
            // parseHexU64. Left uncaught, that aborts the process (exit 134, core
            // dumped), which violates A-019 §9: a harness/user error must exit 2.
            // This catch keeps the exit-code contract. It must NOT swallow a
            // simulation failure: those return 1 from Commands.run normally, never
            // as an exception, so exit 1 stays distinguishable from exit 2 here.
            try
            {
                return Commands.run(argv);
            }
            catch (SystemicFailure e)
            {
                System.Console.Write("harness: " + e.reason + " (command did not complete)\n");
                return 2;
            }
        }
    }
}

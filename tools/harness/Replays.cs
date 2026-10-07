// A-021 Phase 1 prototype — tools/harness/Replays.cs
// Golden fixtures are committed files, never generated at test time: H-02's whole point
// is that the bytes on disk were produced by an earlier blessed run. This file is the
// registry of which fixtures are committed and where they live (A-019 §11 filing), so
// a missing fixture is reported as a harness error instead of silently passing.
namespace EH
{
    public static class Replays
    {
        // Phase 1 commits exactly one golden (PLAN 5.18: one level). A-014's full
        // golden set (8 seeds x 3 digests) is phase 2 and is not claimed here.
        public static string[] levels = { "p1-level-01" };

        public static string jsonPath(string levelId)
        {
            return "replays/" + levelId + ".json";
        }

        public static string digestPath(string levelId)
        {
            return "replays/" + levelId + ".digest.bin";
        }

        // Returns the number of committed fixtures absent from disk for the
        // given command. sim needs the recorded digest (its input); the golden
        // JSON is what sim --out PRODUCES, so only replay requires it.
        public static int checkCommitted(string cmd)
        {
            int missing = 0;
            int k = 0;
            while (k < levels.Length)
            {
                if (cmd == "sim")
                {
                    if (!System.IO.File.Exists(digestPath(levels[k])))
                    {
                        System.Console.Write("golden missing: " + digestPath(levels[k]) + "\n");
                        missing++;
                    }
                }
                if (cmd == "replay")
                {
                    if (!System.IO.File.Exists(jsonPath(levels[k])))
                    {
                        System.Console.Write("golden missing: " + jsonPath(levels[k]) + "\n");
                        missing++;
                    }
                }
                k++;
            }
            return missing;
        }
    }
}

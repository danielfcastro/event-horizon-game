// A-021 Phase 1 prototype — tools/harness/HitchInjector.cs
// Contract-only. A-019 §4 puts hitch injection in the frame driver, and phase 1 has no
// frame driver: the harness steps the fixed loop directly and never runs a frame loop,
// so there is no frame clock to stall. H-06 therefore reports NOT-AVAILABLE and exits
// 2 (A-019 §9: harness error / not applicable). It never reports a pass, because a
// phase with no clock cannot prove a phase-2 property.
namespace EH
{
    public static class HitchInjector
    {
        // False in phase 1: no frame driver exists, so there is nothing to inject into.
        public static bool available()
        {
            return false;
        }

        // Phase 2 signature, fixed now so the call site does not move later. inject()
        // must return 0 only when it actually stalled a frame and observed the sim
        // loop survive it; until then it returns 2 and says why.
        public static int inject(string[] argv, int at)
        {
            System.Console.Write("hitch: phase 1 has no frame driver; H-06 is phase 2 work " +
                "(A-019 §4)\n");
            return 2;
        }
    }
}

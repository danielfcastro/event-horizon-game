// A-021 Phase 1 prototype — tools/harness/HitchInjector.cs
// A-022: the frame driver now exists (FrameDriver.cs), so the phase-1 NOT-AVAILABLE
// answer is gone and this file becomes the routing point A-019 §4 asks for: hitch
// injection belongs to the frame driver, and this file delegates rather than owning
// a second hitch model. The signature stays fixed so the call site in Commands does
// not move.
//
// Phase-1 history kept for honesty: with no frame clock there was nothing to stall,
// so H-06 exited 2 rather than reporting a pass it could not prove. That is no longer
// true and available() says so.
namespace EH
{
    public static class HitchInjector
    {
        // True from A-022 on: FrameDriver owns a frame clock, so a frame CAN be
        // stalled and H-06 can actually be run.
        public static bool available()
        {
            return true;
        }

        // Phase 2 signature, fixed now so the call site does not move later. inject()
        // must return 0 only when it actually stalled a frame and observed the sim
        // loop survive it; until then it returns 2 and says why.
        //
        // A-022: the hitch is a property of the frame clock, so the work lives in
        // FrameCommands.hitch, which drives FrameDriver and compares SimState bytes.
        // This file owns no hitch arithmetic of its own — a duplicated stall model
        // would let H-06 pass while the real frame driver stalled something else.
        public static int inject(string[] argv, int at)
        {
            return FrameCommands.hitch(argv, at);
        }
    }
}

// A-021 Phase 1 prototype — Assets/Runtime/Bridge/Unity/RenderLayer.cs
// A-019 §3 layout / §4 table: RenderLayer.cs (camera, render only) — status
// "stub (camera only)": `void draw(SimState s, float alpha)`; alpha is render-
// only interpolation, NEVER fed to SimCore.
//
// *** CONTRACT-ONLY: NOT COMPILED IN THE HEADLESS BUILD ***
// Excluded from harness.csproj by the <Compile ... Exclude> clause (brief §9).
// Uses the Unity API (Camera, GameObject, Mesh). Authored so the shipping
// target and the harness share one state representation: this file READS
// SimState, it never writes to it.
//
// Hard rules honoured:
//  - pixelsPerUnit is render-only (A-019 §6): SimCore never reads it, and the
//    sim is resolution/DPI-independent by construction. Mapping WU -> px happens
//    here and nowhere else.
//  - alpha interpolates RENDER positions between fixed steps only. SimCore is
//    never given alpha, a wall-clock value, or a float (A-019 §5).
//  - Out of scope for A-021 (brief §9): particles, camera zoom curve, sound,
//    mass-meter layout (A-010). Those slots are left as named absences, not
//    faked with placeholder behaviour that later artifacts would have to hunt
//    for.
//  - Draw order is the sim's own ascending body index (A-019 §4: every system
//    iterates ascending index), so rendering never introduces a second,
//    divergent traversal order.

namespace EH
{
    public static class RenderLayer
    {
        // Camera transform in world units (WU), not pixels: the WU -> px mapping
        // happens once, below, through pixelsPerUnit.
        public static long cameraXRaw;
        public static long cameraYRaw;

        /// <summary>
        /// Draw one frame. `alpha` in [0,1) is the fraction of DT still sitting
        /// in the accumulator (computed by GameLoop from s.accumulator / DT); it
        /// interpolates RENDER positions between the last fixed step and the next
        /// one. It is never passed to SimCore, FixedStepDriver, or any Data
        /// table.
        /// </summary>
        public static void draw(SimState s, float alpha)
        {
            // camera follows the player hole. A-019 §4 words this as "body index
            // 0", but SimState stores the hole in dedicated fields (holeX/holeY/
            // holeRadius) and reserves bodyX[]..bodyRadius[] for the rest, so the
            // camera reads the hole fields, not bodyX[0].
            cameraXRaw = s.holeX;
            cameraYRaw = s.holeY;

            // WU -> screen pixels, the single place pixelsPerUnit is read.
            long ppu = PlatformBridge.pixelsPerUnit; // Q32.32 px per WU
            UnityEngine.Camera.main.SetWorldWindow(cameraXRaw, cameraYRaw, ppu);

            // ascending body index, same order every system uses; inactive
            // records are skipped by the active-flag gate, never drawn as ghosts.
            for (int body = 0; body < SimState.BODY_CAPACITY; body++)
            {
                if ((s.bodyFlags[body] & 0x01) == 0)
                {
                    continue;
                }
                // render radius is the VISUAL radius (1 WU at starting mass),
                // not the gameplay eventRadius/absorptionRadius — those are
                // invisible to the player by design (A-005 §-invisible-forces).
                long rRaw = s.bodyRadius[body];
                UnityEngine.Shape.circleWorldRadius(rRaw, ppu,
                    s.bodyX[body], s.bodyY[body]);
            }

            // hazard pool stays empty in phase 1 (A-009: no hazards on p1-level-01)
            // and HazardSystem is a stub, so there is nothing to draw here; the
            // slot is documented, not faked.
        }

        /// <summary>
        /// Absent in phase 1, named so later artifacts find the slot rather than
        /// inventing a parallel one: particles, camera zoom curve, and sound are
        /// A-010/A-012 owned (brief §9 scope boundaries).
        /// </summary>
        public static void drawEffectsAbsent(SimState s)
        {
            // no-op: phase 1 ships no particles and no zoom curve.
        }
    }
}

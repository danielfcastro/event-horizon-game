// A-023 Unity player package — Assets/Runtime/Bridge/Unity/RenderLayer.cs
// Upgraded from the A-021 "stub (camera only)" contract to the player-facing
// render layer: the §5.3 hybrid camera rule, culling, the horizon ring, and the
// visibilityLayer hook (A-005 §5.2 module tree; A-026 states the read-only
// contract, repeated by A-005 §5.2: "RenderLayer is strictly read-only against
// sim state").
//
// *** NOT COMPILED IN THE HEADLESS BUILD (excluded from harness.csproj and
// player.csproj by their Exclude clauses) *** It uses the Unity runtime API.
// Authored against the Unity 6 LTS 6000.0.84f1 API; NOT compiled or run here —
// the license gate (PLAN.md §4 A-023: the editor exits 198, "No licenses were
// found") blocks any Unity-side compile until sign-in. If the editor build
// rejects a call when the gate opens, that is the drift to fix at build time;
// nothing here may pass quietly.
//
// Hard rules honoured here:
//  - The camera rule is PLAN.md §5.3 verbatim — the SAME rule A-026's view.c
//    implements and whose frames were actually looked at:
//        shortEdgePx = min(windowWidthPx, windowHeightPx)   # the ACTIVE window
//        ppuWorld    = shortEdgePx / boundsH                # phase 1: world-fit
//        ppuContain  = CONTAIN_PCT% * shortEdgePx / (2 * eventRadiusWU) # phase 2
//        pixelsPerUnit = clamp(min(ppuWorld, ppuContain), MIN_PPU, tierPPU)
//        CONTAIN_PCT = 90   MIN_PPU = 2
//    Nothing is re-decided here; boundsW/boundsH are read from SimState (booted
//    from LevelTable by LevelLoader — the same two fields A-026 added to its
//    payload for this rule), never from a hardcoded resolution or world.
//  - pixelsPerUnit is render-only (A-019 §6): chosen here from the active window
//    and the tier's pixelsPerUnit (PlatformBridge.pixelsPerUnit, set by
//    FrameDriver.setTier — render state only), and never read by SimCore.
//  - This file READS SimState and never writes to it. No field of `s` is
//    assigned anywhere below; the render pool and the visibility hook are the
//    layer's own state.
//  - No float/double reaches SimCore. The doubles below are render-side only:
//    the Q32.32 sim values are converted once, here, for the camera.
//  - Draw order is ascending body index (A-019 §4), so rendering never
//    introduces a second, divergent traversal order.
//  - Pools: the render pool is allocated once at boot() and reused every frame
//    (A-014 P-05 names per-frame allocation counters reading 0 after warm-up on
//    the weak device; the render side must not allocate per frame either).
//  - P-08 (draw calls, modern phone): culling against the active window keeps
//    off-screen bodies out of the draw set; the pool bounds the object count.
//  - What the gates need is recorded, not claimed: P-01/P-02/P-04/P-05 need
//    real hardware (PLAN.md §4 A-023 and the A-014 device classes); this file
//    only makes them measurable.

namespace EH
{
    public static class RenderLayer
    {
        // ---- §5.3 constants (dimensionless; the same two A-026's view.c carries) --
        public static int containPct = 90;   // CONTAIN_PCT — PLAN.md §5.3
        public static int minPpu = 2;        // MIN_PPU — PLAN.md §5.3

        // ---- the render pool (allocated once at boot, reused every frame) -----
        private static UnityEngine.GameObject[] bodyObjects;
        private static UnityEngine.SpriteRenderer[] bodySprites;
        private static UnityEngine.Sprite defaultSprite;
        private static UnityEngine.GameObject horizonObject;
        private static UnityEngine.SpriteRenderer horizonSprite;
        private static bool booted;

        // ---- visibilityLayer output (A-005 data flow: "a hook, not a sim write") --
        // Bodies culled from the draw set but within one screen radius of the
        // active window's edge, as ascending body indices. A-010 (UI) consumes
        // this for edge indicators; nothing here draws them.
        public static int[] visibilityLayer;
        public static int visibilityCount;

        /// <summary>
        /// Allocate the render pool and the visibility buffer once. Called by the
        /// player shell at boot; a second boot is a hard rejection (a re-allocated
        /// pool is per-frame allocation in disguise).
        /// </summary>
        public static void boot()
        {
            Fail.check(!booted, "RenderLayer.boot: the pool exists to be allocated once");
            bodyObjects = new UnityEngine.GameObject[SimState.BODY_CAPACITY];
            bodySprites = new UnityEngine.SpriteRenderer[SimState.BODY_CAPACITY];
            for (int body = 0; body < SimState.BODY_CAPACITY; body++)
            {
                bodyObjects[body] = new UnityEngine.GameObject("eh-render-" + body);
                bodySprites[body] = bodyObjects[body].AddComponent<SpriteRenderer>();
            }
            horizonObject = new UnityEngine.GameObject("eh-horizon");
            horizonSprite = horizonObject.AddComponent<SpriteRenderer>();
            // The engine's built-in default square stands in until A-006's art is
            // bundled; the placeholder is named, not hidden (A-006/A-010 own the
            // visuals — brief §9 scope boundary).
            defaultSprite = Resources.GetBuiltinResource<UnityEngine.Sprite>("Default-Sprite.png");
            visibilityLayer = new int[SimState.BODY_CAPACITY];
            booted = true;
        }

        /// <summary>
        /// Draw one frame. `alpha` in [0,1) is the frame driver's render-only
        /// interpolation fraction (FrameClock.renderAlphaRaw / 1.0); it is never
        /// passed to SimCore, FixedStepDriver, or any Data table. Phase-1
        /// rendering draws at the current fixed positions; alpha is accepted so
        /// the interpolation slot is named, not invented (the previous-step
        /// render buffer is A-006/A-010's visual decision, not a phase-1 rule).
        /// Returns nothing; the layer never writes to `s`.
        /// </summary>
        public static void draw(SimState s, float alpha)
        {
            Fail.check(booted, "RenderLayer.draw: boot() must allocate the pool first");

            // ---- the §5.3 rule, one expression, two phases ----------------------
            // shortEdgePx from the ACTIVE window, never a constant (PLAN.md §5.3).
            int windowW = UnityEngine.Display.Primary.virtualWidth;
            int windowH = UnityEngine.Display.Primary.virtualHeight;
            double shortEdge = windowW < windowH ? (double)windowW : (double)windowH;

            // bounds and horizon in WU as a double: Q32.32 / 2^32. This is the SAME
            // read A-026's view.c makes (`(double)p->holeEventRadius / 4294967296.0`);
            // truncating with >> 32 would let the two implementations of the §5.3
            // rule drift by up to one world unit (a few pixels) on the same state.
            double boundsH = (double)(long)s.boundsH / 4294967296.0;
            double evWu = (double)(long)s.holeEventRadius / 4294967296.0;

            double ppuWorld = boundsH > 0.0 ? shortEdge / boundsH : (double)minPpu;
            double ppuContain = evWu > 0.0
                ? (shortEdge * (double)containPct / 100.0) / (2.0 * evWu)
                : ppuWorld;

            double want;
            if (ppuWorld <= ppuContain)
            {
                want = ppuWorld; // phase 1: fixed world-fitting view, growth visible
            }
            else
            {
                want = ppuContain; // phase 2: smooth containment, camera only zooms out
            }
            // tierPPU is the upper bound (FrameDriver.setTier owns it; a sharper
            // device gets a sharper picture, never a different framing). Read as a
            // full Q32.32 quotient, not truncated, so the cap is exact.
            double tierPpu = (double)(long)PlatformBridge.pixelsPerUnit / 4294967296.0;
            if (tierPpu < (double)minPpu)
            {
                tierPpu = (double)minPpu;
            }
            if (want < (double)minPpu)
            {
                want = (double)minPpu;
            }
            if (want > tierPpu)
            {
                want = tierPpu;
            }
            double ppu = want; // px per WU, render-only

            // ---- camera: orthographic, centred on the hole ---------------------
            // Unity 2D units are world units; orthographicSize is half the world
            // height, so worldHeight = windowHeightPx / ppu.
            UnityEngine.Camera camera = UnityEngine.Camera.main;
            camera.orthographic = true;
            camera.orthographicSize = (windowH / ppu) * 0.5;
            double holeXWu = (double)s.holeX >> 32;
            double holeYWu = (double)s.holeY >> 32;
            camera.GetComponent<UnityEngine.Transform>().worldPosition =
                new UnityEngine.Vector3(holeXWu, holeYWu, 0.0);

            // ---- horizon ring: the VISIBLE hole is the event horizon (A-026's
            // render fix established this: drawing holeRadius, the constant
            // collision core, made the hole look frozen) --------------------------
            horizonObject.SetActive(true);
            horizonSprite.sprite = defaultSprite;
            UnityEngine.Transform horizonT = horizonObject.GetComponent<UnityEngine.Transform>();
            horizonT.worldPosition = new UnityEngine.Vector3(holeXWu, holeYWu, 0.0);
            double dWu = 2.0 * evWu;
            horizonT.localScale = new UnityEngine.Vector3(dWu, dWu, 1.0);

            // ---- bodies: ascending index, culled against the active window ------
            double halfWWu = (windowW / ppu) * 0.5;
            double halfHWu = (windowH / ppu) * 0.5;
            visibilityCount = 0;
            for (int body = 0; body < SimState.BODY_CAPACITY; body++)
            {
                if ((s.bodyFlags[body] & 0x01) == 0)
                {
                    bodyObjects[body].SetActive(false); // inactive records are never drawn as ghosts
                    continue;
                }
                double dxWu = ((double)s.bodyX[body] >> 32) - holeXWu;
                double dyWu = ((double)s.bodyY[body] >> 32) - holeYWu;
                double rWu = (double)s.bodyRadius[body] >> 32;
                if (dxWu > halfWWu + rWu || dxWu < -(halfWWu + rWu) ||
                    dyWu > halfHWu + rWu || dyWu < -(halfHWu + rWu))
                {
                    bodyObjects[body].SetActive(false);
                    // near-edge but culled: the visibilityLayer hook (a hook, not
                    // a sim write — the UI layer reads it, the sim never does).
                    if (dxWu <= halfWWu * 2.0 + rWu && dxWu >= -(halfWWu * 2.0 + rWu) &&
                        dyWu <= halfHWu * 2.0 + rWu && dyWu >= -(halfHWu * 2.0 + rWu))
                    {
                        visibilityLayer[visibilityCount] = body;
                        visibilityCount += 1;
                    }
                    continue;
                }
                bodyObjects[body].SetActive(true);
                bodySprites[body].sprite = defaultSprite;
                UnityEngine.Transform t = bodyObjects[body].GetComponent<UnityEngine.Transform>();
                t.worldPosition = new UnityEngine.Vector3(holeXWu + dxWu, holeYWu + dyWu, 0.0);
                t.localScale = new UnityEngine.Vector3(2.0 * rWu, 2.0 * rWu, 1.0);
            }

            // hazard pool: empty on p1-level-01 (A-009); the slot is documented,
            // not faked — HazardSystem is a stub and there is nothing to draw.
        }

        /// <summary>
        /// Named absences so later artifacts find the slot instead of inventing a
        /// parallel one: particles, the horizon-ring art, and the edge-indicator
        /// drawing are A-006/A-010 owned (brief §9 scope boundaries); this layer
        /// exposes the hook and the camera, not the art.
        /// </summary>
        public static void drawEffectsAbsent(SimState s)
        {
            // no-op: phase 1 ships no particles and no zoom curve.
        }
    }
}

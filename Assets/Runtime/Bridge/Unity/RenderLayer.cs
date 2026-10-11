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
        private static UnityEngine.GameObject horizonRimObject;
        private static UnityEngine.SpriteRenderer horizonRimSprite;
        private static bool booted;

        // ---- the camera this layer owns ------------------------------------------
        // The player package has no scene camera: the first real-device run (2026-10-11
        // on the S24 FE) rendered an empty field and Unity logged "Could not produce
        // class with ID 115" for every sprite lookup, because Camera.main was null and
        // the engine had nothing to present. The §5.3 rule owns the camera, so this
        // layer creates it once at boot and keeps the reference; Camera.main is never
        // read, because a runtime-created camera carries no MainCamera tag.
        private static UnityEngine.GameObject cameraObject;
        private static UnityEngine.Camera camera;

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
                bodySprites[body] = bodyObjects[body].AddComponent<UnityEngine.SpriteRenderer>();
            }
            horizonObject = new UnityEngine.GameObject("eh-horizon");
            horizonSprite = horizonObject.AddComponent<UnityEngine.SpriteRenderer>();
            // A-026's client draws the horizon as a true black disc with a bright
            // accretion RIM; one sprite cannot do both, so the rim is a second
            // disc drawn behind it, exactly as view.c layers ring over disc.
            horizonRimObject = new UnityEngine.GameObject("eh-horizon-rim");
            horizonRimSprite = horizonRimObject.AddComponent<UnityEngine.SpriteRenderer>();
            // A-026's palette, mirrored exactly (tools/player/view.c): the accretion
            // rim is pack_px(140, 236, 255) and the hole itself is true black.
            horizonRimSprite.color = new UnityEngine.Color(140.0f / 255.0f, 236.0f / 255.0f, 255.0f / 255.0f, 1.0f);
            horizonSprite.color = new UnityEngine.Color(0.0f, 0.0f, 0.0f, 1.0f);
            // The engine's built-in shapes stand in until A-006's art is bundled;
            // the placeholder is named, not hidden (A-006/A-010 own the visuals).
            // The real-device run showed "Could not produce class with ID 115" for
            // every lookup: "Default-Sprite.png" is not a resource on Unity 6, so
            // GetBuiltinResource returned null and nothing drew. The built-in 2D
            // circle is the shape both halves already draw (A-026's view.c fills a
            // disc), so it is the matching placeholder, not a new visual.
            defaultSprite = UnityEngine.Resources.GetBuiltinResource<UnityEngine.Sprite>("Default-Circle.png");
            // The camera the §5.3 rule drives. Created here, once, so the class is
            // present in the build and the framing has an owner.
            cameraObject = new UnityEngine.GameObject("eh-camera");
            camera = cameraObject.AddComponent<UnityEngine.Camera>();
            // Sprites live at z = 0 and the camera is placed at z = 0 by draw(),
            // so the orthographic depth range must straddle zero; the defaults
            // (near 0) clip the very plane everything is drawn on.
            camera.nearClipPlane = -1000.0f;
            camera.farClipPlane = 1000.0f;
            // Field colour matches A-026's client (pack_px(8, 10, 18)); one palette
            // for both halves of the same §5.3 rule, never a second invention.
            // The compiler is the API oracle here: Camera.clearColor is rejected on
            // 6000.0.84f1; Camera.backgroundColor is the property that exists.
            camera.backgroundColor = new UnityEngine.Color(8.0f / 255.0f, 10.0f / 255.0f, 18.0f / 255.0f, 1.0f);
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
            // Unity's game-window size in pixels. Evidence from THIS editor
            // (6000.0.84f1, the compiler as the oracle): Screen.currentResolution
            // and Screen.resolutions are real properties; every Get* method form
            // (GetResolution(), GetResolutions(), GetMainWindowDisplayInfo()) is
            // rejected. The §5.3 rule reads the short edge of the live window,
            // exactly as A-026's view.c reads the SDL window — never a constant.
            var windowPx = UnityEngine.Screen.currentResolution;
            int windowW = windowPx.width;
            int windowH = windowPx.height;
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
            // The camera this layer created at boot; Camera.main is never read
            // (a runtime-created camera carries no MainCamera tag, and the player
            // package has no scene camera).
            Fail.check(camera != null, "RenderLayer.draw: boot() must create the camera");
            camera.orthographic = true;
            camera.orthographicSize = (float)((windowH / ppu) * 0.5);
            // Q32.32 -> world units as a double, then float only at the Unity API
            // boundary. The same read A-026's view.c makes; truncating with >> 32
            // would let the two implementations of the §5.3 rule disagree.
            double holeXWu = (double)s.holeX / 4294967296.0;
            double holeYWu = (double)s.holeY / 4294967296.0;
            camera.GetComponent<UnityEngine.Transform>().position =
                new UnityEngine.Vector3((float)holeXWu, (float)holeYWu, 0.0f);

            // ---- horizon ring: the VISIBLE hole is the event horizon (A-026's
            // render fix established this: drawing holeRadius, the constant
            // collision core, made the hole look frozen) --------------------------
            horizonRimObject.SetActive(true);
            horizonRimSprite.sprite = defaultSprite;
            UnityEngine.Transform rimT = horizonRimObject.GetComponent<UnityEngine.Transform>();
            rimT.position = new UnityEngine.Vector3((float)holeXWu, (float)holeYWu, -1.0f);
            double dWu = 2.0 * evWu;
            rimT.localScale = new UnityEngine.Vector3((float)dWu, (float)dWu, 1.0f);
            // The black disc sits 2 px inside the rim (view.c: ring at hr, inner
            // ring at hr - 2), so the rim reads as a ring, not a filled disc.
            double insetWu = 4.0 / ppu;
            double coreWu = dWu > insetWu * 2.0 ? dWu - insetWu * 2.0 : dWu;
            horizonObject.SetActive(true);
            horizonSprite.sprite = defaultSprite;
            UnityEngine.Transform horizonT = horizonObject.GetComponent<UnityEngine.Transform>();
            horizonT.position = new UnityEngine.Vector3((float)holeXWu, (float)holeYWu, -0.5f);
            horizonT.localScale = new UnityEngine.Vector3((float)coreWu, (float)coreWu, 1.0f);

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
                double dxWu = ((double)s.bodyX[body] / 4294967296.0) - holeXWu;
                double dyWu = ((double)s.bodyY[body] / 4294967296.0) - holeYWu;
                double rWu = (double)s.bodyRadius[body] / 4294967296.0;
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
                t.position = new UnityEngine.Vector3((float)(holeXWu + dxWu), (float)(holeYWu + dyWu), 0.0f);
                t.localScale = new UnityEngine.Vector3((float)(2.0 * rWu), (float)(2.0 * rWu), 1.0f);
                // A-026's body palette, mirrored exactly: tier from the frame's
                // kind bits ((kind >> 2) & 3), and absorbable bodies read
                // brighter, so the two states are separable without colour alone.
                bodySprites[body].color = bodyColor(s.bodyFlags[body]);
            }

            // hazard pool: empty on p1-level-01 (A-009); the slot is documented,
            // not faked — HazardSystem is a stub and there is nothing to draw.
        }

        /// <summary>
        /// A-026's body palette (tools/player/view.c, verbatim): tier bits select
        /// the base colour, absorbable bodies read brighter. Colour is a render
        /// property only - the sim never sees it, and the digest never changes.
        /// </summary>
        private static UnityEngine.Color bodyColor(int flags)
        {
            float r = 84.0f, g = 200.0f, b = 255.0f; // small
            int tier = (flags >> 2) & 3;
            if (tier == 1)
            {
                r = 255.0f; g = 186.0f; b = 64.0f; // medium
            }
            else if (tier == 2)
            {
                r = 255.0f; g = 92.0f; b = 176.0f; // large
            }
            else if (tier == 3)
            {
                r = 122.0f; g = 255.0f; b = 140.0f; // hazard tier
            }
            if ((flags & 0x02) != 0)
            {
                r = r > 160.0f ? 255.0f : r + 60.0f;
                g = g > 160.0f ? 255.0f : g + 60.0f;
                b = b > 160.0f ? 255.0f : b + 60.0f;
            }
            return new UnityEngine.Color(r / 255.0f, g / 255.0f, b / 255.0f, 1.0f);
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

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
//
// A-023 EXTENSION (player measurement surface for P-01/P-05, A-014 "needs,
// not design" item 3): the frame-time histogram, the per-frame allocation
// counter, and the summary lines live here; GameLoop.Update opens each frame
// through perfBeginFrame(). The clock is UnityEngine.Time.realtimeSinceStartup
// — settled by the compiler on 6000.0.84f1 (the temporary ApiProbe compiled it
// with no error while rejecting realtimeDeltaTIme and the validation line
// thisNameDoesNotExistAtAll, so the error list was real). draw() is now
// allocation-free per frame: reused Vector3/Color temporaries and boot-cached
// Transforms replace every `new` and every per-frame GetComponent. The surface
// is a QA instrument: it may log, it may not gate, and it changes nothing the
// player can perceive except log lines.
//
// A-023 TENTH EXTENSION (2026-10-16, the approved Interstellar look — docs/design.md
// §3.2/§3.3/§3.5/§3.6, the A-006 extension merged at f6892c8): the black hole is
// presented as in the film (true-black sphere + accretion glow + bright rim ring),
// the field carries a static starfield, and every body reads as a glowing star
// (bright core + one soft halo behind it) instead of a flat disc. All of it is
// layered soft-alpha sprites with boot-generated textures — no additive blending,
// no custom shaders, no materials (none of those are proven on this package).
// Decisions taken, recorded per the brief:
//  - Ring layering: the MERGED two-disc layering is kept (rim disc behind +
//    smaller black disc over, insetWu = 4.0 / ppu stays). It already produces
//    the constant-width ring using only settled names; a generated ring texture
//    in front was the alternative and was NOT chosen.
//  - Starfield: ONE boot-generated background texture on ONE sprite scaled to
//    the world bounds (the brief's preferred option: one draw call, static by
//    construction — placed on the first draw because bounds are sim state, then
//    never touched again; it never drifts).
//  - z-order (camera at z = -10, larger z nearer, all inside the measured
//    [-2, 0] presenting range): starfield -2, accretion glow -1.5, rim disc -1,
//    true-black sphere -0.5, body glow -0.25, body core 0. The glow sits behind
//    the rim disc so the ring — the only perfect circle and the brightest
//    element (§3.1) — stays unbroken by decoration.
//  - Body stars: two sprites per body (the §3.5 vocabulary verbatim: "a bright
//    core sprite plus one larger, lower-alpha soft sprite behind it"). The core
//    stays the solid disc at the exact sim radius (drawn size follows the sim
//    radius, unchanged); the halo is a generated radial-gradient sprite scaled
//    to 2.5x the core radius, capped at 1/8 of the horizon radius (§3.2 ratio)
//    so glows never merge into a field that hides the ring. A single
//    core+halo texture was rejected: when the cap binds it would shrink the
//    core below the sim radius, breaking the drawn-size rule.
//  - Accretion rotation: ONE render constant, accretionDegPerS, in one place;
//    A-012's reduced-motion option flips it later — the option itself is NOT
//    implemented here (§3.6: the constant is owned by A-023, not a sim
//    quantity).
//  - Math API drift (the compiler is the oracle on 6000.0.84f1; nothing passed
//    quietly): `Math.*` rejected at 4 sites with CS0103 (no such identifier in
//    this namespace context); `UnityEngine.MathF.*` rejected at the same 4 sites
//    with CS0234 (no type `MathF` in namespace UnityEngine — the class is
//    `Mathf`, capital F at the end, per the engine's own strings table);
//    `UnityEngine.Mathf.sqrt/atan2/cos` rejected at the same 4 sites with CS0117
//    (no lowercase members). Settled: `UnityEngine.Mathf.Sqrt/Atan2/Cos`
//    (PascalCase, matching the DLL's `Atan2`/`Ceil`/`Floor` strings), with
//    explicit (float) arguments and (double) results — C# will not implicitly
//    narrow the doubles this layer already keeps. The module XML stubs contain
//    no math names at all; only the compiler decides.

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
        // A-023 tenth extension: the generated soft-alpha bases (§3.5 glow rule —
        // alpha lives in the texture, colour lives in SpriteRenderer.color).
        private static UnityEngine.Sprite glowSprite;    // body halo: radial fade to transparent
        private static UnityEngine.Sprite accretionSpriteTex; // rim-wrapped band + halo, with swirl
        private static UnityEngine.Sprite starfieldSpriteTex; // static field points, weakest element
        private static UnityEngine.GameObject starfieldObject;
        private static UnityEngine.SpriteRenderer starfieldRenderer;
        private static UnityEngine.Transform starfieldTransform;
        private static bool starfieldPlaced; // world bounds are sim state: place once, first draw
        private static UnityEngine.GameObject accretionObject;
        private static UnityEngine.SpriteRenderer accretionRenderer;
        private static UnityEngine.Transform accretionTransform;
        private static UnityEngine.GameObject[] bodyGlowObjects;
        private static UnityEngine.SpriteRenderer[] bodyGlowSprites;
        private static UnityEngine.Transform[] bodyGlowTransform;
        private static UnityEngine.Quaternion tmpQuat; // value type: no per-frame allocation
        private static double accretionAngleDeg; // accumulated spin (wrapped to [0,360))
        // §3.6: the accretion disc rotates slowly around the sphere; its speed is a
        // render constant owned by A-023, kept in ONE place so A-012's reduced-motion
        // option can flip it later (the option itself is NOT implemented here).
        public static double accretionDegPerS = 12.0; // deg/s: one turn per 30 s — slow, calm
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

        // ---- reused temporaries (allocated once at boot; draw() allocates none) --
        // A-023 extension: P-05 names per-frame allocation counters reading 0 after
        // warm-up; draw() is the player's own per-frame code, so every Vector3/Color
        // it needs is written into one of these and assigned through the Unity
        // property setters (which copy the components), never `new` per frame.
        private static UnityEngine.Vector3 tmpPos;
        private static UnityEngine.Vector3 tmpScale;
        private static UnityEngine.Color tmpColor;

        // ---- boot-cached Transforms (GetComponent is a boot cost, not per frame) --
        private static UnityEngine.Transform cameraTransform;
        private static UnityEngine.Transform rimTransform;
        private static UnityEngine.Transform horizonTransform;
        private static UnityEngine.Transform[] bodyTransform;

        // ---- measurement surface: P-01 frame time + P-05 allocation counter ------
        // A QA instrument, not a gameplay feature: it may log, it may not gate
        // (A-019 §12 ship-build exclusions stay green; no dev.* knob, no harness).
        // Histogram: fixed buckets of 0.25 ms, allocated once at boot; the last
        // bucket is overflow. Percentiles come from integer microsecond arithmetic.
        private static int perfBucketUs = 250;   // bucket width in microseconds
        private static int perfBuckets = 1024;   // covers 0..255.75 ms; last = overflow
        private static double perfWarmupS = 10.0;  // A-014 P-01: 10 s warm-up
        private static double perfSummaryS = 5.0;  // summary line every 5 s measured
        private static long[] perfHist;
        private static double perfClockPrev = -1.0; // previous realtime sample
        private static double perfFirstSample;
        private static double perfNextSummary;
        private static long perfFrames;        // measured frames (after warm-up)
        private static long perfWorstUs;       // worst hitch in microseconds
        private static long perfAllocsThisFrame; // allocation sites in the open frame
        private static long perfWarmupAllocMax;  // warm-up max: proves the counter is wired
        private static long perfAllocMax;        // steady-state max per-frame allocations
        private static bool perfSummaryPending;  // the frame that emitted a summary allocates for its own report; excluded
        // Observability (user directive 2026-10-16): what the last draw()
        // actually presented, so one logcat read tells the whole story. Statics
        // only, written in draw(), read in emitPerfSummary; never a new object.
        private static int lastDrawFrame = -1;
        private static int lastVisCount;
        private static int lastHorizonPx;
        private static double lastCamX, lastCamY, lastPPU;

        /// <summary>
        /// Allocate the render pool and the visibility buffer once. Called by the
        /// player shell at boot; a second boot is a hard rejection (a re-allocated
        /// pool is per-frame allocation in disguise).
        /// </summary>
        public static void boot()
        {
            Fail.check(!booted, "RenderLayer.boot: the pool exists to be allocated once");
            // A-023 extension: boot allocations are counted into the same counter,
            // so the summary line's allocs_warmup_max is nonzero and the counter is
            // PROVABLY wired, while steady state must read 0 (A-014 P-05: nonzero
            // after warm-up = leak). One count per allocation statement.
            perfCountAlloc();
            bodyObjects = new UnityEngine.GameObject[SimState.BODY_CAPACITY];
            perfCountAlloc();
            bodySprites = new UnityEngine.SpriteRenderer[SimState.BODY_CAPACITY];
            perfCountAlloc();
            bodyTransform = new UnityEngine.Transform[SimState.BODY_CAPACITY];
            for (int body = 0; body < SimState.BODY_CAPACITY; body++)
            {
                perfCountAlloc(); // the name string
                bodyObjects[body] = new UnityEngine.GameObject("eh-render-" + body);
                perfCountAlloc(); // the GameObject
                perfCountAlloc(); // the SpriteRenderer component
                bodySprites[body] = bodyObjects[body].AddComponent<UnityEngine.SpriteRenderer>();
                bodyTransform[body] = bodyObjects[body].GetComponent<UnityEngine.Transform>();
            }
            // A-023 tenth extension: the body-glow pool (§3.5 bodies-as-stars: one
            // larger, lower-alpha soft sprite BEHIND each core). Same capacity,
            // allocated at boot only; steady-state frames still allocate zero.
            perfCountAlloc();
            bodyGlowObjects = new UnityEngine.GameObject[SimState.BODY_CAPACITY];
            perfCountAlloc();
            bodyGlowSprites = new UnityEngine.SpriteRenderer[SimState.BODY_CAPACITY];
            perfCountAlloc();
            bodyGlowTransform = new UnityEngine.Transform[SimState.BODY_CAPACITY];
            for (int glow = 0; glow < SimState.BODY_CAPACITY; glow++)
            {
                perfCountAlloc(); // the name string
                bodyGlowObjects[glow] = new UnityEngine.GameObject("eh-glow-" + glow);
                perfCountAlloc(); // the GameObject
                perfCountAlloc(); // the SpriteRenderer component
                bodyGlowSprites[glow] = bodyGlowObjects[glow].AddComponent<UnityEngine.SpriteRenderer>();
                bodyGlowTransform[glow] = bodyGlowObjects[glow].GetComponent<UnityEngine.Transform>();
            }
            perfCountAlloc();
            horizonObject = new UnityEngine.GameObject("eh-horizon");
            perfCountAlloc();
            horizonSprite = horizonObject.AddComponent<UnityEngine.SpriteRenderer>();
            horizonTransform = horizonObject.GetComponent<UnityEngine.Transform>();
            // A-026's client draws the horizon as a true black disc with a bright
            // accretion RIM; one sprite cannot do both, so the rim is a second
            // disc drawn behind it, exactly as view.c layers ring over disc.
            perfCountAlloc();
            horizonRimObject = new UnityEngine.GameObject("eh-horizon-rim");
            perfCountAlloc();
            horizonRimSprite = horizonRimObject.AddComponent<UnityEngine.SpriteRenderer>();
            rimTransform = horizonRimObject.GetComponent<UnityEngine.Transform>();
            // A-026's palette, mirrored exactly (tools/player/view.c): the accretion
            // rim is pack_px(140, 236, 255) and the hole itself is true black.
            perfCountAlloc();
            horizonRimSprite.color = new UnityEngine.Color(140.0f / 255.0f, 236.0f / 255.0f, 255.0f / 255.0f, 1.0f);
            perfCountAlloc();
            horizonSprite.color = new UnityEngine.Color(0.0f, 0.0f, 0.0f, 1.0f);
            // The engine's built-in shapes stand in until A-006's art is bundled;
            // the placeholder is named, not hidden (A-006/A-010 own the visuals).
            // The real-device run showed "Could not produce class with ID 115" for
            // every lookup: "Default-Sprite.png" is not a resource on Unity 6, so
            // GetBuiltinResource returned null and nothing drew. The built-in 2D
            // circle is the shape both halves already draw (A-026's view.c fills a
            // disc), so it is the matching placeholder, not a new visual.
            defaultSprite = UnityEngine.Resources.GetBuiltinResource<UnityEngine.Sprite>("Default-Circle.png");
            // Ninth extension (2026-10-16, measured): the built-in lookup
            // returned NULL on this package — the APK's unity_builtin_extra
            // archive carries no Default-Circle entry, and the display-ON
            // screencap showed the field colour everywhere, including inside
            // the horizon radius: a null sprite draws nothing. Fallback, all
            // names settled by the engine's own stubs (Sprite.Create(Texture2D,
            // Rect, Vector2), Texture2D.SetPixels(Color[])): a generated 16x16
            // circle texture. Colour still comes from SpriteRenderer.color,
            // which this layer already owns, so the base is solid white inside
            // the circle and transparent outside. Boot-only allocation: it
            // lands in the warm-up max, never in a steady frame.
            bool spriteFromBuiltin = defaultSprite != null;
            if (!spriteFromBuiltin)
            {
                UnityEngine.Texture2D tex = new UnityEngine.Texture2D(16, 16);
                perfCountAlloc();
                UnityEngine.Color[] px = new UnityEngine.Color[16 * 16];
                perfCountAlloc();
                for (int y = 0; y < 16; y = y + 1)
                {
                    for (int x = 0; x < 16; x = x + 1)
                    {
                        double dx = x - 7.5;
                        double dy = y - 7.5;
                        px[y * 16 + x] = (dx * dx + dy * dy <= 64.0)
                            ? new UnityEngine.Color(1.0f, 1.0f, 1.0f, 1.0f)
                            : new UnityEngine.Color(0.0f, 0.0f, 0.0f, 0.0f);
                    }
                }
                tex.SetPixels(px);
                defaultSprite = UnityEngine.Sprite.Create(tex, new UnityEngine.Rect(0, 0, 16, 16), new UnityEngine.Vector2(8, 8));
            }
            // Observability: the sprite source is now a printed fact, not a
            // guess — the frame's content depends on it.
            UnityEngine.Debug.Log("EH-SPRITE source=" + (spriteFromBuiltin ? "builtin" : "generated"));
            // A-023 tenth extension: the three soft-alpha bases, all boot-generated
            // (the package ships no asset files; the built-in lookup is null here).
            // Recipes, recorded:
            //  - glowSprite: 32x32 radial gradient, alpha 0.5 at centre fading
            //    quadratically to 0 at the edge — the body halo behind a core.
            //  - accretionSpriteTex: 64x64; alpha rises to a peak at r=0.62 (the
            //    sphere's own edge once scaled to 1.6x the horizon radius) and
            //    fades to 0 at r=1.0; a cos(3*theta + 6r) swirl modulates the
            //    band so the slow rotation reads as gas, as in the film.
            //  - starfieldSpriteTex: 256x256, ~110 one-pixel points from a
            //    deterministic xorshift (boot-only; the same seed every run, so
            //    the field is stable across builds), alpha 0.16..0.30. At the
            //    placeholder 128x96 WU bounds one texel is 0.5 WU — a point is
            //    a quarter of the smallest body core's diameter (ObjectTable
            //    dust radius 1 WU), and the colour below is dimmer than every
            //    palette colour, so the starfield stays the weakest element of
            //    the §3.1 hierarchy by construction.
            glowSprite = makeGlowTexture(32, 0.5);
            accretionSpriteTex = makeAccretionTexture(64);
            starfieldSpriteTex = makeStarfieldTexture(256, 110);
            // The starfield: ONE sprite, farthest z, static by construction (placed
            // once on the first draw from the level's bounds, never touched again —
            // it never drifts and never carries readability, §3.3/§3.6).
            perfCountAlloc();
            starfieldObject = new UnityEngine.GameObject("eh-starfield");
            perfCountAlloc();
            starfieldRenderer = starfieldObject.AddComponent<UnityEngine.SpriteRenderer>();
            starfieldTransform = starfieldObject.GetComponent<UnityEngine.Transform>();
            starfieldRenderer.sprite = starfieldSpriteTex;
            // Dimmer than every palette colour (smallest body core is 84,200,255):
            // the texture alpha (<=0.30) multiplies on top, so this is doubly weak.
            perfCountAlloc();
            starfieldRenderer.color = new UnityEngine.Color(71.0f / 255.0f, 84.0f / 255.0f, 107.0f / 255.0f, 1.0f);
            starfieldObject.SetActive(true);
            // The accretion glow: behind the rim disc and the true-black sphere, so
            // only a rim-wrapped band and an outer halo show (§3.5). Colour is the
            // accretion rim palette already owned here (pack_px(140,236,255)) — one
            // palette, never a second invention; alpha lives in the texture.
            perfCountAlloc();
            accretionObject = new UnityEngine.GameObject("eh-accretion");
            perfCountAlloc();
            accretionRenderer = accretionObject.AddComponent<UnityEngine.SpriteRenderer>();
            accretionTransform = accretionObject.GetComponent<UnityEngine.Transform>();
            accretionRenderer.sprite = accretionSpriteTex;
            perfCountAlloc();
            accretionRenderer.color = new UnityEngine.Color(140.0f / 255.0f, 236.0f / 255.0f, 255.0f / 255.0f, 1.0f);
            accretionObject.SetActive(true);
            // The camera the §5.3 rule drives. Created here, once, so the class is
            // present in the build and the framing has an owner.
            perfCountAlloc();
            cameraObject = new UnityEngine.GameObject("eh-camera");
            perfCountAlloc();
            camera = cameraObject.AddComponent<UnityEngine.Camera>();
            cameraTransform = cameraObject.GetComponent<UnityEngine.Transform>();
            // Sprites live at z = 0 and the camera is placed at z = 0 by draw(),
            // so the orthographic depth range must straddle zero; the defaults
            // (near 0) clip the very plane everything is drawn on.
            camera.nearClipPlane = -1000.0f;
            camera.farClipPlane = 1000.0f;
            // Seventh extension probe (2026-10-16): the default clearFlags is Skybox,
            // and the hand-written scene carries no RenderSettings block and no skybox
            // material, so the skybox pass can clear to black and backgroundColor is
            // never used. Force a colour clear so the field colour IS the clear.
            // Settled by the engine's own API stubs (Data/Managed/UnityEngine/
            // UnityEngine.CoreModule.xml): the enum is UnityEngine.CameraClearFlags
            // with value SolidColor; Unity 6 has no SkyboxEnabled property at all,
            // and SolidColor alone suppresses the skybox pass.
            camera.clearFlags = UnityEngine.CameraClearFlags.SolidColor;
            // Field colour matches A-026's client (pack_px(8, 10, 18)); one palette
            // for both halves of the same §5.3 rule, never a second invention.
            // The compiler is the API oracle here: Camera.clearColor is rejected on
            // 6000.0.84f1; Camera.backgroundColor is the property that exists.
            perfCountAlloc();
            camera.backgroundColor = new UnityEngine.Color(8.0f / 255.0f, 10.0f / 255.0f, 18.0f / 255.0f, 1.0f);
            // Observability (user directive 2026-10-16): report the camera state
            // as the engine actually holds it, so a single logcat read can tell
            // whether the clear fix stuck. clearFlags prints the enum ordinal.
            UnityEngine.Debug.Log("EH-CAMERA created=1 clearFlags=" + (int)camera.clearFlags +
                " orthographic=" + camera.orthographic +
                " near=" + camera.nearClipPlane + " far=" + camera.farClipPlane);
            perfCountAlloc();
            visibilityLayer = new int[SimState.BODY_CAPACITY];
            // measurement surface: histogram and reused temporaries, allocated once
            perfCountAlloc();
            perfHist = new long[perfBuckets];
            perfCountAlloc();
            tmpPos = new UnityEngine.Vector3();
            perfCountAlloc();
            tmpScale = new UnityEngine.Vector3();
            perfCountAlloc();
            tmpColor = new UnityEngine.Color();
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
            // A-023 extension: the reused temporary, never a per-frame `new`. The
            // Unity property setter copies the components, so one Vector3 shared
            // across frames and objects is safe.
            tmpPos.x = (float)holeXWu;
            tmpPos.y = (float)holeYWu;
            // Measured 2026-10-16: with the camera AT z = 0 and the sprites at
            // z <= 0, the display-ON screencap showed the field colour inside
            // the horizon radius — the sprites sit at/behind the camera plane,
            // which is degenerate. The camera looks down +Z (identity rotation),
            // so it stands BACK at z = -10: every sprite (z in [-1, 0]) is then
            // unambiguously in front, and larger z remains nearer the camera,
            // keeping the body > disc > rim order the palette extension relies
            // on. The orthographic range still straddles the whole scene.
            tmpPos.z = -10.0f;
            cameraTransform.position = tmpPos;

            // ---- starfield: one sprite, farthest z, static by construction ------
            // Placed on the FIRST draw only (world bounds are sim state, read
            // read-only here), then never touched again: it never drifts (§3.6)
            // and never carries readability (§3.3). Scaled to the world bounds,
            // centred at (boundsW/2, boundsH/2) — the level's bounds convention
            // (LevelLoader starts the hole at the centre of that rectangle).
            if (!starfieldPlaced)
            {
                double boundsWu = (double)(long)s.boundsW / 4294967296.0;
                if (boundsH > 0.0 && boundsWu > 0.0)
                {
                    tmpPos.x = (float)(boundsWu * 0.5);
                    tmpPos.y = (float)(boundsH * 0.5);
                    tmpPos.z = -2.0f; // farthest layer, inside the measured [-2,0] range
                    starfieldTransform.position = tmpPos;
                    tmpScale.x = (float)boundsWu;
                    tmpScale.y = (float)boundsH;
                    tmpScale.z = 1.0f;
                    starfieldTransform.localScale = tmpScale;
                    starfieldPlaced = true;
                }
            }

            // ---- accretion glow: behind the rim disc and the black sphere -------
            // §3.5: the glow reads as gas around the void; the sphere and the rim
            // disc (z -1 and -0.5) cover its inner band, so only a rim-wrapped
            // band and an outer halo show. §3.6: it rotates slowly — decoration
            // only, driven by the ONE constant accretionDegPerS above; the
            // reduced-motion option is A-012's, not implemented here.
            accretionRenderer.sprite = accretionSpriteTex;
            tmpPos.x = (float)holeXWu;
            tmpPos.y = (float)holeYWu;
            tmpPos.z = -1.5f; // behind the rim disc (-1) and the sphere (-0.5)
            accretionTransform.position = tmpPos;
            // 1.6x the horizon radius: the texture's band peaks at r=0.62 of the
            // sprite, i.e. right at the sphere's own edge, and fades to nothing at
            // 1.6x — a rim-wrapped band plus a soft outer halo, never a hard disc.
            double accWu = 3.2 * evWu; // sprite diameter in WU
            tmpScale.x = (float)accWu;
            tmpScale.y = (float)accWu;
            tmpScale.z = 1.0f;
            accretionTransform.localScale = tmpScale;
            // The spin: accumulate degrees from the frame clock (deltaTime is the
            // engine's per-frame seconds; a render-side double, never sim state),
            // wrap, then set the rotation through a reused Quaternion — a value
            // type, so this allocates nothing per frame.
            accretionAngleDeg = accretionAngleDeg + accretionDegPerS * (double)UnityEngine.Time.deltaTime;
            if (accretionAngleDeg >= 360.0)
            {
                accretionAngleDeg = accretionAngleDeg - 360.0;
            }
            tmpQuat = UnityEngine.Quaternion.Euler(0.0f, 0.0f, (float)accretionAngleDeg);
            accretionTransform.rotation = tmpQuat;

            // ---- horizon ring: the VISIBLE hole is the event horizon (A-026's
            // render fix established this: drawing holeRadius, the constant
            // collision core, made the hole look frozen) --------------------------
            horizonRimObject.SetActive(true);
            horizonRimSprite.sprite = defaultSprite;
            // cached at boot; the setter copies at assignment, so write first, assign after
            tmpPos.x = (float)holeXWu;
            tmpPos.y = (float)holeYWu;
            tmpPos.z = -1.0f;
            rimTransform.position = tmpPos;
            double dWu = 2.0 * evWu;
            tmpScale.x = (float)dWu;
            tmpScale.y = (float)dWu;
            tmpScale.z = 1.0f;
            rimTransform.localScale = tmpScale;
            // The black disc sits 2 px inside the rim (view.c: ring at hr, inner
            // ring at hr - 2), so the rim reads as a ring, not a filled disc.
            double insetWu = 4.0 / ppu;
            double coreWu = dWu > insetWu * 2.0 ? dWu - insetWu * 2.0 : dWu;
            horizonObject.SetActive(true);
            horizonSprite.sprite = defaultSprite;
            tmpPos.x = (float)holeXWu;
            tmpPos.y = (float)holeYWu;
            tmpPos.z = -0.5f;
            horizonTransform.position = tmpPos;
            tmpScale.x = (float)coreWu;
            tmpScale.y = (float)coreWu;
            tmpScale.z = 1.0f;
            horizonTransform.localScale = tmpScale;

            // ---- bodies: ascending index, culled against the active window ------
            double halfWWu = (windowW / ppu) * 0.5;
            double halfHWu = (windowH / ppu) * 0.5;
            visibilityCount = 0;
            for (int body = 0; body < SimState.BODY_CAPACITY; body++)
            {
                if ((s.bodyFlags[body] & 0x01) == 0)
                {
                    bodyObjects[body].SetActive(false); // inactive records are never drawn as ghosts
                    bodyGlowObjects[body].SetActive(false);
                    continue;
                }
                double dxWu = ((double)s.bodyX[body] / 4294967296.0) - holeXWu;
                double dyWu = ((double)s.bodyY[body] / 4294967296.0) - holeYWu;
                double rWu = (double)s.bodyRadius[body] / 4294967296.0;
                if (dxWu > halfWWu + rWu || dxWu < -(halfWWu + rWu) ||
                    dyWu > halfHWu + rWu || dyWu < -(halfHWu + rWu))
                {
                    bodyObjects[body].SetActive(false);
                    bodyGlowObjects[body].SetActive(false); // the glow is culled with its core
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
                UnityEngine.Transform t = bodyTransform[body]; // cached at boot
                tmpPos.x = (float)(holeXWu + dxWu);
                tmpPos.y = (float)(holeYWu + dyWu);
                tmpPos.z = 0.0f;
                t.position = tmpPos;
                tmpScale.x = (float)(2.0 * rWu);
                tmpScale.y = (float)(2.0 * rWu);
                tmpScale.z = 1.0f;
                t.localScale = tmpScale;
                // A-026's body palette, mirrored exactly: tier from the frame's
                // kind bits ((kind >> 2) & 3), and absorbable bodies read
                // brighter, so the two states are separable without colour alone.
                // Written into the reused Color; the setter copies at assignment.
                bodyColorInto(s.bodyFlags[body]);
                bodySprites[body].color = tmpColor;
                // A-023 tenth extension: the star's halo — one larger, lower-alpha
                // soft sprite BEHIND the core (§3.5 vocabulary; the core keeps the
                // exact sim radius, so the silhouette and the identity channels of
                // §3.2 stay intact; hazards would use the same palette family and
                // the glow sits behind the core, per the design's hazard row).
                // §3.2 ratios: halo 2.5x the core radius, capped at 1/8 of the
                // horizon radius so glows never merge into a field that hides the
                // ring; if the cap falls below the core the glow hides behind it.
                bodyGlowObjects[body].SetActive(true);
                bodyGlowSprites[body].sprite = glowSprite;
                double haloR = 2.5 * rWu;
                double haloCapWu = evWu * 0.125;
                if (haloR > haloCapWu)
                {
                    haloR = haloCapWu;
                }
                if (haloR < rWu)
                {
                    haloR = rWu;
                }
                UnityEngine.Transform gt = bodyGlowTransform[body]; // cached at boot
                tmpPos.x = (float)(holeXWu + dxWu);
                tmpPos.y = (float)(holeYWu + dyWu);
                tmpPos.z = -0.25f; // behind the core (0), in front of the sphere (-0.5)
                gt.position = tmpPos;
                tmpScale.x = (float)(2.0 * haloR);
                tmpScale.y = (float)(2.0 * haloR);
                tmpScale.z = 1.0f;
                gt.localScale = tmpScale;
                // Same palette as the core (bodyColorInto already wrote tmpColor);
                // the softness is the texture's alpha, never a second colour.
                bodyGlowSprites[body].color = tmpColor;
            }

            // hazard pool: empty on p1-level-01 (A-009); the slot is documented,
            // not faked — HazardSystem is a stub and there is nothing to draw.

            // Observability (user directive 2026-10-16): record what this draw
            // presented; the EH-DRAW summary line reports it. No allocation.
            lastDrawFrame = UnityEngine.Time.frameCount;
            lastCamX = holeXWu; lastCamY = holeYWu; lastPPU = ppu;
            lastHorizonPx = (int)(evWu * ppu + 0.5);
            lastVisCount = visibilityCount;
        }

        /// <summary>
        /// A-026's body palette (tools/player/view.c, verbatim): tier bits select
        /// the base colour, absorbable bodies read brighter. Colour is a render
        /// property only - the sim never sees it, and the digest never changes.
        /// A-023 extension: writes into the reused tmpColor — the old form returned
        /// `new Color` per body per frame, which is exactly what P-05 targets.
        /// </summary>
        private static void bodyColorInto(int flags)
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
            tmpColor.r = r / 255.0f;
            tmpColor.g = g / 255.0f;
            tmpColor.b = b / 255.0f;
            tmpColor.a = 1.0f;
        }

        // ---- A-023 tenth extension: the boot-generated soft-alpha bases ---------
        // All three run once at boot (the package ships no asset files; the built-in
        // lookup is null on it). Alpha lives in the texture; colour lives in
        // SpriteRenderer.color — the §3.5 glow rule, no shaders, no materials, no
        // additive blending. SetPixels(Color[]) and Sprite.Create(Texture2D, Rect,
        // Vector2) are the settled names the ninth extension compiled.

        /// <summary>
        /// Body halo: a radial gradient from peakAlpha at the centre to fully
        /// transparent at the edge. Drawn behind a core sprite, it reads as the
        /// soft halo of a glowing star (§3.2 bodies-as-stars).
        /// </summary>
        private static UnityEngine.Sprite makeGlowTexture(int size, double peakAlpha)
        {
            UnityEngine.Texture2D tex = new UnityEngine.Texture2D(size, size);
            perfCountAlloc();
            UnityEngine.Color[] px = new UnityEngine.Color[size * size];
            perfCountAlloc();
            UnityEngine.Color clear = new UnityEngine.Color(0.0f, 0.0f, 0.0f, 0.0f);
            double half = (double)size * 0.5;
            for (int y = 0; y < size; y = y + 1)
            {
                for (int x = 0; x < size; x = x + 1)
                {
                    double dx = (x + 0.5 - half) / half;
                    double dy = (y + 0.5 - half) / half;
                    double r2 = dx * dx + dy * dy;
                    if (r2 >= 1.0)
                    {
                        px[y * size + x] = clear;
                    }
                    else
                    {
                        double falloff = 1.0 - (double)UnityEngine.Mathf.Sqrt((float)r2);
                        double a = peakAlpha * falloff * falloff;
                        px[y * size + x] = new UnityEngine.Color(1.0f, 1.0f, 1.0f, (float)a);
                    }
                }
            }
            tex.SetPixels(px);
            return UnityEngine.Sprite.Create(tex, new UnityEngine.Rect(0, 0, size, size), new UnityEngine.Vector2((float)half, (float)half));
        }

        /// <summary>
        /// Accretion disc: a soft band that rises to its peak at r=0.62 (the
        /// sphere's own edge once the sprite is scaled to 1.6x the horizon radius)
        /// and fades to nothing at r=1.0, modulated around the angle by a slow
        /// cos(3*theta + 6r) swirl so the rotation reads as glowing gas, as in
        /// the film (§3.2/§3.3 channel 4 — decoration only, never readability).
        /// </summary>
        private static UnityEngine.Sprite makeAccretionTexture(int size)
        {
            UnityEngine.Texture2D tex = new UnityEngine.Texture2D(size, size);
            perfCountAlloc();
            UnityEngine.Color[] px = new UnityEngine.Color[size * size];
            perfCountAlloc();
            UnityEngine.Color clear = new UnityEngine.Color(0.0f, 0.0f, 0.0f, 0.0f);
            double half = (double)size * 0.5;
            for (int y = 0; y < size; y = y + 1)
            {
                for (int x = 0; x < size; x = x + 1)
                {
                    double dx = (x + 0.5 - half) / half;
                    double dy = (y + 0.5 - half) / half;
                    double r2 = dx * dx + dy * dy;
                    if (r2 >= 1.0)
                    {
                        px[y * size + x] = clear;
                    }
                    else
                    {
                        double r = (double)UnityEngine.Mathf.Sqrt((float)r2);
                        double theta = (double)UnityEngine.Mathf.Atan2((float)dy, (float)dx);
                        double band;
                        if (r < 0.62)
                        {
                            double t = r / 0.62;
                            band = t * t; // rises toward the rim; the sphere covers it
                        }
                        else
                        {
                            double t = (r - 0.62) / 0.38;
                            band = 1.0 - t; // the visible outer halo, fading to nothing
                        }
                        double swirl = 0.72 + 0.28 * (double)UnityEngine.Mathf.Cos((float)(3.0 * theta + 6.0 * r));
                        double a = 0.55 * band * swirl;
                        px[y * size + x] = new UnityEngine.Color(1.0f, 1.0f, 1.0f, (float)a);
                    }
                }
            }
            tex.SetPixels(px);
            return UnityEngine.Sprite.Create(tex, new UnityEngine.Rect(0, 0, size, size), new UnityEngine.Vector2((float)half, (float)half));
        }

        /// <summary>
        /// Starfield: sparse one-pixel points from a deterministic xorshift (the
        /// same field every run — it is decoration, not a per-run random), alpha
        /// 0.16..0.30. One texture, one draw call, static by construction (§3.3/
        /// §3.6): it never drifts and stays the weakest element of the §3.1
        /// hierarchy (points are a quarter of the smallest body core's diameter at
        /// the placeholder bounds, and the colour multiplier is below every
        /// palette colour).
        /// </summary>
        private static UnityEngine.Sprite makeStarfieldTexture(int size, int points)
        {
            UnityEngine.Texture2D tex = new UnityEngine.Texture2D(size, size);
            perfCountAlloc();
            UnityEngine.Color[] px = new UnityEngine.Color[size * size];
            perfCountAlloc();
            UnityEngine.Color clear = new UnityEngine.Color(0.0f, 0.0f, 0.0f, 0.0f);
            for (int i = 0; i < size * size; i = i + 1)
            {
                px[i] = clear;
            }
            ulong st = 0x9E3779B97F4A7C15UL;
            for (int p = 0; p < points; p = p + 1)
            {
                st = st ^ (st << 13); st = st ^ (st >> 7); st = st ^ (st << 17);
                int x = (int)(st % (ulong)size);
                st = st ^ (st << 13); st = st ^ (st >> 7); st = st ^ (st << 17);
                int y = (int)(st % (ulong)size);
                st = st ^ (st << 13); st = st ^ (st >> 7); st = st ^ (st << 17);
                double a = 0.16 + 0.14 * (double)(int)(st & 0xFFUL) / 255.0;
                px[y * size + x] = new UnityEngine.Color(1.0f, 1.0f, 1.0f, (float)a);
            }
            tex.SetPixels(px);
            return UnityEngine.Sprite.Create(tex, new UnityEngine.Rect(0, 0, size, size), new UnityEngine.Vector2((float)((double)size * 0.5), (float)((double)size * 0.5)));
        }

        // ---- the measurement surface (A-023 extension: P-01 / P-05) --------------
        /// <summary>
        /// Open one player frame: sample the real-time clock, feed the previous
        /// frame's delta into the fixed histogram, keep the worst hitch, and
        /// snapshot the allocation counter. Called by GameLoop.Update at the frame
        /// boundary. It may log, it may not gate: no Fail.check on any measured
        /// value, and nothing here writes sim state.
        ///
        /// Clock, settled by the compiler on 6000.0.84f1 (the brief's probe rule):
        /// UnityEngine.Time.realtimeSinceStartup COMPILES (the temporary ApiProbe
        /// compiled it with no error while rejecting realtimeDeltaTIme and the
        /// validation line thisNameDoesNotExistAtAll, so the error list was real).
        /// It is a float seconds clock; widening to double and converting to whole
        /// microseconds bounds the rounding at ~15 us at t=130 s, far below the
        /// 0.25 ms bucket. The OS-side logcat fallback of the brief is therefore
        /// not needed; method=realtime names the source in every line.
        /// </summary>
        public static void perfBeginFrame()
        {
            double now = UnityEngine.Time.realtimeSinceStartup;
            if (perfClockPrev < 0.0)
            {
                perfClockPrev = now;
                perfFirstSample = now;
                perfNextSummary = now + perfWarmupS + perfSummaryS;
                perfSnapshotAllocs(true); // boot's allocations land in the warm-up max
                // A-023 extension: the first-frame marker — proves Update() runs
                // at all, which until now nothing in the log showed.
                UnityEngine.Debug.Log("EH-FRAME first frame seen by the measurement surface");
                return;
            }
            long deltaUs = (long)((now - perfClockPrev) * 1000000.0 + 0.5);
            perfClockPrev = now;
            if (now - perfFirstSample < perfWarmupS)
            {
                perfSnapshotAllocs(true); // warm-up: counted, never reported as steady state
                return;
            }
            int bucket = (int)(deltaUs / (long)perfBucketUs);
            if (bucket >= perfBuckets)
            {
                bucket = perfBuckets - 1;
            }
            perfHist[bucket] += 1L;
            if (deltaUs > perfWorstUs)
            {
                perfWorstUs = deltaUs;
            }
            perfFrames += 1L;
            perfSnapshotAllocs(false);
            if (now >= perfNextSummary)
            {
                emitPerfSummary(now);
                perfSummaryPending = true; // the reporting frame allocates for its own line
                perfNextSummary += perfSummaryS;
            }
        }

        /// <summary>
        /// Count one allocation site in the player's own per-frame code. draw()
        /// has no sites left (reused temporaries); boot() calls this once per
        /// pool allocation so the warm-up maximum proves the counter is wired.
        /// Scope, honestly: the Unity shell (GameLoop + RenderLayer). Shared
        /// bridge code (e.g. InputAdapter's per-step Intent) is outside this
        /// counter and outside the brief's contract item 2.
        /// </summary>
        public static void perfCountAlloc()
        {
            perfAllocsThisFrame += 1L;
        }

        private static void perfSnapshotAllocs(bool warmup)
        {
            if (warmup)
            {
                if (perfAllocsThisFrame > perfWarmupAllocMax)
                {
                    perfWarmupAllocMax = perfAllocsThisFrame;
                }
            }
            else if (perfSummaryPending)
            {
                perfSummaryPending = false; // exclude the summary frame from the steady-state max
            }
            else
            {
                if (perfAllocsThisFrame > perfAllocMax)
                {
                    perfAllocMax = perfAllocsThisFrame;
                }
            }
            perfAllocsThisFrame = 0L;
        }

        /// <summary>
        /// Percentile (num/den) of the measured frame deltas, in whole
        /// microseconds, from the bucket counts; the bucket centre is reported.
        /// </summary>
        private static long perfPercentile(long num, long den)
        {
            if (perfFrames <= 0L)
            {
                return 0L;
            }
            long rank = (perfFrames * num + den - 1L) / den; // ceil(n * p)
            long seen = 0L;
            for (int b = 0; b < perfBuckets; b++)
            {
                seen += perfHist[b];
                if (seen >= rank)
                {
                    return (long)b * (long)perfBucketUs + (long)perfBucketUs / 2L;
                }
            }
            return (long)(perfBuckets - 1) * (long)perfBucketUs + (long)perfBucketUs / 2L;
        }

        /// <summary>
        /// Whole microseconds to "NNN.NNN" milliseconds, integer arithmetic only
        /// (the printed numbers must not depend on float formatting).
        /// </summary>
        private static string perfMs(long us)
        {
            long w = us / 1000L;
            long f = us - w * 1000L;
            return w + "." + (f < 10L ? "00" : (f < 100L ? "0" : "")) + f;
        }

        /// <summary>
        /// The summary line, cumulative since the end of warm-up, emitted every
        /// 5 s of measured time through UnityEngine.Debug.Log so logcat on the
        /// device can read it. One line, space-separated key=value pairs:
        /// EH-PERF method=realtime frame=NN t=NNNNNN p50_ms=NN.NNN p95_ms=NN.NNN
        /// p999_ms=NN.NNN worst_hitch_ms=NNN.NNN frames=NNNN allocs_warmup_max=NN
        /// allocs_per_frame_max=NN
        /// t= is the realtime sample in whole microseconds, so the capture can
        /// cross-check against logcat timestamps; frame= carries frameCount.
        /// </summary>
        private static void emitPerfSummary(double now)
        {
            long tUs = (long)(now * 1000000.0 + 0.5);
            UnityEngine.Debug.Log(
                "EH-PERF method=realtime frame=" + UnityEngine.Time.frameCount +
                " t=" + tUs +
                " p50_ms=" + perfMs(perfPercentile(50L, 100L)) +
                " p95_ms=" + perfMs(perfPercentile(95L, 100L)) +
                " p999_ms=" + perfMs(perfPercentile(999L, 1000L)) +
                " worst_hitch_ms=" + perfMs(perfWorstUs) +
                " frames=" + perfFrames +
                " allocs_warmup_max=" + perfWarmupAllocMax +
                " allocs_per_frame_max=" + perfAllocMax);
            // Observability (user directive 2026-10-16): the draw side of the
            // same cadence. If EH-DRAW reports visible>=2 and horizonPx>0 while
            // the screen is still black, the fault is presentation, not logic.
            UnityEngine.Debug.Log("EH-DRAW frame=" + lastDrawFrame +
                " cam=" + lastCamX + "," + lastCamY +
                " ppu=" + lastPPU + " horizonPx=" + lastHorizonPx +
                " visible=" + lastVisCount +
                " clearFlags=" + (int)camera.clearFlags +
                " orthoSize=" + camera.orthographicSize);
        }

        /// <summary>
        /// Named absences so later artifacts find the slot instead of inventing a
        /// parallel one: particles and the edge-indicator DRAWING are A-010 owned
        /// (brief §9 scope boundaries). The A-023 tenth extension has now filled
        /// the horizon-ring art slot (the Interstellar presentation of A-006 is
        /// implemented above); what remains absent here is particles, the edge
        /// indicators, and A-012's reduced-motion option (which flips the ONE
        /// constant accretionDegPerS, not code here).
        /// </summary>
        public static void drawEffectsAbsent(SimState s)
        {
            // no-op: phase 1 ships no particles and no zoom curve.
        }
    }
}

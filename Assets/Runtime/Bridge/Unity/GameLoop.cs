// A-023 Unity player package — Assets/Runtime/Bridge/Unity/GameLoop.cs
// Upgraded from the A-021/A-022 contract entry to the player-facing shell: the
// Unity MonoBehaviour that boots the SAME SimCore through the SAME frame driver,
// derives the frame clock from DEVICE TELEMETRY (A-019 §12 item 3 and FrameDriver
// selectTier's note "Device-derived selection belongs to A-023"), and keeps the
// dev.* knobs off the shipping path (A-020 §7 STEP-11d is what checks this on the
// built package).
//
// *** NOT COMPILED IN THE HEADLESS BUILD (excluded from harness.csproj and
// player.csproj by their Exclude clauses) *** Uses the Unity runtime API.
// Authored against the Unity 6 LTS 6000.0.84f1 API; NOT compiled or run here —
// the license gate (PLAN.md §4 A-023: the editor exits 198, "No licenses were
// found") blocks any Unity-side compile until sign-in. If the editor build
// rejects a call when the gate opens, that is the drift to fix at build time;
// nothing here may pass quietly.
//
// A-019 §11 hard rule honoured here: "there is exactly one simulation loop and
// both entries reach it". This file does NOT re-implement stepping, owns no
// accumulator of its own, and never calls SimStep directly. The accumulator,
// DT = 1/60 (71582788L, unchanged — PLAN.md §4 A-023 acceptance), MAX_CATCHUP =
// 4 and the dropped-step rule live in FixedStepDriver.advance, which the
// harness reaches the same way through FrameDriver.frameLive.
//
// Pure-sim rule (A-019 §5/§6): nothing in this file feeds float/double into
// SimCore. The only float here is the render-only alpha handed to RenderLayer.
//
// Time scale is the weak-device fallback (A-014 P-02), never DT: the 30 fps
// path is a frame-clock targetFps choice — one frame's wall delta is exactly
// two DTs of accumulator debt and the SAME accumulator catches up within
// MAX_CATCHUP or drops steps. No second simulation path exists or is built.
//
// A-023 ELEVENTH EXTENSION (2026-10-17, defect C of the four the user SAW on
// the S24 FE — brief docs/briefs/A-023-visual-input.md; PLAN.md updated FIRST
// at 283af76 docs(plan):). TOUCH DOES NOTHING: pushLiveIntent was a pause()
// stub — the candidate build NEVER read a finger. A-011's approved default
// scheme (docs/input.md §2): touch + drag TRANSLATES the hole; the intent
// channel is direction + rate. Wire it honestly and MINIMALLY here (the
// platform side): read Unity's live touch each frame, keep a drag anchor in
// instance fields (first contact sets anchor = finger position in screen px;
// while a contact is held the drag vector = current − anchor; no contact
// clears the anchor and calls InputAdapter.pause() — the existing coast rule),
// and feed the transport contract ONLY: InputAdapter.pushIntent(thrustX,
// thrustY, rate) with Q32.32 int64 values (FixedQ.ONE = 1.0): direction =
// normalized drag vector, rate = min(dragLength / DRAG_FULL_PX, 1). DRAG_FULL_PX
// is a named constant (120 px to full rate) — a PLACEHOLDER for A-011's
// tuning, not a re-decided scheme. first-contact-wins and pause-cancels stay
// in InputAdapter (untouched); one pushIntent per frame is fine — the adapter
// locks the first per step and discards the rest. This is the MINIMAL drag
// wiring: A-011's full scheme (offset-preserving pickup, dead zone, edge band,
// re-anchor) is a later refinement; the transport contract and the sim are
// untouched.
//
// Settled by the compiler on 6000.0.84f1 (the ApiProbe pattern: a temporary
// probe under Assets/Runtime/Bridge/Unity, built, read, deleted; the
// validation line thisNameDoesNotExistAtAll appeared in the error list as
// CS0103, so the list was real evidence). ACCEPTED: UnityEngine.Input.touchCount
// (no error), UnityEngine.Input.GetTouch(int) returning the value-type
// UnityEngine.Touch (no error — no per-frame allocation), and the position
// member UnityEngine.Touch.position (Vector2; the read compiled). REJECTED:
// UnityEngine.Touch.fingerPos (CS1061 — Unity 6 renamed the member to
// position), UnityEngine.Input.GetTouches() (CS0117 — no such method), and
// UnityEngine.Input.touches EXISTS but as a Touch[] array (CS0029 against an
// int read; the count read is touchCount). The Input System package is NOT
// installed and was not reached for.
//
// D. THE FREEZE — INVESTIGATED, NOT PAPERED OVER (see the Update comment):
// no try/catch, no softening anywhere for it.

namespace EH
{
    // Unity MonoBehaviour lifecycle methods are instance members, so this is the
    // one non-static class in Bridge/Unity; the sim side never instantiates it.
    // Defect found by RUNNING on a real device 2026-10-11: without the
    // UnityEngine.MonoBehaviour base, Unity never calls Start()/Update() - the
    // player booted, the engine ran an empty scene loop, and our code never
    // executed (zero EH-BOOT markers in the device log). The base is what makes
    // the scene's component reference a live lifecycle owner.
    public class GameLoop : UnityEngine.MonoBehaviour
    {
        private SimState sim;
        private bool started;
        // A-022: the per-run frame clock. GameLoop owns a clock rather than
        // calling FixedStepDriver.advance itself, because the frame driver is
        // the single owner of frame -> step mapping, hitch accounting, and the
        // render-only alpha; the harness reaches the very same code.
        private FrameDriver.FrameClock clock;

        // A-023 eleventh extension (defect C): the drag anchor — platform-side
        // state ONLY. Two floats and a flag in instance fields: no per-frame
        // `new`, no allocation (the Unity Touch read returns a value-type
        // struct; the hard rule "steady-state frames allocate ZERO" holds).
        private bool dragContact;
        private float dragAnchorX, dragAnchorY; // screen px, set on first contact
        // DRAG_FULL_PX: px of drag to reach full rate. A named constant and a
        // PLACEHOLDER for A-011's tuning — not a re-decided scheme; the
        // transport contract and the sim are untouched.
        private static readonly long dragFullPx = 120L;

        /// <summary>
        /// Unity entry (A-019 §11). Builds the SAME SimState the harness builds
        /// — LevelLoader.load of the single LevelTable entry — and starts the
        /// single shared fixed-step driver. The shipping platform line is iOS
        /// and Android (A-014 device classes; PLAN.md §5.3's platform line), so
        /// the build target is "mobile", never "desktop".
        /// </summary>
        public void Start()
        {
            // levelId and sessionSeed are a platform concern; phase 1 has exactly
            // one level and the LevelTable default seed, identical to the harness
            // invocation in A-019 §11 so both entries load a byte-identical state.
            sim = LevelLoader.load("p1-level-01", LevelTable.defaultSeed);
            PlatformBridge.setBuildTarget("mobile");

            // A-023: the device tier probe — device-derived selection belongs
            // here (FrameDriver.selectTier stays honest at 0 for the headless
            // build; this is the Unity side with real telemetry). The classes
            // are A-014's definitions: weak device (<= 2 GB RAM) runs the 30 fps
            // fallback; a modern phone (>= 8 GB) runs 60. The concrete
            // reference-device set is NOT picked here (PLAN.md §4 A-023: blocked
            // on hardware, recorded for A-020/A-025).
            int tier = probeDeviceTier();
            int targetFps = tier == 1 ? 30 : 60;
            // A-023 extension (device capture): an observable boot marker — the
            // engine's "Starting Game Loop" line is GameActivity's own message,
            // NOT ours, so until now nothing in the log proved OUR Start() ran.
            UnityEngine.Debug.Log("EH-BOOT level=p1-level-01 targetFps=" + targetFps + " tier=" + tier);
            // P-01 pacing control (2026-10-16): the engine's own frame limiter
            // was never set, so its default applied and the device read 30 FPS.
            // Settled by the engine's own stubs (UnityEngine.CoreModule.xml):
            // Unity 6 REMOVED Time.SetTargetFPS; the runtime control is
            // UnityEngine.Application.targetFrameRate. Mirror the derived target
            // onto the engine so the limiter is ours, not the platform default.
            UnityEngine.Application.targetFrameRate = targetFps;
            // Observability: report the limiter AND the engine's own view of the
            // panel refresh rate, so the pacing cause is arithmetic on the log
            // (interval = vsyncsPerSecond / target), never speculation. FrameTimingManager
            // is gated by PlayerSettings.enableFrameTimingStats, now ON.
            string pace = "EH-PACE target=" + UnityEngine.Application.targetFrameRate;
            if (UnityEngine.FrameTimingManager.IsFeatureEnabled())
            {
                pace += " vsyncsPerSecond=" + UnityEngine.FrameTimingManager.GetVSyncsPerSecond();
            }
            UnityEngine.Debug.Log(pace);

            // The dev.* knobs (PlatformBridge.devTargetFps/devTimeScale) are the
            // headless development surface ONLY: this boot path derives the
            // clock from telemetry, never from a dev.* key, so no input handler
            // or settings entry in the built package reads one (STEP-11d). One
            // accumulator, one advance — no second simulation path.
            clock = FrameDriver.boot(targetFps, FixedQ.ONE, 0);
            if (tier != 0)
            {
                FrameDriver.setTier(clock, tier); // render state only (A-019 §10 item 2)
            }
            InputAdapter.boot(64);
            RenderLayer.boot();
            UIRoot.register();
            started = true;
        }

        /// <summary>
        /// Device tier from Unity telemetry. 0 = unknown (never a guessed
        /// default — the same honesty FrameDriver.selectTier keeps headless).
        ///
        /// Evidence, not assumption (6000.0.84f1, the compiler as the oracle):
        /// Unity 6's C# API exposes NO physical-RAM member. Every candidate name
        /// is rejected — SystemInfo.GetPhysicalMemoryMB, GetTotalPhysicalMB,
        /// GetAvailableMB, totalPhysicalMB, physicalMemoryMB, GetGraphicsMemorySize,
        /// GetProcessorFrequencyMHz. The C++ side has systeminfo::GetPhysicalMemoryMB,
        /// but it is not scriptable. So A-014's RAM-class thresholds (>= 8 GB modern,
        /// <= 2 GB weak) are applied by the platform and the hardware, NOT by player
        /// code; inventing a CPU-count rule here would be a new design decision, so
        /// the tier stays unknown and the open question is recorded for A-025.
        /// </summary>
        private int probeDeviceTier()
        {
            return 0; // unknown: no scriptable RAM signal on this engine version
        }

        // A-023 eleventh extension — D. THE FREEZE: OPEN INVESTIGATION, NOT
        // PAPERED OVER. Measured on the S24 FE: the log shows EH-BOOT/EH-PACE/
        // EH-SPRITE/EH-CAMERA/EH-FRAME and then ZERO EH-PERF lines — Update
        // ran once and stopped; an earlier run left an il2cpp dropbox report
        // (native crash) while the user's process was alive but suspended.
        // The cause is NOT proven (the DOZE display-surface stop remains a
        // competing environment explanation; PROGRESS.md records both). NO
        // try/catch, no softening, no guess: the A/B/C fixes touch exactly the
        // code that runs in the FIRST draw() (the generated-texture formats,
        // the both-axis fit, the first starfield placement) and C adds the
        // first finger read in pushLiveIntent. The NEXT device capture decides:
        // EH-PERF lines appearing means the freeze was in the code being fixed;
        // if the code still faults, Unity's own log will name the fault.
        /// <summary>
        /// One Unity frame. The intent is sampled ONCE per step boundary through
        /// InputAdapter (transport only — it holds no input scheme of its own,
        /// A-019 §4), then the whole frame budget goes to the ONE driver via
        /// FrameDriver.frameLive, which is the same frame body the harness runs
        /// (target-fps wall budget, hitch debt, then FixedStepDriver.advance).
        /// advance() runs at most MAX_CATCHUP steps and drops further debt into
        /// s.droppedSteps; GameLoop never interpolates, cushions, or softens, and
        /// owns no accumulator of its own.
        ///
        /// A-022 boundary: frameLive is the LIVE path, so one already-resolved
        /// intent covers the whole frame. At 30 fps a frame delivers two steps
        /// and both share it; the per-frame to per-step mapping for live input is
        /// A-011's decision, not this file's. The deterministic path (re-simulable
        /// playback, H-01/H-02/H-06/H-07/H-09) uses FrameDriver.frame with per-step
        /// intents from the EIDIG1 channel.
        /// </summary>
        public void Update()
        {
            if (!started)
            {
                return;
            }

            // A-011 owns the pointer/touch/tilt mapping; this file forwards the
            // platform telemetry layer's already-resolved values into the
            // transport (first-contact-wins and pause-cancels-intent live in
            // InputAdapter, not here). The candidate build now wires the
            // built-in mobile touch drag (A-023 eleventh extension, defect C);
            // the scheme itself is not re-decided.
            // A-023 extension: the measurement surface opens the frame (P-01/P-05
            // QA instrument — samples the real-time clock at the frame boundary,
            // feeds the previous frame's delta into the fixed histogram, snapshots
            // the allocation counter; it may log, it may not gate, and it writes
            // nothing the player can perceive except log lines).
            RenderLayer.perfBeginFrame();

            pushLiveIntent();
            Intent i = InputAdapter.sampleIntent(sim.stepIndex);
            InputAdapter.endStep(); // opens the next step's first-contact window

            FrameDriver.frameLive(clock, sim, i);

            // render-only interpolation between the last two fixed steps; never
            // fed back into the sim (A-019 §4 RenderLayer row: alpha is render
            // only). The frame driver already computed accumulator/DT as Q32.32
            // raw; the float conversion happens here, on the render side, so no
            // float ever reaches SimCore.
            float alpha = (float)clock.renderAlphaRaw / (float)FixedQ.ONE;
            RenderLayer.draw(sim, alpha);
        }

        /// <summary>
        /// A-023 eleventh extension: the MINIMAL drag wiring (defect C — the
        /// user touched the screen and nothing moved because this slot was a
        /// pause() stub that NEVER read a finger). A-011's approved default
        /// scheme (docs/input.md §2): touch + drag TRANSLATES the hole; the
        /// intent channel is direction + rate. This is the MINIMAL honest
        /// wiring, not the full scheme: first contact anchors at the finger
        /// position (screen px); while a contact is held the drag vector =
        /// current − anchor; no contact clears the anchor and calls
        /// InputAdapter.pause() (the existing coast/neutral rule). A-011's
        /// full scheme (offset-preserving pickup, dead zone, edge band,
        /// re-anchor) is a LATER refinement; first-contact-wins and pause-
        /// cancels stay in InputAdapter (untouched); one pushIntent per frame
        /// is fine — the adapter locks the first per step and discards the
        /// rest. The transport contract and the sim are untouched; floats
        /// appear ONLY on this platform side and are converted to Q32.32
        /// int64 before the transport call.
        /// </summary>
        private void pushLiveIntent()
        {
            // Settled by the compiler on 6000.0.84f1 (the ApiProbe pattern:
            // temporary probe built, read, deleted; the validation line
            // appeared in the error list, so the list was real evidence).
            // ACCEPTED: UnityEngine.Input.touchCount; UnityEngine.Input.GetTouch(
            // int) returning the value-type UnityEngine.Touch (no per-frame
            // allocation); UnityEngine.Touch.position (Vector2). REJECTED:
            // Touch.fingerPos (CS1061 — Unity 6 names it position),
            // Input.GetTouches() (CS0117), Input.touches EXISTS but as a
            // Touch[] array (CS0029 against an int read — the count read is
            // touchCount). The Input System package is NOT installed.
            int contacts = UnityEngine.Input.touchCount;
            if (contacts > 0)
            {
                UnityEngine.Touch touch = UnityEngine.Input.GetTouch(0); // the minimal wiring reads one finger; multi-touch is A-011's later
                float x = touch.position.x;
                float y = touch.position.y;
                if (!dragContact)
                {
                    // first contact: anchor = current finger position; no drag
                    // vector exists yet, so the intent is a zero (the adapter
                    // records coast/zero-rate for a zero vector). A contact
                    // frame must NOT fall through to pause().
                    dragContact = true;
                    dragAnchorX = x;
                    dragAnchorY = y;
                    InputAdapter.pushIntent(0L, 0L, 0L);
                    return;
                }
                float dx = x - dragAnchorX;
                float dy = y - dragAnchorY;
                // floats are allowed HERE — platform side only, never into
                // SimCore. Normalize the drag, scale the rate in px, then
                // convert to Q32.32 int64 before the transport call.
                double len2 = (double)dx * (double)dx + (double)dy * (double)dy;
                double len = (double)UnityEngine.Mathf.Sqrt((float)len2); // Mathf.Sqrt: the PascalCase member settled by the tenth extension
                if (len <= 0.0)
                {
                    InputAdapter.pushIntent(0L, 0L, 0L); // held but no movement yet: neutral
                    return;
                }
                double nx = (double)dx / len;
                double ny = (double)dy / len;
                long thrustX = (long)(nx * 4294967296.0); // Q32.32 of the unit direction
                long thrustY = (long)(ny * 4294967296.0);
                long rate;
                if (len >= (double)dragFullPx)
                {
                    rate = FixedQ.ONE; // 1.0 Q32.32
                }
                else
                {
                    rate = (long)(len / (double)dragFullPx * 4294967296.0); // min(len / DRAG_FULL_PX, 1)
                }
                InputAdapter.pushIntent(thrustX, thrustY, rate);
            }
            else
            {
                // no contact: clear the anchor and coast/neutral — the existing
                // rule (InputAdapter.pause with the A-011 ordering).
                dragContact = false;
                InputAdapter.pause();
            }
        }

        /// <summary>
        /// Pause/resume is a platform concern (A-019 §11, A-018 fail routing);
        /// phase 1 only stops stepping. The state and its accumulator debt are
        /// untouched, so resume continues the SAME deterministic step sequence —
        /// there is no re-seed and no rebuild path here.
        /// </summary>
        public void Pause()
        {
            // contract slot: the platform layer stops calling Update(); nothing
            // in the sim state changes, so H-02 replay equality is unaffected.
        }
    }
}

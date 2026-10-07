// A-021 Phase 1 prototype — Assets/Runtime/Bridge/Unity/InputAdapter.cs
// A-019 §4: InputAdapter (transport ONLY): telemetry/pointer events -> A-011
// intent channel -> ONE fixed-point Intent sampled once per step via InputDigest
// codes. Holds NO input scheme: no drag, joystick, tilt, snap, or aim logic —
// those are A-011 (designer-input) and out of scope here (brief §9).
//
// Unity-API-free (brief §9: compiles in the headless harness). The adapter
// never reads a device, a clock, or a pointer position: it receives already-
// resolved intent values from the platform telemetry layer and forwards them
// into the digest-code channel (EIDIG1 codes, A-019 §8), which is the single
// deterministic input surface a run replays from.
//
// A-011 semantics PRESERVED here (device handling stays out of scope):
//  - first-contact-wins: the first intent pushed for a step locks the step's
//    code; later contacts in the same step are DISCARDED (never blended,
//    never overwritten — no-softening).
//  - pause-cancels-intent: a pause with no locked contact forces coast/neutral
//    (codes 16/10) for the step; a locked contact stays locked (first-contact-
//    wins takes precedence over a late pause, per A-011 ordering).
//
// Pure C#, zero Unity API, no float/double, Q32.32 raw int64 (A-019 §5).

namespace EH
{
    public static class InputAdapter
    {
        // ---- the A-011 intent channel: EIDIG1 per-step code pairs --------------
        public static InputDigest.Codes codes; // codec-owned storage (InputDigest.cs)
        public static bool lockedThisStep;     // first-contact-wins flag for the open step
        public static long lastSampledStep;    // monotonic sample cursor (-1 = none yet)

        /// <summary>
        /// Boot the channel with room for the expected run length (codec grows
        /// itself; this is just the initial capacity).
        /// </summary>
        public static void boot(int initialCapacity)
        {
            codes = new InputDigest.Codes();
            InputDigest.Codes.boot(codes, initialCapacity);
            lockedThisStep = false;
            lastSampledStep = -1L;
        }

        /// <summary>
        /// TELEMETRY ENTRY POINT (transport only): the platform layer hands over
        /// an already-resolved intent — thrust direction (Q32.32) and rate
        /// (Q32.32 in [0,1]) — with no scheme interpretation here. The value is
        /// quantized through InputDigest's canonical codes and pushed once.
        /// First-contact-wins: if this step already has a locked contact, the
        /// later contact is discarded (identity, no cushioning).
        /// </summary>
        public static void pushIntent(long thrustX, long thrustY, long rate)
        {
            if (lockedThisStep)
            {
                return; // first-contact-wins (A-011): later contacts never overwrite
            }
            int angle = InputDigest.quantizeAngle(thrustX, thrustY); // 16-way or 16=coast
            int rateCode = InputDigest.quantizeRate(rate);           // 0..20, 10=neutral
            InputDigest.Codes.push(codes, angle, rateCode);
            lockedThisStep = true;
        }

        /// <summary>
        /// PAUSE semantics (A-011: pause cancels the intent). With no locked
        /// contact for the open step, the step records coast/neutral (16/10).
        /// With a locked contact, first-contact-wins keeps it — a pause arriving
        /// after the contact cannot rewrite the step's code.
        /// </summary>
        public static void pause()
        {
            if (lockedThisStep)
            {
                return; // contact already locked this step (first-contact-wins)
            }
            InputDigest.Codes.push(codes,
                InputDigest.ANGLE_COAST, InputDigest.RATE_NEUTRAL);
            lockedThisStep = true;
        }

        /// <summary>
        /// Close the open step (called once per sim step by the frame driver
        /// after sampleIntent). Opens the next step's first-contact window.
        /// </summary>
        public static void endStep()
        {
            lockedThisStep = false;
        }

        /// <summary>
        /// The ONE fixed-point Intent sampled for this step (A-019 §4: input
        /// quantized to steps; exactly one sample per step). Reads the code
        /// pair just recorded for stepIndex through the canonical code tables.
        /// Hard-asserts the sample cursor is monotonic — a step is never
        /// sampled twice and never rewound (determinism, no-softening).
        /// </summary>
        public static Intent sampleIntent(long stepIndex)
        {
            Fail.check(stepIndex == lastSampledStep + 1L,
                "InputAdapter.sampleIntent: step must advance exactly one per sample (sampled once per step)");
            Fail.check(codes.count >= (int)(stepIndex + 1L),
                "InputAdapter.sampleIntent: no code recorded for this step (frame driver must push/pause before sampling)");

            int a = codes.angle[(int)stepIndex];
            int r = codes.rate[(int)stepIndex];

            Intent i0 = new Intent();
            i0.thrustX = InputDigest.ANGLE_X[a];
            i0.thrustY = InputDigest.ANGLE_Y[a];
            i0.rate = InputDigest.rateFromCode(r);
            lastSampledStep = stepIndex;
            return i0;
        }

        /// <summary>
        /// Replay path: sample the Intent for one step straight from a
        /// committed EIDIG1 digest (delegates to InputDigest.decode — hard
        /// rejection with named reasons, never silent fill). Used by the
        /// harness (H-02) and by re-simulable playback; identical codes.
        /// </summary>
        public static Intent sampleFromDigest(byte[] digest, long step)
        {
            return InputDigest.decode(digest, step);
        }

        /// <summary>
        /// Encode the recorded channel into the EIDIG1 byte stream the run is
        /// identified by ((sessionSeed, inputDigest), A-005). Codec lives in
        /// InputDigest; nothing here re-encodes.
        /// </summary>
        public static byte[] encodeChannel()
        {
            return InputDigest.encode(codes);
        }
    }
}

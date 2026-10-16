#!/bin/bash
# tools/player-package/capture.sh — the single-shot real-device capture for
# A-014's device gates (P-01, P-05) and the visual proof. One command, one
# structured verdict. This tool exists to end the recurring pattern of many
# install/launch/logcat/screencap attempts: the game self-reports (EH-BOOT,
# EH-PACE, EH-CAMERA, EH-FRAME, EH-DRAW, EH-PERF), so the LOG decides the
# simulation and pacing gates, and the screencap is read TOGETHER WITH the
# display state so "black" is never ambiguous again (black + display OFF/DOZE
# = inconclusive environment, never a game failure).
# Never prints a number it did not measure; never greens a gate silently.
#
# Usage: capture.sh [serial] [apk] [wait-seconds]
set -u
SERIAL="${1:-RZCXB2BSF8P}"
APK="${2:-Build/EventHorizon-Android.apk}"
WAIT="${3:-25}"
PKG=com.DefaultCompany.eventhorizongame
ACT=com.unity3d.player.UnityPlayerGameActivity
ADB="adb -s $SERIAL"

say() { echo "$1"; }
incon() { say "VERDICT: INCONCLUSIVE"; say "reason=$1"; exit 1; }

say "== capture $(date -u +%Y-%m-%dT%H:%M:%SZ) serial=$SERIAL apk=$APK =="

# 1. connection: one wait-for-device, then proceed or stop with a reason.
$ADB shell "echo connected" >/dev/null 2>&1 || incon "device-not-connected(reconnect-the-phone)"

# 2. install + enable (the settled recipe: plain install, then enable).
$ADB install "$APK" >/dev/null 2>&1 || incon "install-failed"
$ADB shell "cmd package enable $PKG" >/dev/null 2>&1

# 3. launch. A plain install does NOT restart a running instance, so any
#    stale process is killed first and the log buffer is cleared: the EH
#    lines read below belong to THIS run only.
old_pid=$($ADB shell "ps -A" 2>/dev/null | grep "$PKG" | awk '{print $2}' | head -1)
[ -n "$old_pid" ] && $ADB shell "kill $old_pid" >/dev/null 2>&1 && sleep 2
$ADB logcat -C >/dev/null 2>&1
$ADB shell "am start-activity $PKG/$ACT" 2>&1 | tail -1
sleep "$WAIT"

# 4. the app must be running (ps is the reliable check; am may warn
#    "delivered to currently running top-most instance").
ps_line=$($ADB shell "ps -A" 2>/dev/null | grep "$PKG" | head -1)
[ -z "$ps_line" ] && incon "app-not-running"
say "app: ${ps_line}"

# 5. the self-report: every EH-* line the game printed.
log=$($ADB logcat -d 2>/dev/null | grep -o 'EH-[^"]*')
if ! printf '%s\n' "$log" | grep -q 'EH-BOOT'; then
  incon "no-EH-BOOT(Start()-did-not-run)"
fi
say "--- EH log ---"
printf '%s\n' "$log"

# 6. display state, read WITH the screencap so black is never ambiguous.
state=$($ADB shell "dumpsys display" 2>/dev/null | grep -o 'Display State=[A-Z_]*' | head -1)
say "display-state: ${state:-unknown}"

# 7. the screencap, decoded stdlib-only (no PIL on this host).
# /sd is READ-ONLY on this Android 16 build; /data/local/tmp is the writable
# location (measured 2026-10-16).
$ADB shell "screencap -p /data/local/tmp/eh-cap.png" >/dev/null 2>&1 && \
  $ADB pull /data/local/tmp/eh-cap.png /tmp/opencode/eh-cap.png >/dev/null 2>&1 || incon "screencap-failed"
px=$(python3 - <<'PY'
import zlib, struct
d = open('/tmp/opencode/eh-cap.png','rb').read()
i, idat = 8, b''
w = h = 0; ct = 2
while i + 8 <= len(d):
    ln = struct.unpack('>I', d[i:i+4])[0]; t = d[i+4:i+8]
    if t == b'IHDR':
        w, h, bd, ct = struct.unpack('>IIBB', d[i+8:i+18])
    elif t == b'IDAT':
        idat += d[i+8:i+8+ln]
    i += 12 + ln
raw = zlib.decompress(idat)
bpp = {0:1, 2:3, 6:4}.get(ct, 0)
if bpp == 0:
    print("UNDECODABLE"); raise SystemExit
stride = w * bpp
def paeth(a, b, c):
    p = a + b - c
    return b if (abs(p-a) <= abs(p-b) and abs(p-a) <= abs(p-c)) else (c if abs(p-b) <= abs(p-c) else a)
prev = bytearray(stride)
pos = 0
# the center is the black horizon disc (A-026 draws it), so the decisive
# samples are FIELD points outside the horizon radius (~429 px measured):
# they must read the camera field pack_px(8,10,18) when the camera presents.
pts = {(h//2, w//2): 'center', (h//2+500, w//2): 'field', (h//2-500, w//2): 'field2',
       (h//2, w//2+500): 'field3', (h//2, w//2-500): 'field4'}
found = {}
for y in range(h):
    f = raw[pos]; pos += 1
    line = bytearray(raw[pos:pos+stride]); pos += stride
    if f == 1:
        for x in range(bpp, stride): line[x] = (line[x] + line[x-bpp]) & 255
    elif f == 2:
        for x in range(stride): line[x] = (line[x] + prev[x]) & 255
    elif f == 3:
        for x in range(stride):
            a = line[x-bpp] if x >= bpp else 0
            c = prev[x-bpp] if x >= bpp else 0
            line[x] = (line[x] + paeth(a, prev[x], c)) & 255
    elif f == 4:
        for x in range(stride):
            a = line[x-bpp] if x >= bpp else 0
            line[x] = (line[x] + ((a + prev[x]) >> 1)) & 255
    for (yy, xx), name in pts.items():
        if yy == y:
            p = line[xx*bpp:xx*bpp+3]
            found[name] = (p[0], p[1], p[2])
    prev = line
print(f"{w}x{h}")
for name in ('center', 'field', 'field2', 'field3', 'field4'):
    if name in found:
        r, g, b = found[name]
        print(f"{name}-px=({r},{g},{b})")
PY
)
say "screencap: ${px:-undecoded}"

# 8. the verdict: gates from the LOG (display-independent), visual from
#    pixel + display state TOGETHER.
p95=$(printf '%s\n' "$log" | grep -o 'EH-PERF.*' | tail -1 | grep -o 'p95_ms=[0-9.]*' | cut -d= -f2)
alloc=$(printf '%s\n' "$log" | grep -o 'EH-PERF.*' | tail -1 | grep -o 'allocs_per_frame_max=[0-9]*' | cut -d= -f2)
say "P-01 p95_ms=${p95:-none} bound=16.6 -> $( [ -n "${p95:-}" ] && awk "BEGIN{exit !($p95<=16.6)}" && echo PASS || echo FAIL )"
say "P-05 allocs_per_frame_max=${alloc:-none} bound=0 -> $( [ -n "${alloc:-}" ] && [ "$alloc" = 0 ] && echo PASS || echo FAIL )"
case "$state" in
  *ON*)
    fields=$(printf '%s\n' "$px" | grep -o 'field[0-9]*-px=([0-9,]*)')
    nonblack=$(printf '%s\n' "$fields" | grep -cv 'field[0-9]*-px=(0,0,0)' || true)
    exact=$(printf '%s\n' "$fields" | grep -c 'field[0-9]*-px=(8,10,18)' || true)
    if [ "${exact:-0}" -gt 0 ] 2>/dev/null; then
      say "visual: PASS camera presents, field colour pack_px(8,10,18) confirmed"
    elif [ "${nonblack:-0}" -gt 0 ] 2>/dev/null; then
      say "visual: PRESENTING(field points non-black; exact colour differs — read the printed values)"
    else
      say "visual: FAIL camera-not-presenting(display ON, all field points pure black)"
    fi ;;
  *DOZE*)
    say "visual: INCONCLUSIVE(display dimmed standby; a non-black pixel still proves presenting)" ;;
  *)
    say "visual: INCONCLUSIVE(display not ON; the EH log still decides the sim/pacing gates)" ;;
esac
say "== end =="

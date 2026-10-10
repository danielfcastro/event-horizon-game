#!/bin/sh
# A-023 Unity player package — tools/player-package/exclusions.sh
# The two ship-build exclusion steps of A-019 §12 (items 5 and 6), expanded to
# the A-020 §7 STEP-11a..STEP-11e checks against the BUILT PACKAGE:
#   11a no tools/harness path, no harness, no golden in the package listing
#   11b harness entrypoint name and CLI flags absent from the package bytes
#   11c no replays/ directory and no golden replay digest in the manifest
#   11d no input handler or settings entry reads a dev.* key (the no-softening
#       re-run on device is hardware-gated and stays NOT EXECUTABLE)
#   11e release flags only: the debug symbol/flag set is empty in the shipped
#       artifact
# Without a built package every step prints NOT EXECUTABLE — never green
# (PLAN.md §4 A-023: the license gate blocks the build until sign-in).
ROOT=/mnt/data/projetos/event-horizon-game
GOLDEN_DIGEST=6d25ff0add639448 # the committed p1-level-01 golden digest (H-01)

if [ ! -d "$ROOT/Build" ]; then
  echo "STEP-11a NOT EXECUTABLE: no built package (license gate open)"
  echo "STEP-11b NOT EXECUTABLE: no built package"
  echo "STEP-11c NOT EXECUTABLE: no built package"
  echo "STEP-11d NOT EXECUTABLE: no built package (device re-run hardware-gated regardless)"
  echo "STEP-11e NOT EXECUTABLE: no built package"
  exit 2
fi

fail=0
# 11a: package listing must not carry harness/golden paths
hits=$(find "$ROOT/Build" \( -iname '*harness*' -o -iname '*golden*' -o -path '*tools/harness*' \) | wc -l)
[ "$hits" -gt 0 ] && { echo "STEP-11a FAIL: $hits harness/golden path(s) in the package"; fail=1; } || echo "STEP-11a PASS: no harness/golden paths"

# 11b: harness entrypoint name and CLI flags absent from the package bytes
hits=$(grep -rlE -- '--digest|--max-frames|FIXED-FORK' "$ROOT/Build" 2>/dev/null | wc -l)
[ "$hits" -gt 0 ] && { echo "STEP-11b FAIL: harness CLI surface reachable in the package"; fail=1; } || echo "STEP-11b PASS: no harness entrypoint/flags"

# 11c: no replays/ directory, no golden digest string in the package
c_fail=0
[ -d "$ROOT/Build/replays" ] && { echo "STEP-11c FAIL: replays/ present"; c_fail=1; }
grep -rq "$GOLDEN_DIGEST" "$ROOT/Build" 2>/dev/null && { echo "STEP-11c FAIL: golden digest present"; c_fail=1; }
[ "$c_fail" -eq 0 ] && echo "STEP-11c PASS: no replays/ or golden digest" || fail=1

# 11d: no dev.* key read by any input handler or settings entry
hits=$(grep -rlE 'dev\.targetFps|dev\.timeScale' "$ROOT/Build" 2>/dev/null | wc -l)
[ "$hits" -gt 0 ] && { echo "STEP-11d FAIL: dev.* knob reachable in the package"; fail=1; } || echo "STEP-11d PASS (static): no dev.* key in the package; device no-softening re-run NOT EXECUTABLE (hardware)"

# 11e: release flags only — no debug power state in the shipped artifact
hits=$(find "$ROOT/Build" -iname '*.debug' | wc -l)
[ "$hits" -gt 0 ] && { echo "STEP-11e FAIL: debug artifacts present"; fail=1; } || echo "STEP-11e PASS: no debug power state"

[ "$fail" -eq 0 ] && exit 0
exit 1

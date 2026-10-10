#!/bin/sh
# A-023 Unity player package — tools/player-package/build.sh
# The candidate ship-build recipe: run the verified Unity 6 LTS editor headless
# against THIS repo (the repo root is the Unity project root) and build the
# Android and iOS players via Assets/Editor/BuildPlayerPackage.cs.
#
# Verified 2026-10-10 with the license gate open (Unity Personal active):
# the editor flag is -executeMethod. -executeCommand is NOT a Unity flag — the
# editor ignores an unknown flag, exits 0, and writes nothing, which is a false
# green; so success is reported only when the package exists on disk, and a
# stale package is removed first so it cannot make a failed build look green.
# The equivalent Unity CLI form is
#   unity build --target Android --execute-method BuildPlayerPackage.BuildAll
# which also prints editorErrors: [].
#
# Exit codes: 0 a package exists; 2 no package (license gate or build failure).
ROOT=/mnt/data/projetos/event-horizon-game
EDITOR=/home/dfcastro/Unity/Hub/Editor/6000.0.84f1/Editor/Unity

rm -rf "$ROOT/Build"
"$EDITOR" -batchmode -quit -consoleLog \
  -projectPath "$ROOT" \
  -executeMethod BuildPlayerPackage.BuildAll
status=$?
if [ ! -f "$ROOT/Build/EventHorizon-Android.apk" ]; then
  echo "build: no package on disk (Unity exited $status; license gate or build failure); player build NOT EXECUTABLE, never reported green"
  exit 2
fi
if [ "$status" -ne 0 ]; then
  echo "build: Unity exited $status AND a package exists — inspect the log before calling it green"
  exit 2
fi
echo "build: Build/EventHorizon-Android.apk and Build/EventHorizon-iOS.app written; exclusions.sh can now run against them"

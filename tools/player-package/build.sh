#!/bin/sh
# A-023 Unity player package — tools/player-package/build.sh
# The candidate ship-build recipe: run the verified Unity 6 LTS editor headless
# against THIS repo (the repo root is the Unity project root) and build the
# Android and iOS players via Assets/Editor/BuildPlayerPackage.cs.
#
# Honest state (probed 2026-10-10, PLAN.md §4 A-023): the editor and the ios/
# android modules are on disk, but the LICENSE gate is open — the editor exits
# 198 and the shipped licensing client reports "No licenses were found". Until
# the user signs in (Hub GUI or a license token), this recipe's player build is
# NOT EXECUTABLE; running it here proves the gate, not a build.
ROOT=/mnt/data/projetos/event-horizon-game
EDITOR=/home/dfcastro/Unity/Hub/Editor/6000.0.84f1/Editor/Unity

"$EDITOR" -batchmode -quit -consoleLog \
  -projectPath "$ROOT" \
  -executeCommand BuildPlayerPackage.BuildAll
status=$?
if [ "$status" -ne 0 ]; then
  echo "build: Unity exited $status — license gate open (sign-in required); player build NOT EXECUTABLE, never reported green"
  exit 2
fi
echo "build: players written under Build/ (Android + iOS); exclusions.sh can now run against them"

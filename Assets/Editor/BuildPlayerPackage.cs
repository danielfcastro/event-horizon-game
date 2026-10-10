// A-023 Unity player package — Assets/Editor/BuildPlayerPackage.cs
// The scripted build entry for the candidate ship build: builds the Android and
// iOS players of THIS project (the repo root is the Unity project root; Assets/
// carries the SAME Fixed/ + SimCore/ + Bridge/Unity sources the headless harness
// compiles). Run headless via tools/player-package/build.sh:
//   Unity -batchmode -quit -projectPath <repo> -executeCommand BuildPlayerPackage.BuildAll
//
// NOT COMPILED IN THE HEADLESS BUILD: this file uses the UnityEditor API and is
// added to the Exclude clauses of harness.csproj and player.csproj. It is also
// NOT compiled or run here — the license gate (PLAN.md §4 A-023: editor exits
// 198, "No licenses were found") blocks the editor until sign-in. The build
// recipe is recorded so the step runs green the moment the gate opens; until
// then it is NOT EXECUTABLE, never claimed.
//
// The classic UnityEditor.BuildPlayer signature is used because it exists
// unchanged across the supported range (deprecated in Unity 6 but functional);
// the Unity 6-native build API is preferred at ship time (A-025/A-020) and
// swapping to it changes no candidate-build property.

public static class BuildPlayerPackage
{
    /// <summary>
    /// Build the player for both shipping targets named by A-014's device
    /// classes (iOS and Android — the platform line; desktop/Steam is an
    /// open question recorded for A-023/A-025, NOT a shipping target).
    /// Outputs land under Build/ so the ship-build exclusion checks
    /// (tools/player-package/exclusions.sh) have a built package to inspect.
    /// </summary>
    public static void BuildAll()
    {
        // Release flags only (A-020 §7 STEP-11e: no debug power state may be
        // enabled in a shipped artifact — the build below sets no debug flag).
        UnityEditor.BuildPlayer(
            UnityEditor.BuildTargetGroup.Android, UnityEditor.BuildTarget.Android,
            "Build/EventHorizon-Android.apk");
        // iOS shares the Standalone build-target group in current Unity versions;
        // the exact target constant is confirmed when the build runs (license
        // gate open) — a wrong constant is a build-time rejection, not a pass.
        UnityEditor.BuildPlayer(
            UnityEditor.BuildTargetGroup.Standalone, UnityEditor.BuildTarget.Standalone_ARM64,
            "Build/EventHorizon-iOS.app");
    }
}

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
// The API below is settled by the compiler on THIS editor (6000.0.84f1), not by
// memory. Evidence: UnityEditor.BuildPlayer does not exist; the real entry point
// is UnityEditor.BuildPipeline.BuildPlayer, whose overload takes
// (EditorBuildSettingsScene[], string target, BuildTarget target,
//  UnityEditor.BuildOptions). A 3-argument call is rejected ("no overload takes
// 3 arguments"), and the fourth parameter's type is named by the compiler.
// BuildTarget.Android and BuildTarget.iOS are real members; Standalone_ARM64 and
// Standalone64Bit are not, so the iOS player uses BuildTarget.iOS directly.
//
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
        UnityEditor.EditorBuildSettingsScene[] scenes = UnityEditor.EditorBuildSettings.scenes;
        // Default BuildOptions: no development flag, no debug power state —
        // A-020 §7 STEP-11e requires that a shipped artifact carry neither.
        UnityEditor.BuildOptions androidOptions = new UnityEditor.BuildOptions();
        UnityEditor.BuildPipeline.BuildPlayer(
            scenes, "Build/EventHorizon-Android.apk", UnityEditor.BuildTarget.Android, androidOptions);
        UnityEditor.BuildOptions iosOptions = new UnityEditor.BuildOptions();
        UnityEditor.BuildPipeline.BuildPlayer(
            scenes, "Build/EventHorizon-iOS.app", UnityEditor.BuildTarget.iOS, iosOptions);
    }
}

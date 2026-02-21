#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class WindowsBuild
{
    private const string OutputFolder = "Build/Windows";
    private const string ExeName = "TopDownRunner.exe";

    [MenuItem("Build/Build Windows EXE")]
    public static void BuildWindowsExe()
    {
        Build(false);
    }

    public static void BuildFromCommandLine()
    {
        Build(true);
    }

    private static void Build(bool isBatchMode)
    {
        string[] scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);

        if (scenes.Length == 0)
        {
            throw new InvalidOperationException("No scenes are enabled in Build Settings.");
        }

        Directory.CreateDirectory(OutputFolder);
        string outputPath = Path.Combine(OutputFolder, ExeName);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            target = BuildTarget.StandaloneWindows64,
            locationPathName = outputPath,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result != BuildResult.Succeeded)
        {
            throw new Exception($"Windows build failed: {summary.result}");
        }

        if (!isBatchMode)
        {
            EditorUtility.RevealInFinder(Path.GetFullPath(OutputFolder));
        }

        UnityEngine.Debug.Log($"Windows EXE build completed: {outputPath}");
    }
}
#endif

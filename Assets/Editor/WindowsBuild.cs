#if UNITY_EDITOR
using System;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class WindowsBuild
{
    private const string OutputFolder = "Build/Windows";
    private const string ReleaseFolder = "Build/Releases";
    private const string ExeName = "TopDownRunner.exe";
    private const string ZipName = "TopDownRunner-Windows.zip";

    [MenuItem("Build/Build Windows EXE")]
    public static void BuildWindowsExe()
    {
        BuildAndPackage(false);
    }

    public static void BuildFromCommandLine()
    {
        BuildAndPackage(true);
    }

    private static void BuildAndPackage(bool isBatchMode)
    {
        string[] scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);

        if (scenes.Length == 0)
        {
            throw new InvalidOperationException("No scenes are enabled in Build Settings.");
        }

        Directory.CreateDirectory(OutputFolder);
        Directory.CreateDirectory(ReleaseFolder);

        string exePath = Path.Combine(OutputFolder, ExeName);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            target = BuildTarget.StandaloneWindows64,
            locationPathName = exePath,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result != BuildResult.Succeeded)
        {
            throw new Exception($"Windows build failed: {summary.result}");
        }

        string zipPath = Path.Combine(ReleaseFolder, ZipName);
        CreatePortableZip(OutputFolder, zipPath);

        if (!isBatchMode)
        {
            EditorUtility.RevealInFinder(Path.GetFullPath(ReleaseFolder));
        }

        UnityEngine.Debug.Log($"Windows EXE build completed: {exePath}");
        UnityEngine.Debug.Log($"Portable ZIP created: {zipPath}");
    }

    private static void CreatePortableZip(string sourceFolder, string zipPath)
    {
        if (File.Exists(zipPath))
        {
            File.Delete(zipPath);
        }

        ZipFile.CreateFromDirectory(sourceFolder, zipPath, CompressionLevel.Optimal, false);
    }
}
#endif

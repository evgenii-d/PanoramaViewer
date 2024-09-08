using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

/// <summary>
/// Updates the Unity build version
/// from a .VERSION file before each build.
/// </summary>
public class BuildVersionUpdater : IPreprocessBuildWithReport
{
    /// <summary>
    /// Ensures this script runs early in the build process.
    /// </summary>
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        var versionFilePath = Path.Combine(
            Directory.GetParent(Application.dataPath).ToString(),
            ".VERSION"
        );

        if (File.Exists(versionFilePath))
        {
            var appVersion = File.ReadLines(versionFilePath).First();
            PlayerSettings.bundleVersion = appVersion;
        }
        else Debug.LogError(
            $"Application version file not found - {versionFilePath}"
        );
    }
}
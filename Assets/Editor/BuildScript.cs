using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Linq;

/// <summary>
/// CI/CD 构建脚本 - 用于 GitHub Actions 自动化构建
/// </summary>
public static class BuildScript
{
    private static readonly string[] iOSBuildScenes = new[]
    {
        "Assets/Scenes/Init.unity",
        "Assets/Scenes/Title.unity",
        "Assets/Scenes/GameClassic.unity",
        "Assets/Scenes/GameLab.unity",
        "Assets/Scenes/GameRPG.unity",
        "Assets/Scenes/GameKindle.unity"
    };

    /// <summary>
    /// 构建 iOS Xcode 项目 (GitHub Actions 调用)
    /// </summary>
    public static void BuildIOS()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = GetEnabledScenes(),
            locationPathName = "build/iOS",
            target = BuildTarget.iOS,
            options = BuildOptions.None
        };

        // 开发构建
        string development = GetCommandLineArg("-development");
        if (development == "true")
        {
            buildPlayerOptions.options |= BuildOptions.Development;
        }

        // iOS 特定设置
        PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
        PlayerSettings.iOS.targetOSVersionString = "15.0";
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
        PlayerSettings.iOS.appleEnableAutomaticSigning = false;
        PlayerSettings.iOS.appleDeveloperTeamID = "WB5752S5M6";
        PlayerSettings.applicationIdentifier = "com.doraccoon.raccoin";
        PlayerSettings.productName = "RACCOIN";
        PlayerSettings.bundleVersion = "1.0.0";
        PlayerSettings.iOS.buildNumber = "1";

        // IL2CPP 优化
        PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.iOS, Il2CppCompilerConfiguration.Release);
        PlayerSettings.SetIl2CppCodeGeneration(BuildTargetGroup.iOS, Il2CppCodeGeneration.OptimizeSpeed);

        Debug.Log("[BuildScript] Starting iOS build...");
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[BuildScript] iOS build succeeded: {summary.totalSize / 1024 / 1024} MB");
        }
        else
        {
            Debug.LogError($"[BuildScript] iOS build failed: {summary.result}");
            EditorApplication.Exit(1);
        }
    }

    /// <summary>
    /// 构建 Android APK (备用)
    /// </summary>
    public static void BuildAndroid()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = GetEnabledScenes(),
            locationPathName = "build/Android/RACCOIN.apk",
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        if (report.summary.result != BuildResult.Succeeded)
        {
            EditorApplication.Exit(1);
        }
    }

    /// <summary>
    /// 运行测试
    /// </summary>
    public static void RunTests()
    {
        Debug.Log("[BuildScript] Running tests...");
        // 测试逻辑
    }

    private static string[] GetEnabledScenes()
    {
        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            Debug.LogWarning("[BuildScript] No enabled scenes found, using default scenes");
            return iOSBuildScenes.Where(s => File.Exists(s)).ToArray();
        }

        return scenes;
    }

    private static string GetCommandLineArg(string name)
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == name)
            {
                return args[i + 1];
            }
        }
        return null;
    }
}

/// <summary>
/// 项目设置验证器
/// </summary>
[InitializeOnLoad]
public static class ProjectValidator
{
    static ProjectValidator()
    {
        // 确保必要的文件夹存在
        EnsureDirectory("Assets/Scenes");
        EnsureDirectory("Assets/Resources");
        EnsureDirectory("Assets/Resources/Prefabs");
        EnsureDirectory("Assets/Resources/Config");
        EnsureDirectory("Assets/Resources/FX");
        EnsureDirectory("Assets/StreamingAssets");
    }

    private static void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            Debug.Log($"[ProjectValidator] Created directory: {path}");
        }
    }
}

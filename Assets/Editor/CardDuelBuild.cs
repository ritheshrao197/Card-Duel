using UnityEngine;
using UnityEditor;

/// <summary>
/// Build scripts for Card Duel game
/// </summary>
public class CardDuelBuild 
{
    [MenuItem("Card Duel/Build/Build Android APK")]
    public static void BuildAndroid()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/MainScene.unity" };
        buildPlayerOptions.locationPathName = "Builds/Android/CardDuel.apk";
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;
        
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        
        Debug.Log("Android build completed!");
    }
    
    [MenuItem("Card Duel/Build/Build Windows")]
    public static void BuildWindows()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/MainScene.unity" };
        buildPlayerOptions.locationPathName = "Builds/Windows/CardDuel.exe";
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.options = BuildOptions.None;
        
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        
        Debug.Log("Windows build completed!");
    }
    
    [MenuItem("Card Duel/Build/Build Mac")]
    public static void BuildMac()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Scenes/MainScene.unity" };
        buildPlayerOptions.locationPathName = "Builds/Mac/CardDuel.app";
        buildPlayerOptions.target = BuildTarget.StandaloneOSX;
        buildPlayerOptions.options = BuildOptions.None;
        
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        
        Debug.Log("Mac build completed!");
    }
    
    [MenuItem("Card Duel/Setup/Setup Android")]
    public static void SetupAndroid()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel21;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel30;
        
        // Enable required permissions
        PlayerSettings.Android.forceInternetPermission = true;
        PlayerSettings.Android.forceSDCardPermission = false;
        
        Debug.Log("Android setup completed!");
    }
    
    [MenuItem("Card Duel/Tools/Validate Build")]
    public static void ValidateBuild()
    {
        // Check required scenes exist
        string[] requiredScenes = { "Assets/Scenes/MainScene.unity" };
        foreach (string scene in requiredScenes)
        {
            if (!System.IO.File.Exists(scene))
            {
                Debug.LogError($"Required scene not found: {scene}");
                return;
            }
        }
        
        // Check required packages
        var packages = UnityEditor.PackageManager.Client.List();
        while (!packages.IsCompleted) { }
        
        bool hasNetcode = false;
        bool hasTextMeshPro = false;
        
        foreach (var package in packages.Result)
        {
            if (package.name.Contains("netcode.gameobjects"))
                hasNetcode = true;
            if (package.name.Contains("textmeshpro"))
                hasTextMeshPro = true;
        }
        
        if (!hasNetcode)
            Debug.LogWarning("Netcode for GameObjects package not found!");
            
        if (!hasTextMeshPro)
            Debug.LogWarning("TextMeshPro package not found!");
            
        Debug.Log("Build validation completed!");
    }
}
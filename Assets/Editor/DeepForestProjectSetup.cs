using UnityEditor;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.Build.Reporting;

[InitializeOnLoad]
public static class DeepForestProjectSetup
{
    private const string SceneFolder = "Assets/Scenes";
    private const string ScenePath = SceneFolder + "/DeepForest.unity";

    static DeepForestProjectSetup()
    {
        EditorApplication.delayCall += CreateSceneIfMissing;
    }

    private static void CreateSceneIfMissing()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isPlaying)
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            return;
        }

        string fullScenePath = Path.Combine(Directory.GetCurrentDirectory(),
            ScenePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullScenePath))
            CreatePlayableScene();
        else
            OpenPlayableSceneIfEditorIsEmpty();
    }

    private static void OpenPlayableSceneIfEditorIsEmpty()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.IsValid() && string.IsNullOrEmpty(activeScene.path) &&
            activeScene.GetRootGameObjects().Length == 0)
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Debug.Log("Opened the Deep Forest playable scene.");
        }
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredEditMode)
            return;

        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.delayCall += CreateSceneIfMissing;
    }

    [MenuItem("Deep Forest/Create Playable Scene")]
    public static void CreatePlayableScene()
    {
        if (!Directory.Exists(SceneFolder))
            Directory.CreateDirectory(SceneFolder);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject bootstrap = new GameObject("GameBootstrap");
        bootstrap.AddComponent<GameBootstrap>();

        if (!EditorSceneManager.SaveScene(scene, ScenePath))
            throw new IOException("Unity could not save the Deep Forest scene at " + ScenePath);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };
        AssetDatabase.SaveAssets();
        Debug.Log("Deep Forest playable scene created and added to the build settings.");
    }

    [MenuItem("Deep Forest/Open Playable Scene")]
    public static void OpenPlayableScene()
    {
        if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(),
                ScenePath.Replace('/', Path.DirectorySeparatorChar))))
            CreatePlayableScene();
        else
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
    }

    [MenuItem("Deep Forest/Build Windows Player")]
    public static void BuildWindowsPlayer()
    {
        string outputPath = "Build/Windows/DeepForest.exe";
        string outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Build/Windows");
        Directory.CreateDirectory(outputDirectory);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Length > 0
                ? GetEnabledScenePaths()
                : new[] { ScenePath },
            locationPathName = outputPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
            throw new IOException("Windows build failed: " + report.summary.result);

        Debug.Log("Windows player built at " + outputPath + " (" + report.summary.totalSize + " bytes).");
    }

    private static string[] GetEnabledScenePaths()
    {
        System.Collections.Generic.List<string> paths = new System.Collections.Generic.List<string>();
        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            if (EditorBuildSettings.scenes[i].enabled)
                paths.Add(EditorBuildSettings.scenes[i].path);
        }
        return paths.ToArray();
    }
}

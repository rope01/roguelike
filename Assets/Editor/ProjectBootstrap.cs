using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class ProjectBootstrap
{
    private const string ScenePath = "Assets/Scenes/MovingPrototype.unity";

    static ProjectBootstrap() => EditorApplication.delayCall += EnsureScene;

    private static void EnsureScene()
    {
        if (Application.isPlaying || File.Exists(ScenePath)) return;
        Directory.CreateDirectory("Assets/Scenes");
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        new GameObject("Prototype World").AddComponent<PrototypeWorld>();
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        Debug.Log("Moving prototype scene created. Press Play.");
    }
}

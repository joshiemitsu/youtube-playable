using System.IO;
using RapidPro.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace RapidPro.Scenes.Editor
{
    /// <summary>
    /// Makes Play always start from the Bootstrap scene, so services exist in every scene you test.
    /// The scene you were editing is remembered and opened by the scene flow after Bootstrap.
    /// Switch it off with "Play From Any Scene" on the SceneFlowSettings asset.
    /// </summary>
    [InitializeOnLoad]
    internal static class PlayFromAnySceneHook
    {
        static PlayFromAnySceneHook()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredEditMode)
            {
                EditorSceneManager.playModeStartScene = null;
                return;
            }

            if (change != PlayModeStateChange.ExitingEditMode)
                return;

            var settings = SceneFlowSettings.Load();
            if (settings == null || !settings.PlayFromAnyScene)
            {
                EditorSceneManager.playModeStartScene = null;
                return;
            }

            var bootstrap = FindScene(settings.BootstrapScene);
            if (bootstrap == null)
            {
                Log.Warning($"Play from any scene: could not find a scene named '{settings.BootstrapScene}'. Playing the open scene instead.");
                return;
            }

            // Play loads scenes from disk, so unsaved edits would be missing. Ask first, like Unity does for builds.
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorApplication.isPlaying = false;
                return;
            }

            var active = SceneManager.GetActiveScene();
            string requested = PlayFromAnyScene.ResolveRequestedScene(active.name, active.path, settings);
            SessionState.SetString(PlayFromAnyScene.SessionKey, requested ?? string.Empty);

            EditorSceneManager.playModeStartScene = bootstrap;
        }

        private static SceneAsset FindScene(string sceneName)
        {
            foreach (var guid in AssetDatabase.FindAssets($"t:Scene {sceneName}"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == sceneName)
                    return AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            }

            return null;
        }
    }
}

using UnityEngine;

namespace RapidPro.Scenes
{
    /// <summary>
    /// Names of the game's scenes and how loading behaves. Create one with
    /// <b>Assets &gt; Create &gt; RapidPro &gt; Scene Flow Settings</b> and save it as
    /// <c>Resources/SceneFlowSettings.asset</c> so the Editor's "play from any scene" can find it.
    /// </summary>
    [CreateAssetMenu(fileName = ResourcesPath, menuName = "RapidPro/Scene Flow Settings")]
    public class SceneFlowSettings : ScriptableObject
    {
        /// <summary>Name the asset must have inside a Resources folder.</summary>
        public const string ResourcesPath = "SceneFlowSettings";

        [Tooltip("Loads first and never unloads. Holds the GameInstaller and every service.")]
        [SerializeField] private string _bootstrapScene = "Bootstrap";

        [Tooltip("Optional. Shows the logo and progress bar while the game loads, then unloads. Leave empty to skip.")]
        [SerializeField] private string _loadingScene = "";

        [Tooltip("The one scene every level plays in.")]
        [SerializeField] private string _gameScene = "Game";

        [Tooltip("Loaded on top of the Game scene. Holds the canvas and screens.")]
        [SerializeField] private string _gameUIScene = "GameUI";

        [Tooltip("Keep the Loading scene up at least this long, so the logo does not just flash.")]
        [Range(0f, 5f)]
        [SerializeField] private float _minimumLoadingSeconds = 1f;

        [Tooltip("Editor only. Pressing Play in any scene starts from Bootstrap first, then opens the scene you were editing.")]
        [SerializeField] private bool _playFromAnyScene = true;

        /// <summary>Bootstrap scene name.</summary>
        public string BootstrapScene => _bootstrapScene;

        /// <summary>Loading scene name, or empty when there is none.</summary>
        public string LoadingScene => _loadingScene;

        /// <summary>Game scene name.</summary>
        public string GameScene => _gameScene;

        /// <summary>Game UI scene name.</summary>
        public string GameUIScene => _gameUIScene;

        /// <summary>Shortest time the Loading scene stays up, in seconds.</summary>
        public float MinimumLoadingSeconds => _minimumLoadingSeconds;

        /// <summary>True when pressing Play in any scene should start from Bootstrap.</summary>
        public bool PlayFromAnyScene => _playFromAnyScene;

        /// <summary>True when a Loading scene is set.</summary>
        public bool HasLoadingScene => !string.IsNullOrEmpty(_loadingScene);

        /// <summary>Loads the settings from <c>Resources/SceneFlowSettings</c>, or returns null when there is none.</summary>
        public static SceneFlowSettings Load() => Resources.Load<SceneFlowSettings>(ResourcesPath);

        /// <summary>True when <paramref name="sceneName"/> is part of the normal start-up: Bootstrap, Loading, Game or GameUI.</summary>
        public bool IsStartupScene(string sceneName)
        {
            return sceneName == _bootstrapScene || sceneName == _gameScene || sceneName == _gameUIScene
                || (HasLoadingScene && sceneName == _loadingScene);
        }

        /// <summary>Fills the settings from code. For tests only.</summary>
        internal void Configure(string bootstrap, string loading, string game, string gameUI, float minimumLoadingSeconds)
        {
            _bootstrapScene = bootstrap;
            _loadingScene = loading;
            _gameScene = game;
            _gameUIScene = gameUI;
            _minimumLoadingSeconds = minimumLoadingSeconds;
        }

#if UNITY_EDITOR
        // Warns in the Console when a named scene is missing from File > Build Profiles (Build Settings),
        // because scenes that are not listed there cannot be loaded by name.
        private void OnValidate()
        {
            WarnIfNotInBuild(_bootstrapScene);
            WarnIfNotInBuild(_loadingScene);
            WarnIfNotInBuild(_gameScene);
            WarnIfNotInBuild(_gameUIScene);
        }

        private void WarnIfNotInBuild(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
                return;

            foreach (var scene in UnityEditor.EditorBuildSettings.scenes)
            {
                if (System.IO.Path.GetFileNameWithoutExtension(scene.path) == sceneName)
                    return;
            }

            RapidPro.Core.Log.Warning($"SceneFlowSettings: scene '{sceneName}' is not in the build scene list, so it cannot be loaded. Add it under File > Build Profiles.", this);
        }
#endif
    }
}

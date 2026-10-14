namespace RapidPro.Scenes
{
    /// <summary>
    /// Editor helper behind "play from any scene". When you press Play in a scene that is not part of
    /// the normal start-up (e.g. a sandbox scene), the Editor starts from Bootstrap instead and remembers
    /// your scene here. <see cref="SceneFlowStarter"/> then opens it in place of Game and GameUI.
    /// In builds there is never a remembered scene.
    /// </summary>
    public static class PlayFromAnyScene
    {
        internal const string SessionKey = "RapidPro.Scenes.RequestedScene";

        /// <summary>
        /// Returns the scene to open instead of Game and GameUI, then forgets it.
        /// Null for a normal start.
        /// </summary>
        public static string ConsumeRequestedScene()
        {
#if UNITY_EDITOR
            string scene = UnityEditor.SessionState.GetString(SessionKey, string.Empty);
            UnityEditor.SessionState.EraseString(SessionKey);
            return string.IsNullOrEmpty(scene) ? null : scene;
#else
            return null;
#endif
        }

        /// <summary>
        /// The scene to remember when Play is pressed while <paramref name="sceneName"/> is open:
        /// null for start-up scenes (they start normally), otherwise the scene's path.
        /// </summary>
        internal static string ResolveRequestedScene(string sceneName, string scenePath, SceneFlowSettings settings)
        {
            if (string.IsNullOrEmpty(scenePath) || settings.IsStartupScene(sceneName))
                return null;

            return scenePath;
        }
    }
}

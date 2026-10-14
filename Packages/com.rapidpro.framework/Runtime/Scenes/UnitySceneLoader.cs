using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RapidPro.Scenes
{
    /// <summary>
    /// <see cref="ISceneLoader"/> built on Unity's <see cref="SceneManager"/>.
    /// In the Editor it can also load a scene by its path, even if it is not in the build list
    /// (used to open sandbox scenes through "play from any scene").
    /// </summary>
    public class UnitySceneLoader : ISceneLoader
    {
        /// <inheritdoc />
        public async UniTask LoadAdditiveAsync(string scene, IProgress<float> progress, CancellationToken cancellationToken)
        {
            AsyncOperation operation = StartLoad(scene);
            if (operation == null)
                throw new InvalidOperationException($"Could not load scene '{scene}'. Check its name and that it is in the build scene list.");

            await operation.ToUniTask(progress: progress, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public async UniTask UnloadAsync(string scene, CancellationToken cancellationToken)
        {
            var operation = SceneManager.UnloadSceneAsync(scene);
            if (operation != null)
                await operation.ToUniTask(cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public void SetActive(string scene)
        {
            var loaded = IsPath(scene) ? SceneManager.GetSceneByPath(scene) : SceneManager.GetSceneByName(scene);
            if (loaded.IsValid())
                SceneManager.SetActiveScene(loaded);
        }

        private static AsyncOperation StartLoad(string scene)
        {
#if UNITY_EDITOR
            if (IsPath(scene))
            {
                return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                    scene, new LoadSceneParameters(LoadSceneMode.Additive));
            }
#endif
            return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        }

        private static bool IsPath(string scene) => scene.EndsWith(".unity", StringComparison.OrdinalIgnoreCase);
    }
}

using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace RapidPro.Scenes
{
    /// <summary>
    /// The three scene operations <see cref="SceneFlow"/> needs. The real one is <see cref="UnitySceneLoader"/>;
    /// tests pass a fake so the flow can be checked without loading real scenes.
    /// </summary>
    public interface ISceneLoader
    {
        /// <summary>Loads a scene on top of the ones already open.</summary>
        /// <param name="scene">A scene name, or in the Editor a full path ending in <c>.unity</c>.</param>
        /// <param name="progress">Receives 0 to 1 while loading.</param>
        /// <param name="cancellationToken">Stops waiting when cancelled.</param>
        UniTask LoadAdditiveAsync(string scene, IProgress<float> progress, CancellationToken cancellationToken);

        /// <summary>Unloads a scene by name.</summary>
        UniTask UnloadAsync(string scene, CancellationToken cancellationToken);

        /// <summary>Makes a loaded scene the active one, so new objects are created in it.</summary>
        void SetActive(string scene);
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using RapidPro.Core;
using UnityEngine;

namespace RapidPro.Scenes
{
    /// <summary>
    /// Default <see cref="ISceneFlow"/>. The Bootstrap scene stays loaded the whole time, so its services
    /// never need <c>DontDestroyOnLoad</c>.
    /// </summary>
    /// <remarks>
    /// Order of a normal start:
    /// <list type="number">
    /// <item>Load the Loading scene, if one is set.</item>
    /// <item>Load Game, then GameUI, on top. <see cref="Progress"/> goes from 0 to 1 across both.</item>
    /// <item>Make Game the active scene, so spawned objects land in it.</item>
    /// <item>Wait out the minimum loading time, then unload Loading.</item>
    /// </list>
    /// Register it in your <c>GameInstaller</c>:
    /// <c>Services.Register&lt;ISceneFlow&gt;(new SceneFlow(_sceneFlowSettings));</c>
    /// </remarks>
    public class SceneFlow : ISceneFlow, IDisposable
    {
        private readonly SceneFlowSettings _settings;
        private readonly ISceneLoader _loader;
        private readonly ReactiveProperty<float> _progress = new ReactiveProperty<float>(0f);
        private readonly ReactiveProperty<bool> _isLoading = new ReactiveProperty<bool>(false);

        /// <summary>Creates the flow.</summary>
        /// <param name="settings">Scene names and loading options.</param>
        /// <param name="loader">How scenes are loaded. Leave null for Unity's scene manager.</param>
        public SceneFlow(SceneFlowSettings settings, ISceneLoader loader = null)
        {
            _settings = settings != null ? settings : throw new ArgumentNullException(nameof(settings));
            _loader = loader ?? new UnitySceneLoader();
        }

        /// <inheritdoc />
        public ReadOnlyReactiveProperty<float> Progress => _progress;

        /// <inheritdoc />
        public ReadOnlyReactiveProperty<bool> IsLoading => _isLoading;

        /// <inheritdoc />
        public async UniTask StartGameAsync(string startScene = null, CancellationToken cancellationToken = default)
        {
            if (_isLoading.Value)
            {
                Log.Warning("SceneFlow: StartGameAsync was called while the game is already loading. Ignored.");
                return;
            }

            _isLoading.Value = true;
            _progress.Value = 0f;
            float startTime = Time.realtimeSinceStartup;

            try
            {
                if (_settings.HasLoadingScene)
                    await _loader.LoadAdditiveAsync(_settings.LoadingScene, null, cancellationToken);

                var scenes = GetScenesToLoad(startScene);
                for (int i = 0; i < scenes.Count; i++)
                {
                    int index = i;
                    var stepProgress = new StepProgress(p => _progress.Value = (index + p) / scenes.Count);
                    await _loader.LoadAdditiveAsync(scenes[i], stepProgress, cancellationToken);
                    _progress.Value = (float)(i + 1) / scenes.Count;
                }

                _loader.SetActive(scenes[0]);

                if (_settings.HasLoadingScene)
                {
                    float remaining = _settings.MinimumLoadingSeconds - (Time.realtimeSinceStartup - startTime);
                    if (remaining > 0f)
                        await UniTask.Delay(TimeSpan.FromSeconds(remaining), ignoreTimeScale: true, cancellationToken: cancellationToken);

                    await _loader.UnloadAsync(_settings.LoadingScene, cancellationToken);
                }

                _progress.Value = 1f;
            }
            finally
            {
                _isLoading.Value = false;
            }
        }

        /// <summary>Completes <see cref="Progress"/> and <see cref="IsLoading"/>.</summary>
        public void Dispose()
        {
            _progress.Dispose();
            _isLoading.Dispose();
        }

        private List<string> GetScenesToLoad(string startScene)
        {
            if (!string.IsNullOrEmpty(startScene))
                return new List<string> { startScene };

            return new List<string> { _settings.GameScene, _settings.GameUIScene };
        }

        // Reports straight away. System.Progress<T> would post each value to a later frame instead.
        private class StepProgress : IProgress<float>
        {
            private readonly Action<float> _onReport;

            public StepProgress(Action<float> onReport) => _onReport = onReport;

            public void Report(float value) => _onReport(value);
        }
    }
}

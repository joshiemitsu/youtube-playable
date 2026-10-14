using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace RapidPro.Scenes.Tests
{
    /// <summary>
    /// Edit-mode tests for <see cref="SceneFlow"/> and <see cref="PlayFromAnyScene"/>.
    /// A fake loader records what would be loaded, so no real scenes are touched.
    /// </summary>
    public class SceneFlowTests
    {
        /// <summary>Records every call. Scenes listed in <see cref="Hold"/> wait until released.</summary>
        private class FakeSceneLoader : ISceneLoader
        {
            public readonly List<string> Calls = new List<string>();
            public readonly Dictionary<string, UniTaskCompletionSource> Hold = new Dictionary<string, UniTaskCompletionSource>();

            public UniTask LoadAdditiveAsync(string scene, IProgress<float> progress, CancellationToken cancellationToken)
            {
                Calls.Add($"load {scene}");
                progress?.Report(0.5f);
                return Hold.TryGetValue(scene, out var hold) ? hold.Task : UniTask.CompletedTask;
            }

            public UniTask UnloadAsync(string scene, CancellationToken cancellationToken)
            {
                Calls.Add($"unload {scene}");
                return UniTask.CompletedTask;
            }

            public void SetActive(string scene) => Calls.Add($"active {scene}");
        }

        private SceneFlowSettings _settings;
        private FakeSceneLoader _loader;
        private SceneFlow _flow;

        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<SceneFlowSettings>();
            _settings.Configure("Bootstrap", "Loading", "Game", "GameUI", minimumLoadingSeconds: 0f);
            _loader = new FakeSceneLoader();
            _flow = new SceneFlow(_settings, _loader);
        }

        [TearDown]
        public void TearDown()
        {
            _flow.Dispose();
            Object.DestroyImmediate(_settings);
        }

        [Test]
        public void StartGame_LoadsInTheRightOrder()
        {
            _flow.StartGameAsync().Forget();

            CollectionAssert.AreEqual(new[]
            {
                "load Loading",
                "load Game",
                "load GameUI",
                "active Game",
                "unload Loading"
            }, _loader.Calls);
        }

        [Test]
        public void StartGame_WithoutLoadingScene_SkipsIt()
        {
            _settings.Configure("Bootstrap", "", "Game", "GameUI", 0f);

            _flow.StartGameAsync().Forget();

            CollectionAssert.AreEqual(new[] { "load Game", "load GameUI", "active Game" }, _loader.Calls);
        }

        [Test]
        public void StartGame_WithStartScene_LoadsOnlyThatScene()
        {
            const string sandbox = "Assets/Game/Sandbox/MechanicSandbox.unity";

            _flow.StartGameAsync(sandbox).Forget();

            CollectionAssert.AreEqual(new[]
            {
                "load Loading",
                $"load {sandbox}",
                $"active {sandbox}",
                "unload Loading"
            }, _loader.Calls);
        }

        [Test]
        public void Progress_CountsAcrossBothScenes()
        {
            var holdGame = new UniTaskCompletionSource();
            _loader.Hold["Game"] = holdGame;

            _flow.StartGameAsync().Forget();

            // Game reported 0.5 of the first of two scenes.
            Assert.AreEqual(0.25f, _flow.Progress.CurrentValue, 0.0001f);
            Assert.IsTrue(_flow.IsLoading.CurrentValue);

            holdGame.TrySetResult();

            Assert.AreEqual(1f, _flow.Progress.CurrentValue, 0.0001f);
            Assert.IsFalse(_flow.IsLoading.CurrentValue);
        }

        [Test]
        public void StartGame_WhileLoading_IsIgnoredWithWarning()
        {
            var holdGame = new UniTaskCompletionSource();
            _loader.Hold["Game"] = holdGame;
            _flow.StartGameAsync().Forget();
            LogAssert.Expect(LogType.Warning, new Regex("already loading"));

            _flow.StartGameAsync().Forget();

            Assert.AreEqual(1, _loader.Calls.FindAll(call => call == "load Game").Count);
            holdGame.TrySetResult();
        }

        [Test]
        public void IsStartupScene_KnowsTheFourScenes()
        {
            Assert.IsTrue(_settings.IsStartupScene("Bootstrap"));
            Assert.IsTrue(_settings.IsStartupScene("Loading"));
            Assert.IsTrue(_settings.IsStartupScene("Game"));
            Assert.IsTrue(_settings.IsStartupScene("GameUI"));
            Assert.IsFalse(_settings.IsStartupScene("LevelEditor"));
        }

        [Test]
        public void PlayFromAnyScene_StartupScene_StartsNormally()
        {
            Assert.IsNull(PlayFromAnyScene.ResolveRequestedScene("Game", "Assets/Scenes/Game.unity", _settings));
            Assert.IsNull(PlayFromAnyScene.ResolveRequestedScene("Bootstrap", "Assets/Scenes/Bootstrap.unity", _settings));
        }

        [Test]
        public void PlayFromAnyScene_OtherScene_IsRemembered()
        {
            const string path = "Assets/Scenes/TestBed/LevelEditor.unity";

            Assert.AreEqual(path, PlayFromAnyScene.ResolveRequestedScene("LevelEditor", path, _settings));
        }

        [Test]
        public void PlayFromAnyScene_UnsavedScene_StartsNormally()
        {
            Assert.IsNull(PlayFromAnyScene.ResolveRequestedScene("Untitled", "", _settings));
        }
    }
}

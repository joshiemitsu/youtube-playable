using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using R3;
using RapidPro.Events;
using RapidPro.Save;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace RapidPro.GameLoop.Tests
{
    /// <summary>
    /// Edit-mode tests for <see cref="GameLoop"/> with a fake game mode that records what the loop asks of it.
    /// </summary>
    public class GameLoopTests
    {
        private class FakeGameMode : IGameMode
        {
            public readonly List<string> Calls = new List<string>();
            public LevelData LastLevel;

            public UniTask PrepareAsync(LevelData level, CancellationToken cancellationToken)
            {
                LastLevel = level;
                Calls.Add($"prepare {level.name}");
                return UniTask.CompletedTask;
            }

            public void Begin() => Calls.Add("begin");
            public void Cleanup() => Calls.Add("cleanup");
        }

        private FakeGameMode _mode;
        private LevelCatalog _catalog;
        private TestLevel _first;
        private TestLevel _second;
        private SaveService _save;
        private string _savePath;
        private EventBus _events;
        private GameLoop _loop;

        [SetUp]
        public void SetUp()
        {
            _mode = new FakeGameMode();
            _first = ScriptableObject.CreateInstance<TestLevel>();
            _first.name = "First";
            _second = ScriptableObject.CreateInstance<TestLevel>();
            _second.name = "Second";
            _catalog = ScriptableObject.CreateInstance<LevelCatalog>();
            _catalog.Add(_first);
            _catalog.Add(_second);

            _savePath = Path.Combine(Application.temporaryCachePath, $"gameloop-test-{Guid.NewGuid():N}.json");
            _save = new SaveService(_savePath);
            _events = new EventBus();
            _loop = new GameLoop(_mode, _catalog, _save, _events);
        }

        [TearDown]
        public void TearDown()
        {
            _loop.Dispose();
            _events.Dispose();
            _save.DeleteAll();
            _save.Dispose();
            Object.DestroyImmediate(_first);
            Object.DestroyImmediate(_second);
            Object.DestroyImmediate(_catalog);
        }

        [Test]
        public void NewLoop_IsHome()
        {
            Assert.AreEqual(GameState.Home, _loop.State.CurrentValue);
        }

        [Test]
        public void Start_CleansPreparesAndBeginsFirstLevel()
        {
            _loop.StartAsync().Forget();

            Assert.AreEqual(GameState.Playing, _loop.State.CurrentValue);
            CollectionAssert.AreEqual(new[] { "cleanup", "prepare First", "begin" }, _mode.Calls);
        }

        [Test]
        public void Win_WhilePlaying_ChangesStateAndSavesNextLevel()
        {
            var completed = new List<int>();
            using (_events.On<LevelCompleted>().Subscribe(e => completed.Add(e.LevelIndex)))
            {
                _loop.StartAsync().Forget();
                _loop.Win();
            }

            Assert.AreEqual(GameState.Win, _loop.State.CurrentValue);
            Assert.AreEqual(1, _save.Load(GameLoop.LevelSaveKey, 0), "The next level is saved.");
            CollectionAssert.AreEqual(new[] { 0 }, completed);
        }

        [Test]
        public void Lose_WhilePlaying_ChangesStateAndPublishes()
        {
            var failed = new List<int>();
            using (_events.On<LevelFailed>().Subscribe(e => failed.Add(e.LevelIndex)))
            {
                _loop.StartAsync().Forget();
                _loop.Lose();
            }

            Assert.AreEqual(GameState.Lose, _loop.State.CurrentValue);
            Assert.AreEqual(1, failed.Count);
        }

        [Test]
        public void Win_BeforePlaying_IsIgnored()
        {
            LogAssert.Expect(LogType.Warning, new Regex("ignored Win"));

            _loop.Win();

            Assert.AreEqual(GameState.Home, _loop.State.CurrentValue);
        }

        [Test]
        public void Lose_AfterWin_IsIgnored()
        {
            _loop.StartAsync().Forget();
            _loop.Win();
            LogAssert.Expect(LogType.Warning, new Regex("ignored Lose"));

            _loop.Lose();

            Assert.AreEqual(GameState.Win, _loop.State.CurrentValue);
        }

        [Test]
        public void Restart_AfterLose_CleansAndReplaysSameLevel()
        {
            _loop.StartAsync().Forget();
            _loop.Lose();
            _mode.Calls.Clear();

            _loop.RestartAsync().Forget();

            Assert.AreEqual(GameState.Playing, _loop.State.CurrentValue);
            CollectionAssert.AreEqual(new[] { "cleanup", "prepare First", "begin" }, _mode.Calls);
        }

        [Test]
        public void NextLevel_PlaysNextAndLoopsAfterTheLast()
        {
            _loop.StartAsync().Forget();
            _loop.Win();

            _loop.NextLevelAsync().Forget();
            Assert.AreSame(_second, _mode.LastLevel);

            _loop.Win();
            _loop.NextLevelAsync().Forget();
            Assert.AreSame(_first, _mode.LastLevel, "After the last level the catalog starts again.");
        }

        [Test]
        public void Start_ContinuesFromSavedLevel()
        {
            _save.Save(GameLoop.LevelSaveKey, 1);

            _loop.StartAsync().Forget();

            Assert.AreEqual(1, _loop.LevelIndex.CurrentValue);
            Assert.AreSame(_second, _mode.LastLevel);
        }
    }
}

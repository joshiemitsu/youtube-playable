using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using R3;
using UnityEngine;
using UnityEngine.TestTools;

namespace RapidPro.StateMachine.Tests
{
    /// <summary>
    /// Edit-mode tests for <see cref="StateMachine{TStateId}"/>.
    /// </summary>
    public class StateMachineTests
    {
        private enum GameState { Home, Playing, Win, Lose }

        /// <summary>Instant state that writes every call to a shared log.</summary>
        private class RecordingState : State
        {
            private readonly string _name;
            private readonly List<string> _log;

            public RecordingState(string name, List<string> log)
            {
                _name = name;
                _log = log;
            }

            public int TickCount { get; private set; }

            public override void Tick(float deltaTime) => TickCount++;
            public override void Exit() => _log.Add($"{_name}.Exit");
            protected override void Enter() => _log.Add($"{_name}.Enter");
        }

        /// <summary>State whose enter only finishes when the test says so, like an animation.</summary>
        private class AnimatedState : State
        {
            private readonly UniTaskCompletionSource _finish = new UniTaskCompletionSource();

            public override UniTask EnterAsync(CancellationToken cancellationToken) => _finish.Task;

            public void FinishEnter() => _finish.TrySetResult();
        }

        private List<string> _log;
        private RecordingState _home;
        private RecordingState _playing;
        private AnimatedState _win;
        private StateMachine<GameState> _machine;

        [SetUp]
        public void SetUp()
        {
            _log = new List<string>();
            _home = new RecordingState("Home", _log);
            _playing = new RecordingState("Playing", _log);
            _win = new AnimatedState();

            _machine = new StateMachine<GameState>();
            _machine.Register(GameState.Home, _home);
            _machine.Register(GameState.Playing, _playing);
            _machine.Register(GameState.Win, _win);
        }

        [TearDown]
        public void TearDown()
        {
            _machine.Dispose();
        }

        [Test]
        public void NewMachine_HasNoState()
        {
            Assert.IsFalse(_machine.HasState);
            Assert.IsFalse(_machine.IsChanging);
        }

        [Test]
        public void ChangeState_EntersStateAndUpdatesCurrent()
        {
            _machine.ChangeStateAsync(GameState.Home).Forget();

            Assert.IsTrue(_machine.HasState);
            Assert.AreEqual(GameState.Home, _machine.CurrentState.CurrentValue);
            CollectionAssert.AreEqual(new[] { "Home.Enter" }, _log);
        }

        [Test]
        public void ChangeState_ExitsOldStateBeforeEnteringNew()
        {
            _machine.ChangeStateAsync(GameState.Home).Forget();

            _machine.ChangeStateAsync(GameState.Playing).Forget();

            CollectionAssert.AreEqual(new[] { "Home.Enter", "Home.Exit", "Playing.Enter" }, _log);
        }

        [Test]
        public void CurrentState_EmitsEachChange()
        {
            var seen = new List<GameState>();
            _machine.ChangeStateAsync(GameState.Home).Forget();
            using (_machine.CurrentState.Subscribe(seen.Add))
            {
                _machine.ChangeStateAsync(GameState.Playing).Forget();
            }

            CollectionAssert.AreEqual(new[] { GameState.Home, GameState.Playing }, seen);
        }

        [Test]
        public void Tick_OnlyTicksCurrentState()
        {
            _machine.ChangeStateAsync(GameState.Home).Forget();
            _machine.ChangeStateAsync(GameState.Playing).Forget();

            _machine.Tick(0.016f);

            Assert.AreEqual(0, _home.TickCount);
            Assert.AreEqual(1, _playing.TickCount);
        }

        [Test]
        public void ChangeState_ToSameState_ExitsAndReentersAndEmits()
        {
            var seen = new List<GameState>();
            _machine.ChangeStateAsync(GameState.Playing).Forget();
            using (_machine.CurrentState.Subscribe(seen.Add))
            {
                _machine.ChangeStateAsync(GameState.Playing).Forget();
            }

            CollectionAssert.AreEqual(new[] { "Playing.Enter", "Playing.Exit", "Playing.Enter" }, _log);
            CollectionAssert.AreEqual(new[] { GameState.Playing, GameState.Playing }, seen);
        }

        [Test]
        public void ChangeState_DuringAnimatedEnter_IsIgnoredWithWarning()
        {
            _machine.ChangeStateAsync(GameState.Playing).Forget();
            var toWin = _machine.ChangeStateAsync(GameState.Win);
            Assert.IsTrue(_machine.IsChanging);

            LogAssert.Expect(LogType.Warning, new Regex("ignored change to Home"));
            _machine.ChangeStateAsync(GameState.Home).Forget();

            Assert.AreEqual(GameState.Win, _machine.CurrentState.CurrentValue);

            _win.FinishEnter();
            Assert.AreEqual(UniTaskStatus.Succeeded, toWin.Status);
            Assert.IsFalse(_machine.IsChanging);
        }

        [Test]
        public void Tick_DuringAnimatedEnter_DoesNothing()
        {
            _machine.ChangeStateAsync(GameState.Playing).Forget();
            _machine.ChangeStateAsync(GameState.Win).Forget();

            _machine.Tick(0.016f);

            Assert.AreEqual(0, _playing.TickCount);
            _win.FinishEnter();
        }

        [Test]
        public void ChangeState_UnregisteredState_Throws()
        {
            Assert.Throws<System.ArgumentException>(() => _machine.ChangeStateAsync(GameState.Lose));
        }
    }
}

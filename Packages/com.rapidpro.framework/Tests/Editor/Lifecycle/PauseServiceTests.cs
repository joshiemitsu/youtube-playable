using System.Collections.Generic;
using NUnit.Framework;
using R3;
using UnityEngine;

namespace RapidPro.Lifecycle.Tests
{
    /// <summary>
    /// Edit-mode tests for <see cref="PauseService"/>. Each test resets <c>Time.timeScale</c> afterwards.
    /// </summary>
    public class PauseServiceTests
    {
        private PauseService _pause;

        [SetUp]
        public void SetUp()
        {
            Time.timeScale = 1f;
            _pause = new PauseService();
        }

        [TearDown]
        public void TearDown()
        {
            _pause.Dispose();
            Time.timeScale = 1f;
        }

        [Test]
        public void NewService_IsNotPaused()
        {
            Assert.IsFalse(_pause.IsPaused.CurrentValue);
        }

        [Test]
        public void Pause_SetsIsPausedAndStopsTime()
        {
            _pause.Pause(PauseReasons.Popup);

            Assert.IsTrue(_pause.IsPaused.CurrentValue);
            Assert.IsTrue(_pause.IsPausedFor(PauseReasons.Popup));
            Assert.AreEqual(0f, Time.timeScale);
        }

        [Test]
        public void Resume_RestoresPreviousTimeScale()
        {
            Time.timeScale = 0.5f;
            _pause.Pause(PauseReasons.Popup);

            _pause.Resume(PauseReasons.Popup);

            Assert.IsFalse(_pause.IsPaused.CurrentValue);
            Assert.AreEqual(0.5f, Time.timeScale);
        }

        [Test]
        public void TwoReasons_StayPausedUntilBothResume()
        {
            _pause.Pause(PauseReasons.Player);
            _pause.Pause(PauseReasons.Popup);

            _pause.Resume(PauseReasons.Popup);
            Assert.IsTrue(_pause.IsPaused.CurrentValue);
            Assert.AreEqual(0f, Time.timeScale);

            _pause.Resume(PauseReasons.Player);
            Assert.IsFalse(_pause.IsPaused.CurrentValue);
            Assert.AreEqual(1f, Time.timeScale);
        }

        [Test]
        public void SameReasonTwice_OneResumeIsEnough()
        {
            _pause.Pause(PauseReasons.Popup);
            _pause.Pause(PauseReasons.Popup);

            _pause.Resume(PauseReasons.Popup);

            Assert.IsFalse(_pause.IsPaused.CurrentValue);
            Assert.AreEqual(1f, Time.timeScale);
        }

        [Test]
        public void Resume_UnknownReason_DoesNothing()
        {
            _pause.Pause(PauseReasons.Popup);

            _pause.Resume(PauseReasons.Debug);

            Assert.IsTrue(_pause.IsPaused.CurrentValue);
        }

        [Test]
        public void IsPaused_EmitsChanges()
        {
            var seen = new List<bool>();
            using (_pause.IsPaused.Subscribe(seen.Add))
            {
                _pause.Pause(PauseReasons.Popup);
                _pause.Resume(PauseReasons.Popup);
            }

            CollectionAssert.AreEqual(new[] { false, true, false }, seen);
        }
    }
}

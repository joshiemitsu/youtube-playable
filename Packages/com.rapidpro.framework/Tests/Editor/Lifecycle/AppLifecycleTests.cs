using System;
using System.IO;
using NUnit.Framework;
using R3;
using RapidPro.Save;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RapidPro.Lifecycle.Tests
{
    /// <summary>
    /// Edit-mode tests for <see cref="AppLifecycle"/>. Going to the background is simulated
    /// by calling the same method Unity's <c>OnApplicationPause</c> calls.
    /// </summary>
    public class AppLifecycleTests
    {
        private GameObject _gameObject;
        private AppLifecycle _lifecycle;
        private PauseService _pause;
        private SaveService _save;
        private string _savePath;

        [SetUp]
        public void SetUp()
        {
            Time.timeScale = 1f;
            _savePath = Path.Combine(Application.temporaryCachePath, $"lifecycle-test-{Guid.NewGuid():N}.json");
            _save = new SaveService(_savePath);
            _pause = new PauseService();

            _gameObject = new GameObject("Lifecycle Test");
            _lifecycle = _gameObject.AddComponent<AppLifecycle>();
            _lifecycle.Connect(_pause, _save);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gameObject);
            _save.DeleteAll();
            _save.Dispose();
            _pause.Dispose();
            Time.timeScale = 1f;
        }

        [Test]
        public void GoingToBackground_PausesSavesAndNotifies()
        {
            _save.Save("progress.level", 4);
            int notified = 0;
            using (_lifecycle.WentToBackground.Subscribe(_ => notified++))
            {
                _lifecycle.HandleApplicationPause(true);
            }

            Assert.IsTrue(_pause.IsPausedFor(PauseReasons.Background));
            Assert.IsTrue(File.Exists(_savePath), "The save should be flushed to disk.");
            Assert.AreEqual(1, notified);
        }

        [Test]
        public void GoingToBackgroundTwice_NotifiesOnce()
        {
            int notified = 0;
            using (_lifecycle.WentToBackground.Subscribe(_ => notified++))
            {
                _lifecycle.HandleApplicationPause(true);
                _lifecycle.HandleApplicationPause(true);
            }

            Assert.AreEqual(1, notified);
        }

        [Test]
        public void Returning_WithResumeOnReturn_ResumesAndNotifies()
        {
            int notified = 0;
            _lifecycle.HandleApplicationPause(true);
            using (_lifecycle.ReturnedFromBackground.Subscribe(_ => notified++))
            {
                _lifecycle.HandleApplicationPause(false);
            }

            Assert.IsFalse(_pause.IsPaused.CurrentValue);
            Assert.AreEqual(1, notified);
        }

        [Test]
        public void Returning_WithoutResumeOnReturn_StaysPaused()
        {
            _lifecycle.ResumeOnReturn = false;
            _lifecycle.HandleApplicationPause(true);

            _lifecycle.HandleApplicationPause(false);

            Assert.IsTrue(_pause.IsPausedFor(PauseReasons.Background));
        }

        [Test]
        public void Returning_KeepsOtherPauseReasons()
        {
            _pause.Pause(PauseReasons.Player);
            _lifecycle.HandleApplicationPause(true);

            _lifecycle.HandleApplicationPause(false);

            Assert.IsTrue(_pause.IsPaused.CurrentValue);
            Assert.IsTrue(_pause.IsPausedFor(PauseReasons.Player));
        }

        [Test]
        public void BackPressed_Notifies()
        {
            int notified = 0;
            using (_lifecycle.BackPressed.Subscribe(_ => notified++))
            {
                _lifecycle.HandleBackPressed();
            }

            Assert.AreEqual(1, notified);
        }
    }
}

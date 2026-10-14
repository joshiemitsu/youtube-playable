using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

namespace RapidPro.Lifecycle
{
    /// <summary>
    /// Default <see cref="IPauseService"/>. Pausing sets <c>Time.timeScale</c> to 0; resuming restores
    /// the value it had before. Code that must keep running while paused (UI animations, music fades)
    /// should use unscaled time.
    /// </summary>
    /// <remarks>
    /// Register it in your <c>GameInstaller</c>:
    /// <c>Services.Register&lt;IPauseService&gt;(new PauseService());</c>
    /// </remarks>
    public class PauseService : IPauseService, IDisposable
    {
        private readonly HashSet<string> _reasons = new HashSet<string>();
        private readonly ReactiveProperty<bool> _isPaused = new ReactiveProperty<bool>(false);
        private float _timeScaleBeforePause = 1f;

        /// <inheritdoc />
        public ReadOnlyReactiveProperty<bool> IsPaused => _isPaused;

        /// <inheritdoc />
        public void Pause(string reason)
        {
            if (string.IsNullOrEmpty(reason))
                throw new ArgumentException("Pause needs a reason, e.g. PauseReasons.Popup.", nameof(reason));

            if (!_reasons.Add(reason) || _reasons.Count > 1)
                return;

            _timeScaleBeforePause = Time.timeScale;
            Time.timeScale = 0f;
            _isPaused.Value = true;
        }

        /// <inheritdoc />
        public void Resume(string reason)
        {
            if (string.IsNullOrEmpty(reason) || !_reasons.Remove(reason) || _reasons.Count > 0)
                return;

            Time.timeScale = _timeScaleBeforePause;
            _isPaused.Value = false;
        }

        /// <inheritdoc />
        public bool IsPausedFor(string reason) => reason != null && _reasons.Contains(reason);

        /// <summary>Completes <see cref="IsPaused"/>.</summary>
        public void Dispose()
        {
            _isPaused.Dispose();
        }
    }
}

using System.Collections.Generic;

namespace RapidPro.Audio
{
    /// <summary>
    /// Remembers when each cue last played and blocks it until its <see cref="AudioCue.MinGap"/> has passed.
    /// Kept here, not on the cue asset, so the timing resets every Play session.
    /// </summary>
    internal class SfxGapLimiter
    {
        private readonly Dictionary<AudioCue, float> _lastPlayed = new Dictionary<AudioCue, float>();

        /// <summary>Returns true and records the time when <paramref name="cue"/> may play now.</summary>
        public bool TryPlay(AudioCue cue, float now)
        {
            if (_lastPlayed.TryGetValue(cue, out var last) && now - last < cue.MinGap)
                return false;

            _lastPlayed[cue] = now;
            return true;
        }
    }
}

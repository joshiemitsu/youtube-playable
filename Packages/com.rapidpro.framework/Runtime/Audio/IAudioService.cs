using UnityEngine;

namespace RapidPro.Audio
{
    /// <summary>
    /// Plays music and sound effects. Used by gameplay and UI.
    /// </summary>
    /// <example>
    /// <code>
    /// var audio = Services.Get&lt;IAudioService&gt;();
    ///
    /// audio.PlayBgm(_gameMusic);                  // crossfades from the current track
    /// audio.PlaySfx(_tapCue);                     // 2D, e.g. UI clicks
    /// audio.PlaySfx(_exitCue, arrow.position);    // 3D at a world position
    /// </code>
    /// </example>
    public interface IAudioService
    {
        /// <summary>
        /// Crossfades to <paramref name="cue"/> as background music. Does nothing if it is already playing.
        /// </summary>
        /// <param name="cue">The music cue. Loops until changed or stopped.</param>
        /// <param name="fade">Crossfade length in seconds.</param>
        void PlayBgm(AudioCue cue, float fade = 0.5f);

        /// <summary>Fades out and stops the background music.</summary>
        /// <param name="fade">Fade length in seconds.</param>
        void StopBgm(float fade = 0.5f);

        /// <summary>Plays a 2D sound effect, e.g. a UI click.</summary>
        void PlaySfx(AudioCue cue);

        /// <summary>Plays a 3D sound effect at a world position.</summary>
        void PlaySfx(AudioCue cue, Vector3 position);
    }
}

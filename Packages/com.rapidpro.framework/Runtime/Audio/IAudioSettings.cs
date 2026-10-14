using R3;

namespace RapidPro.Audio
{
    /// <summary>
    /// Music and SFX volume, for the Settings screen only. Gameplay uses <see cref="IAudioService"/>.
    /// </summary>
    /// <remarks>
    /// Volumes live in memory for now and reset on restart. The Save module will persist them later,
    /// without changing this interface.
    /// </remarks>
    /// <example>
    /// <code>
    /// _settings.MusicVolume.Subscribe(v => _musicSlider.SetValueWithoutNotify(v)).AddTo(this);
    /// _musicSlider.onValueChanged.AddListener(_settings.SetMusicVolume);
    /// </code>
    /// </example>
    public interface IAudioSettings
    {
        /// <summary>Music volume, 0 to 1.</summary>
        ReadOnlyReactiveProperty<float> MusicVolume { get; }

        /// <summary>Sound effect volume, 0 to 1.</summary>
        ReadOnlyReactiveProperty<float> SfxVolume { get; }

        /// <summary>Sets the music volume. Values outside 0 to 1 are clamped.</summary>
        void SetMusicVolume(float value);

        /// <summary>Sets the SFX volume. Values outside 0 to 1 are clamped.</summary>
        void SetSfxVolume(float value);
    }
}

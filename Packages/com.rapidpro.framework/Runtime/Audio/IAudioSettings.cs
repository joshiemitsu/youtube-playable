using R3;

namespace RapidPro.Audio
{
    /// <summary>
    /// Music and SFX volume, for the Settings screen only. Gameplay uses <see cref="IAudioService"/>.
    /// </summary>
    /// <remarks>
    /// Volumes are saved when the <see cref="AudioManager"/> is connected to a save service with
    /// <see cref="AudioManager.UseSave"/>. Without it they live in memory and reset on restart.
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

using R3;
using RapidPro.Core;
using UnityEngine;

namespace RapidPro.Audio
{
    /// <summary>
    /// Plays music and sound effects. Put it in the Bootstrap scene so music keeps playing between levels.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Music (BGM) crossfades between two sources.</item>
    /// <item>Sound effects (SFX) play from up to 8 sources, in 2D or 3D, and respect each cue's minimum gap.</item>
    /// <item>Volumes live in memory for now; the Save module will persist them.</item>
    /// </list>
    /// Register it once for each interface in your <c>GameInstaller</c>:
    /// <code>
    /// [SerializeField] private AudioManager _audio;
    ///
    /// Services.Register&lt;IAudioService&gt;(_audio);
    /// Services.Register&lt;IAudioSettings&gt;(_audio);
    /// </code>
    /// </remarks>
    public class AudioManager : MonoBehaviour, IAudioService, IAudioSettings
    {
        private readonly ReactiveProperty<float> _musicVolume = new ReactiveProperty<float>(1f);
        private readonly ReactiveProperty<float> _sfxVolume = new ReactiveProperty<float>(1f);
        private readonly SfxGapLimiter _gapLimiter = new SfxGapLimiter();

        private BgmChannel _bgmA;
        private BgmChannel _bgmB;
        private BgmChannel _activeBgm;
        private SfxSourcePool _sfxPool;

        /// <inheritdoc />
        public ReadOnlyReactiveProperty<float> MusicVolume => _musicVolume;

        /// <inheritdoc />
        public ReadOnlyReactiveProperty<float> SfxVolume => _sfxVolume;

        /// <summary>The music channel currently playing or fading in. For tests.</summary>
        internal BgmChannel ActiveBgm
        {
            get { EnsureInitialized(); return _activeBgm; }
        }

        /// <summary>The sound effect sources. For tests.</summary>
        internal SfxSourcePool SfxPool
        {
            get { EnsureInitialized(); return _sfxPool; }
        }

        /// <inheritdoc />
        public void PlayBgm(AudioCue cue, float fade = 0.5f)
        {
            EnsureInitialized();

            if (cue == null)
            {
                Log.Warning("AudioManager: PlayBgm was called without a cue.", this);
                return;
            }

            if (_activeBgm.Cue == cue)
                return;

            var clip = cue.PickClip();
            if (clip == null)
            {
                Log.Warning($"AudioManager: cue '{cue.name}' has no clips.", cue);
                return;
            }

            var next = _activeBgm == _bgmA ? _bgmB : _bgmA;
            _activeBgm.FadeOut(fade, _musicVolume.Value);
            next.Play(cue, clip, fade, _musicVolume.Value);
            _activeBgm = next;
        }

        /// <inheritdoc />
        public void StopBgm(float fade = 0.5f)
        {
            EnsureInitialized();
            _activeBgm.FadeOut(fade, _musicVolume.Value);
        }

        /// <inheritdoc />
        public void PlaySfx(AudioCue cue)
        {
            PlaySfx(cue, is3D: false, Vector3.zero);
        }

        /// <inheritdoc />
        public void PlaySfx(AudioCue cue, Vector3 position)
        {
            PlaySfx(cue, is3D: true, position);
        }

        /// <inheritdoc />
        public void SetMusicVolume(float value)
        {
            EnsureInitialized();
            _musicVolume.Value = Mathf.Clamp01(value);
            _bgmA.ApplyVolume(_musicVolume.Value);
            _bgmB.ApplyVolume(_musicVolume.Value);
        }

        /// <summary>
        /// Sets the SFX volume. Values outside 0 to 1 are clamped. Applies to sounds started after the change.
        /// </summary>
        public void SetSfxVolume(float value)
        {
            _sfxVolume.Value = Mathf.Clamp01(value);
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        private void Update()
        {
            if (_bgmA == null)
                return;

            float deltaTime = Time.unscaledDeltaTime;
            _bgmA.Tick(deltaTime, _musicVolume.Value);
            _bgmB.Tick(deltaTime, _musicVolume.Value);
        }

        private void OnDestroy()
        {
            _musicVolume.Dispose();
            _sfxVolume.Dispose();
        }

        private void PlaySfx(AudioCue cue, bool is3D, Vector3 position)
        {
            EnsureInitialized();

            if (cue == null)
            {
                Log.Warning("AudioManager: PlaySfx was called without a cue.", this);
                return;
            }

            var clip = cue.PickClip();
            if (clip == null)
            {
                Log.Warning($"AudioManager: cue '{cue.name}' has no clips.", cue);
                return;
            }

            float now = Time.realtimeSinceStartup;
            if (!_gapLimiter.TryPlay(cue, now))
                return;

            float pitch = cue.PickPitch();
            var source = _sfxPool.Get(now, clip.length / pitch);

            source.Stop();
            source.clip = clip;
            source.pitch = pitch;
            source.volume = cue.Volume * _sfxVolume.Value;
            source.spatialBlend = is3D ? 1f : 0f;
            source.transform.position = is3D ? position : transform.position;
            source.Play();
        }

        // Builds the sources on first use, so the manager also works when Awake has not run (e.g. in edit-mode tests).
        private void EnsureInitialized()
        {
            if (_bgmA != null)
                return;

            _bgmA = new BgmChannel(CreateChildSource("BGM A"));
            _bgmB = new BgmChannel(CreateChildSource("BGM B"));
            _activeBgm = _bgmA;
            _sfxPool = new SfxSourcePool(transform);
        }

        private AudioSource CreateChildSource(string sourceName)
        {
            var go = new GameObject(sourceName);
            go.transform.SetParent(transform, false);
            return go.AddComponent<AudioSource>();
        }
    }
}

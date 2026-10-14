using UnityEngine;

namespace RapidPro.Audio
{
    /// <summary>
    /// One looping music source that can fade in and out. <see cref="AudioManager"/> keeps two and
    /// crossfades between them.
    /// </summary>
    internal class BgmChannel
    {
        private readonly AudioSource _source;
        private float _cueVolume;
        private float _fade;
        private float _fadeFrom;
        private float _fadeTo;
        private float _fadeDuration;
        private float _fadeTime;
        private bool _stopWhenFaded;

        public BgmChannel(AudioSource source)
        {
            _source = source;
            _source.loop = true;
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
        }

        /// <summary>The cue playing on this channel, or null when it is idle or fading out.</summary>
        public AudioCue Cue { get; private set; }

        /// <summary>The Unity source behind this channel.</summary>
        public AudioSource Source => _source;

        /// <summary>Starts <paramref name="clip"/> from silence and fades it in.</summary>
        public void Play(AudioCue cue, AudioClip clip, float fadeIn, float musicVolume)
        {
            Cue = cue;
            _cueVolume = cue.Volume;
            _source.clip = clip;
            _source.pitch = 1f;
            _fade = 0f;
            _source.Play();
            FadeTo(1f, fadeIn, stopWhenFaded: false);
            ApplyVolume(musicVolume);
        }

        /// <summary>Fades to silence, then stops.</summary>
        public void FadeOut(float duration, float musicVolume)
        {
            Cue = null;
            FadeTo(0f, duration, stopWhenFaded: true);
            ApplyVolume(musicVolume);
        }

        /// <summary>Moves the fade forward. Called every frame by <see cref="AudioManager"/>.</summary>
        public void Tick(float deltaTime, float musicVolume)
        {
            if (_fadeTime < _fadeDuration)
            {
                _fadeTime += deltaTime;
                _fade = Mathf.Lerp(_fadeFrom, _fadeTo, _fadeTime / _fadeDuration);

                if (_fadeTime >= _fadeDuration)
                    FinishFade();
            }

            ApplyVolume(musicVolume);
        }

        /// <summary>Sets the source volume from the cue volume, the fade and the player's music volume.</summary>
        public void ApplyVolume(float musicVolume)
        {
            _source.volume = _cueVolume * _fade * musicVolume;
        }

        private void FadeTo(float target, float duration, bool stopWhenFaded)
        {
            _fadeFrom = _fade;
            _fadeTo = target;
            _fadeDuration = duration;
            _fadeTime = 0f;
            _stopWhenFaded = stopWhenFaded;

            if (duration <= 0f)
                FinishFade();
        }

        private void FinishFade()
        {
            _fade = _fadeTo;
            _fadeTime = _fadeDuration;

            if (_stopWhenFaded && _fade <= 0f)
                _source.Stop();
        }
    }
}

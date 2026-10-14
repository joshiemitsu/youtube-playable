using System.Collections.Generic;
using UnityEngine;

namespace RapidPro.Audio
{
    /// <summary>
    /// One sound the game can play, e.g. "Arrow exit" or "Level win music".
    /// Create one with <b>Assets &gt; Create &gt; RapidPro &gt; Audio Cue</b> and tune it in the Inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioCue", menuName = "RapidPro/Audio Cue")]
    public class AudioCue : ScriptableObject
    {
        [Tooltip("Clips to choose from. One is picked at random each time, so repeated sounds don't feel robotic.")]
        [SerializeField] private List<AudioClip> _clips = new List<AudioClip>();

        [Tooltip("Volume of this cue before the player's music or SFX volume is applied.")]
        [Range(0f, 1f)]
        [SerializeField] private float _volume = 1f;

        [Tooltip("Lowest random pitch. Set both pitch values to 1 for no variation.")]
        [Range(0.5f, 2f)]
        [SerializeField] private float _minPitch = 0.95f;

        [Tooltip("Highest random pitch.")]
        [Range(0.5f, 2f)]
        [SerializeField] private float _maxPitch = 1.05f;

        [Tooltip("Seconds that must pass before this cue can play again. Stops fast taps from stacking. SFX only.")]
        [Range(0f, 1f)]
        [SerializeField] private float _minGap = 0.05f;

        /// <summary>Volume of this cue, 0 to 1.</summary>
        public float Volume => _volume;

        /// <summary>Seconds that must pass before this cue can play again.</summary>
        public float MinGap => _minGap;

        /// <summary>Returns a random clip from the list, or null when the list is empty.</summary>
        public AudioClip PickClip()
        {
            if (_clips.Count == 0)
                return null;

            return _clips[Random.Range(0, _clips.Count)];
        }

        /// <summary>Returns a random pitch between the min and max pitch.</summary>
        public float PickPitch() => Random.Range(_minPitch, _maxPitch);

        /// <summary>Fills the cue from code. For tests only; designers use the Inspector.</summary>
        internal void Configure(IEnumerable<AudioClip> clips, float volume, float minGap)
        {
            _clips = new List<AudioClip>(clips);
            _volume = volume;
            _minGap = minGap;
            _minPitch = 1f;
            _maxPitch = 1f;
        }
    }
}

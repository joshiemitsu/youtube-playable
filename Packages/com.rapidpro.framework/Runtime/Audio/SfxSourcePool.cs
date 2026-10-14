using System.Collections.Generic;
using UnityEngine;

namespace RapidPro.Audio
{
    /// <summary>
    /// Up to <see cref="MaxSources"/> AudioSources for sound effects. Sources are created only when needed.
    /// When all are busy, the one closest to finishing is reused.
    /// </summary>
    internal class SfxSourcePool
    {
        /// <summary>Most sound effects that can play at once.</summary>
        public const int MaxSources = 8;

        private readonly Transform _parent;
        private readonly List<AudioSource> _sources = new List<AudioSource>();
        private readonly List<float> _busyUntil = new List<float>();

        public SfxSourcePool(Transform parent)
        {
            _parent = parent;
        }

        /// <summary>How many sources exist so far.</summary>
        public int Count => _sources.Count;

        /// <summary>
        /// Returns a source to play a sound that lasts <paramref name="duration"/> seconds, starting at <paramref name="now"/>.
        /// </summary>
        public AudioSource Get(float now, float duration)
        {
            int index = FindFree(now);

            if (index < 0 && _sources.Count < MaxSources)
                index = Create();

            if (index < 0)
                index = FindClosestToFinishing();

            _busyUntil[index] = now + duration;
            return _sources[index];
        }

        private int FindFree(float now)
        {
            for (int i = 0; i < _sources.Count; i++)
            {
                if (_busyUntil[i] <= now)
                    return i;
            }

            return -1;
        }

        private int FindClosestToFinishing()
        {
            int best = 0;
            for (int i = 1; i < _busyUntil.Count; i++)
            {
                if (_busyUntil[i] < _busyUntil[best])
                    best = i;
            }

            return best;
        }

        private int Create()
        {
            var go = new GameObject($"SFX {_sources.Count}");
            go.transform.SetParent(_parent, false);

            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;

            _sources.Add(source);
            _busyUntil.Add(0f);
            return _sources.Count - 1;
        }
    }
}

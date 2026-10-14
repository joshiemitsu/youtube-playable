using System;
using UnityEngine;

namespace RapidPro.Pooling
{
    /// <summary>
    /// One row in a <see cref="PoolConfig"/>: a prefab and how many copies to create up front.
    /// </summary>
    [Serializable]
    public class PoolConfigEntry
    {
        [Tooltip("Prefab to pool.")]
        [SerializeField] private GameObject _prefab;

        [Tooltip("Copies created when the game starts, so the first spawns don't instantiate.")]
        [Range(0, 200)]
        [SerializeField] private int _prewarmCount = 10;

        /// <summary>Creates an entry. Used by tests and editor tools; designers fill entries in the Inspector.</summary>
        public PoolConfigEntry(GameObject prefab, int prewarmCount)
        {
            _prefab = prefab;
            _prewarmCount = prewarmCount;
        }

        /// <summary>Prefab to pool.</summary>
        public GameObject Prefab => _prefab;

        /// <summary>Copies created up front.</summary>
        public int PrewarmCount => _prewarmCount;
    }
}

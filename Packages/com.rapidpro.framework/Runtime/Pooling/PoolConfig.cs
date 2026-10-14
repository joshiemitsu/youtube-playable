using System.Collections.Generic;
using UnityEngine;

namespace RapidPro.Pooling
{
    /// <summary>
    /// Lists the prefabs to prewarm and how many of each. Create one with
    /// <b>Assets &gt; Create &gt; RapidPro &gt; Pool Config</b> and pass it to <see cref="PoolService"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "PoolConfig", menuName = "RapidPro/Pool Config")]
    public class PoolConfig : ScriptableObject
    {
        [Tooltip("Prefabs to create up front when the game starts.")]
        [SerializeField] private List<PoolConfigEntry> _entries = new List<PoolConfigEntry>();

        /// <summary>The prefabs and their prewarm counts.</summary>
        public IReadOnlyList<PoolConfigEntry> Entries => _entries;

        /// <summary>Adds an entry. Used by tests and editor tools.</summary>
        public void Add(PoolConfigEntry entry) => _entries.Add(entry);
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace RapidPro.GameLoop
{
    /// <summary>
    /// The game's levels in play order. After the last level the list starts again, as is usual in
    /// hyper-casual games. Create one with <b>Assets &gt; Create &gt; RapidPro &gt; Level Catalog</b>.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelCatalog", menuName = "RapidPro/Level Catalog")]
    public class LevelCatalog : ScriptableObject
    {
        [Tooltip("Levels in play order. The list loops after the last one.")]
        [SerializeField] private List<LevelData> _levels = new List<LevelData>();

        /// <summary>How many levels there are.</summary>
        public int Count => _levels.Count;

        /// <summary>Returns the level for <paramref name="levelIndex"/>, looping past the end.</summary>
        /// <exception cref="InvalidOperationException">Thrown when the catalog is empty.</exception>
        public LevelData Get(int levelIndex)
        {
            if (_levels.Count == 0)
                throw new InvalidOperationException($"Level catalog '{name}' has no levels. Add at least one level asset.");

            int index = ((levelIndex % _levels.Count) + _levels.Count) % _levels.Count;
            return _levels[index];
        }

        /// <summary>Adds a level at the end. Used by tests and editor tools.</summary>
        public void Add(LevelData level) => _levels.Add(level);
    }
}

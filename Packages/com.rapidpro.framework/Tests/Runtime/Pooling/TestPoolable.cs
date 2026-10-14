using UnityEngine;

namespace RapidPro.Pooling.Tests
{
    /// <summary>Test component that counts its pool hooks.</summary>
    public class TestPoolable : MonoBehaviour, IPoolable
    {
        /// <summary>How many times <see cref="OnSpawn"/> was called.</summary>
        public int SpawnCount { get; private set; }

        /// <summary>How many times <see cref="OnDespawn"/> was called.</summary>
        public int DespawnCount { get; private set; }

        /// <inheritdoc />
        public void OnSpawn() => SpawnCount++;

        /// <inheritdoc />
        public void OnDespawn() => DespawnCount++;
    }
}

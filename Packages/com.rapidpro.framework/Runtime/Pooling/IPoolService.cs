using UnityEngine;

namespace RapidPro.Pooling
{
    /// <summary>
    /// Spawns objects from reusable pools instead of <c>Instantiate</c> and <c>Destroy</c>,
    /// so levels can restart without garbage or hitches.
    /// </summary>
    /// <example>
    /// <code>
    /// var pool = Services.Get&lt;IPoolService&gt;();
    ///
    /// Arrow arrow = pool.Spawn(_arrowPrefab, cellPosition, Quaternion.identity);
    /// // ... later, instead of Destroy:
    /// pool.Despawn(arrow.gameObject);
    /// </code>
    /// </example>
    public interface IPoolService
    {
        /// <summary>
        /// Returns an active instance of <paramref name="prefab"/>, reusing a pooled one when available.
        /// </summary>
        /// <param name="prefab">The prefab to spawn. Each prefab gets its own pool.</param>
        /// <param name="position">World position of the spawned object.</param>
        /// <param name="rotation">World rotation of the spawned object.</param>
        GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation);

        /// <summary>
        /// Same as <see cref="Spawn(GameObject, Vector3, Quaternion)"/>, but takes and returns a component,
        /// so you skip the <c>GetComponent</c> call.
        /// </summary>
        T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component;

        /// <summary>
        /// Deactivates <paramref name="instance"/> and returns it to its pool.
        /// Logs a warning and destroys it if it did not come from a pool.
        /// </summary>
        void Despawn(GameObject instance);
    }
}

using System;
using System.Collections.Generic;
using RapidPro.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RapidPro.Pooling
{
    /// <summary>
    /// Default <see cref="IPoolService"/>. Keeps one pool per prefab. Pooled objects wait,
    /// inactive, under a <c>[RapidPro Pool]</c> object so you can see them in the Hierarchy.
    /// </summary>
    /// <remarks>
    /// Create it once in your <c>GameInstaller</c>:
    /// <c>Services.Register&lt;IPoolService&gt;(new PoolService(_poolConfig, transform));</c>
    /// </remarks>
    public class PoolService : IPoolService, IDisposable
    {
        private readonly Transform _root;
        private readonly Dictionary<GameObject, Stack<GameObject>> _inactiveByPrefab = new Dictionary<GameObject, Stack<GameObject>>();
        private readonly Dictionary<GameObject, GameObject> _prefabByInstance = new Dictionary<GameObject, GameObject>();
        private readonly HashSet<GameObject> _inPool = new HashSet<GameObject>();
        private readonly List<IPoolable> _poolables = new List<IPoolable>();

        /// <summary>
        /// Creates the pool root and prewarms every entry in <paramref name="config"/>.
        /// </summary>
        /// <param name="config">Prefabs to create up front. Optional.</param>
        /// <param name="parent">Parent of the pool root, e.g. the installer, so pooled objects live in the Bootstrap scene. Optional.</param>
        public PoolService(PoolConfig config = null, Transform parent = null)
        {
            _root = new GameObject("[RapidPro Pool]").transform;
            _root.SetParent(parent, false);

            if (config == null)
                return;

            foreach (var entry in config.Entries)
                Prewarm(entry.Prefab, entry.PrewarmCount);
        }

        /// <summary>The object that holds pooled, inactive instances.</summary>
        public Transform Root => _root;

        /// <summary>
        /// Creates <paramref name="count"/> inactive copies of <paramref name="prefab"/> so later spawns don't instantiate.
        /// </summary>
        public void Prewarm(GameObject prefab, int count)
        {
            if (prefab == null)
            {
                Log.Warning("PoolService: skipped prewarm for a missing prefab. Check your PoolConfig entries.");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                var instance = CreateInstance(prefab);
                ReturnToPool(instance, prefab);
            }
        }

        /// <inheritdoc />
        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab), "PoolService.Spawn needs a prefab.");

            var inactive = GetInactiveStack(prefab);
            var instance = inactive.Count > 0 ? inactive.Pop() : CreateInstance(prefab);
            _inPool.Remove(instance);

            instance.transform.SetParent(null);
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);

            instance.GetComponentsInChildren(true, _poolables);
            foreach (var poolable in _poolables)
                poolable.OnSpawn();

            return instance;
        }

        /// <inheritdoc />
        public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab), $"PoolService.Spawn needs a {typeof(T).Name} prefab.");

            return Spawn(prefab.gameObject, position, rotation).GetComponent<T>();
        }

        /// <inheritdoc />
        public void Despawn(GameObject instance)
        {
            if (instance == null)
                return;

            if (!_prefabByInstance.TryGetValue(instance, out var prefab))
            {
                Log.Warning($"PoolService: '{instance.name}' was not spawned from a pool, so it was destroyed instead.", instance);
                DestroyObject(instance);
                return;
            }

            if (_inPool.Contains(instance))
            {
                Log.Warning($"PoolService: '{instance.name}' was despawned twice. Ignored the second call.", instance);
                return;
            }

            instance.GetComponentsInChildren(true, _poolables);
            foreach (var poolable in _poolables)
                poolable.OnDespawn();

            ReturnToPool(instance, prefab);
        }

        /// <summary>
        /// Destroys the pool root and every pooled (inactive) instance.
        /// Spawned objects still in the scene are left alone.
        /// </summary>
        public void Dispose()
        {
            if (_root != null)
                DestroyObject(_root.gameObject);

            _inactiveByPrefab.Clear();
            _prefabByInstance.Clear();
            _inPool.Clear();
        }

        private GameObject CreateInstance(GameObject prefab)
        {
            var instance = Object.Instantiate(prefab, _root);
            instance.name = prefab.name;
            _prefabByInstance[instance] = prefab;
            return instance;
        }

        private void ReturnToPool(GameObject instance, GameObject prefab)
        {
            instance.SetActive(false);
            instance.transform.SetParent(_root, false);
            GetInactiveStack(prefab).Push(instance);
            _inPool.Add(instance);
        }

        private Stack<GameObject> GetInactiveStack(GameObject prefab)
        {
            if (!_inactiveByPrefab.TryGetValue(prefab, out var stack))
            {
                stack = new Stack<GameObject>();
                _inactiveByPrefab[prefab] = stack;
            }

            return stack;
        }

        private static void DestroyObject(GameObject target)
        {
            if (Application.isPlaying)
                Object.Destroy(target);
            else
                Object.DestroyImmediate(target);
        }
    }
}

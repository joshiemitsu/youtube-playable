using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace RapidPro.Pooling.Tests
{
    /// <summary>
    /// Edit-mode tests for <see cref="PoolService"/>.
    /// </summary>
    public class PoolServiceTests
    {
        private GameObject _prefab;
        private PoolService _pool;

        [SetUp]
        public void SetUp()
        {
            _prefab = new GameObject("Coin");
            _prefab.AddComponent<TestPoolable>();
            _pool = new PoolService();
        }

        [TearDown]
        public void TearDown()
        {
            _pool.Dispose();
            Object.DestroyImmediate(_prefab);
        }

        [Test]
        public void Spawn_CreatesActiveInstanceAtPosition()
        {
            var position = new Vector3(1f, 2f, 3f);

            var instance = _pool.Spawn(_prefab, position, Quaternion.identity);

            Assert.AreNotSame(_prefab, instance);
            Assert.IsTrue(instance.activeSelf);
            Assert.AreEqual(position, instance.transform.position);
            Object.DestroyImmediate(instance);
        }

        [Test]
        public void Spawn_AfterDespawn_ReusesSameInstance()
        {
            var first = _pool.Spawn(_prefab, Vector3.zero, Quaternion.identity);
            _pool.Despawn(first);

            var second = _pool.Spawn(_prefab, Vector3.zero, Quaternion.identity);

            Assert.AreSame(first, second);
            Object.DestroyImmediate(second);
        }

        [Test]
        public void Despawn_DeactivatesAndMovesUnderRoot()
        {
            var instance = _pool.Spawn(_prefab, Vector3.zero, Quaternion.identity);

            _pool.Despawn(instance);

            Assert.IsFalse(instance.activeSelf);
            Assert.AreEqual(_pool.Root, instance.transform.parent);
        }

        [Test]
        public void SpawnAndDespawn_CallPoolableHooks()
        {
            var poolable = _pool.Spawn(_prefab.GetComponent<TestPoolable>(), Vector3.zero, Quaternion.identity);
            Assert.AreEqual(1, poolable.SpawnCount);

            _pool.Despawn(poolable.gameObject);

            Assert.AreEqual(1, poolable.DespawnCount);
        }

        [Test]
        public void Prewarm_CreatesInactiveInstances()
        {
            var config = ScriptableObject.CreateInstance<PoolConfig>();
            config.Add(new PoolConfigEntry(_prefab, 5));

            using (var pool = new PoolService(config))
            {
                Assert.AreEqual(5, pool.Root.childCount);
                foreach (Transform child in pool.Root)
                    Assert.IsFalse(child.gameObject.activeSelf);
            }

            Object.DestroyImmediate(config);
        }

        [Test]
        public void Spawn_AfterPrewarm_DoesNotCreateNewInstance()
        {
            _pool.Prewarm(_prefab, 1);

            var instance = _pool.Spawn(_prefab, Vector3.zero, Quaternion.identity);

            Assert.AreEqual(0, _pool.Root.childCount);
            Object.DestroyImmediate(instance);
        }

        [Test]
        public void Despawn_NotPooledObject_WarnsAndDoesNotThrow()
        {
            var stranger = new GameObject("Stranger");
            LogAssert.Expect(LogType.Warning, new Regex("was not spawned from a pool"));

            Assert.DoesNotThrow(() => _pool.Despawn(stranger));
        }

        [Test]
        public void Despawn_Twice_WarnsAndKeepsOneCopy()
        {
            var instance = _pool.Spawn(_prefab, Vector3.zero, Quaternion.identity);
            _pool.Despawn(instance);
            LogAssert.Expect(LogType.Warning, new Regex("despawned twice"));

            _pool.Despawn(instance);

            Assert.AreEqual(1, _pool.Root.childCount);
        }
    }
}

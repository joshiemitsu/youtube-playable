namespace RapidPro.Pooling
{
    /// <summary>
    /// Add to any component on a pooled object that must reset itself when it is reused.
    /// <see cref="IPoolService"/> calls these on every <c>IPoolable</c> in the object and its children.
    /// </summary>
    /// <example>
    /// <code>
    /// public class Coin : MonoBehaviour, IPoolable
    /// {
    ///     [SerializeField] private TrailRenderer _trail;
    ///
    ///     public void OnSpawn() => _trail.Clear();
    ///     public void OnDespawn() => StopAllCoroutines();
    /// }
    /// </code>
    /// </example>
    public interface IPoolable
    {
        /// <summary>Called right after the object is taken from the pool and activated.</summary>
        void OnSpawn();

        /// <summary>Called right before the object is deactivated and returned to the pool.</summary>
        void OnDespawn();
    }
}

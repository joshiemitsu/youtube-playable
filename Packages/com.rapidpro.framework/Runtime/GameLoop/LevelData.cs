using UnityEngine;

namespace RapidPro.GameLoop
{
    /// <summary>
    /// Base class for one level's data. Each game makes its own subclass with whatever a level needs
    /// (board size, pieces, goals) and lists its levels in a <see cref="LevelCatalog"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// [CreateAssetMenu(menuName = "MyGame/Level")]
    /// public class MyLevelData : LevelData
    /// {
    ///     public int Width;
    ///     public int Height;
    /// }
    /// </code>
    /// </example>
    public abstract class LevelData : ScriptableObject
    {
        [Tooltip("Name shown in debug tools and logs.")]
        [SerializeField] private string _displayName;

        /// <summary>Name shown in debug tools and logs. Falls back to the asset name.</summary>
        public string DisplayName => string.IsNullOrEmpty(_displayName) ? name : _displayName;
    }
}

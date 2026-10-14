using UnityEngine;

namespace RapidPro.Input
{
    /// <summary>
    /// A finger or mouse button that is down right now, as reported by an <see cref="IPointerSource"/>.
    /// </summary>
    public readonly struct RawPointer
    {
        /// <summary>Stays the same while the finger is down. The mouse uses <see cref="MouseId"/>.</summary>
        public readonly int Id;

        /// <summary>Screen position in pixels, (0, 0) at the bottom-left.</summary>
        public readonly Vector2 Position;

        /// <summary>Id used for the left mouse button.</summary>
        public const int MouseId = -1;

        /// <summary>Creates a raw pointer.</summary>
        public RawPointer(int id, Vector2 position)
        {
            Id = id;
            Position = position;
        }
    }
}

using UnityEngine;

namespace RapidPro.Input
{
    /// <summary>A quick touch and release without moving, or the second one of a double tap.</summary>
    public readonly struct TapEvent
    {
        /// <summary>Screen position in pixels.</summary>
        public readonly Vector2 Position;

        /// <summary>1 for a tap, 2 for a double tap.</summary>
        public readonly int TapCount;

        /// <summary>Creates a tap event.</summary>
        public TapEvent(Vector2 position, int tapCount)
        {
            Position = position;
            TapCount = tapCount;
        }
    }
}

using UnityEngine;

namespace RapidPro.Input
{
    /// <summary>A finger moving while held. Sent for drag start, every move, and drag end.</summary>
    public readonly struct DragEvent
    {
        /// <summary>Where the finger went down, in pixels.</summary>
        public readonly Vector2 StartPosition;

        /// <summary>Where the finger is now, in pixels.</summary>
        public readonly Vector2 Position;

        /// <summary>Movement since the last drag event, in pixels.</summary>
        public readonly Vector2 Delta;

        /// <summary>Creates a drag event.</summary>
        public DragEvent(Vector2 startPosition, Vector2 position, Vector2 delta)
        {
            StartPosition = startPosition;
            Position = position;
            Delta = delta;
        }

        /// <summary>Total movement since the finger went down, in pixels.</summary>
        public Vector2 TotalDelta => Position - StartPosition;
    }
}

using UnityEngine;

namespace RapidPro.Input
{
    /// <summary>A quick swipe released while still moving fast.</summary>
    public readonly struct FlickEvent
    {
        /// <summary>Where the finger went down, in pixels.</summary>
        public readonly Vector2 StartPosition;

        /// <summary>Where it was released, in pixels.</summary>
        public readonly Vector2 EndPosition;

        /// <summary>Average speed and direction, in pixels per second.</summary>
        public readonly Vector2 Velocity;

        /// <summary>The main direction: up, down, left or right.</summary>
        public readonly FlickDirection Direction;

        /// <summary>Creates a flick event.</summary>
        public FlickEvent(Vector2 startPosition, Vector2 endPosition, Vector2 velocity, FlickDirection direction)
        {
            StartPosition = startPosition;
            EndPosition = endPosition;
            Velocity = velocity;
            Direction = direction;
        }

        /// <summary>The exact direction as a unit vector, for diagonal flicks.</summary>
        public Vector2 DirectionVector => (EndPosition - StartPosition).normalized;
    }
}

using UnityEngine;

namespace RapidPro.Input
{
    /// <summary>A finger held still for the hold time. Fires once per press.</summary>
    public readonly struct HoldEvent
    {
        /// <summary>Screen position in pixels.</summary>
        public readonly Vector2 Position;

        /// <summary>Seconds held when the event fired.</summary>
        public readonly float Duration;

        /// <summary>Creates a hold event.</summary>
        public HoldEvent(Vector2 position, float duration)
        {
            Position = position;
            Duration = duration;
        }
    }
}

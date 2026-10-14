using UnityEngine;

namespace RapidPro.Input
{
    /// <summary>Two fingers moving apart (pinch out) or together (pinch in), or the scroll wheel on desktop.</summary>
    public readonly struct PinchEvent
    {
        /// <summary>Point between the two fingers (or the mouse cursor), in pixels. Zoom towards this.</summary>
        public readonly Vector2 Center;

        /// <summary>
        /// Change this frame, as a fraction of the screen's shorter side.
        /// Positive = pinch out (fingers apart, zoom in), negative = pinch in.
        /// </summary>
        public readonly float Delta;

        /// <summary>Current finger distance divided by the distance when the pinch began. 1 for the scroll wheel.</summary>
        public readonly float Scale;

        /// <summary>Creates a pinch event.</summary>
        public PinchEvent(Vector2 center, float delta, float scale)
        {
            Center = center;
            Delta = delta;
            Scale = scale;
        }

        /// <summary>True when the fingers moved apart.</summary>
        public bool IsPinchOut => Delta > 0f;
    }
}

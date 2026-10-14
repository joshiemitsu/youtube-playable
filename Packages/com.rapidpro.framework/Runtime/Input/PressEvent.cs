using UnityEngine;

namespace RapidPro.Input
{
    /// <summary>A finger going down or being released.</summary>
    public readonly struct PressEvent
    {
        /// <summary>Screen position in pixels.</summary>
        public readonly Vector2 Position;

        /// <summary>Seconds the finger was down. 0 for a press.</summary>
        public readonly float Duration;

        /// <summary>True when the finger moved far enough to be a drag. Release only.</summary>
        public readonly bool WasDrag;

        /// <summary>Creates a press event.</summary>
        public PressEvent(Vector2 position, float duration, bool wasDrag)
        {
            Position = position;
            Duration = duration;
            WasDrag = wasDrag;
        }
    }
}

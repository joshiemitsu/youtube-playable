using UnityEngine;

namespace RapidPro.Input
{
    /// <summary>
    /// One finger or mouse button for one frame, with its history since it went down.
    /// Gesture recognizers read these from <see cref="PointerFrame.Pointers"/>.
    /// </summary>
    public readonly struct TrackedPointer
    {
        /// <summary>Id from the pointer source.</summary>
        public readonly int Id;

        /// <summary>What happened this frame.</summary>
        public readonly PointerPhase Phase;

        /// <summary>Screen position in pixels.</summary>
        public readonly Vector2 Position;

        /// <summary>Movement since last frame, in pixels.</summary>
        public readonly Vector2 Delta;

        /// <summary>Where it went down, in pixels.</summary>
        public readonly Vector2 StartPosition;

        /// <summary>When it went down, in unscaled seconds.</summary>
        public readonly float StartTime;

        /// <summary>The furthest it has been from <see cref="StartPosition"/>, in pixels. Tells taps from drags.</summary>
        public readonly float MaxDistance;

        /// <summary>True if another finger was down at any point while this one was. Single-finger gestures ignore these.</summary>
        public readonly bool WasMultiTouch;

        /// <summary>True if it went down over a UI element. Gameplay gestures ignore these by default.</summary>
        public readonly bool StartedOverUI;

        /// <summary>Creates a pointer snapshot.</summary>
        public TrackedPointer(int id, PointerPhase phase, Vector2 position, Vector2 delta, Vector2 startPosition,
            float startTime, float maxDistance, bool wasMultiTouch, bool startedOverUI)
        {
            Id = id;
            Phase = phase;
            Position = position;
            Delta = delta;
            StartPosition = startPosition;
            StartTime = startTime;
            MaxDistance = maxDistance;
            WasMultiTouch = wasMultiTouch;
            StartedOverUI = startedOverUI;
        }

        /// <summary>True on the frame it went down or while it is held.</summary>
        public bool IsDown => Phase != PointerPhase.Ended;
    }
}

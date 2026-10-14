using UnityEngine;

namespace RapidPro.Display
{
    /// <summary>Where the camera should go, and its orthographic size if it has one.</summary>
    internal readonly struct CameraFitResult
    {
        public readonly Vector3 Position;
        public readonly float OrthographicSize;

        public CameraFitResult(Vector3 position, float orthographicSize)
        {
            Position = position;
            OrthographicSize = orthographicSize;
        }
    }
}

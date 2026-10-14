using UnityEngine;

namespace RapidPro.Display
{
    /// <summary>
    /// The math behind <see cref="CameraFitter"/>, kept free of scene objects so it can be tested.
    /// </summary>
    /// <remarks>
    /// Works in the camera's own axes: x = right, y = up, z = forward. The level bounds are turned into
    /// those axes, so a top-down, side-on or tilted camera all fit the same way.
    /// <list type="number">
    /// <item>Measure the level's width and height as the camera sees them, plus padding.</item>
    /// <item>Work out how big the view must be for that size to fit the usable part of the screen
    /// (the viewport: safe area minus reserved HUD space).</item>
    /// <item>Orthographic: that view size is the orthographic size. Perspective: it is the distance
    /// from the camera to the front of the level.</item>
    /// <item>Slide the camera so the level sits at the anchor inside the usable area.</item>
    /// </list>
    /// </remarks>
    internal static class CameraFitMath
    {
        private const float MinimumViewportFraction = 0.05f;

        /// <param name="bounds">World area that must be visible.</param>
        /// <param name="rotation">Camera rotation. It is kept; only position and size change.</param>
        /// <param name="currentPosition">Camera position now. Orthographic cameras keep their depth.</param>
        /// <param name="orthographic">True for an orthographic camera.</param>
        /// <param name="verticalFieldOfView">Perspective only, in degrees.</param>
        /// <param name="aspect">Camera width divided by height.</param>
        /// <param name="viewport">Usable part of the screen, as 0 to 1 fractions.</param>
        /// <param name="padding">World units kept free around the level.</param>
        /// <param name="mode">Fit inside, or blend width and height.</param>
        /// <param name="widthOrHeight">Weighted mode only. 0 = fit width, 1 = fit height.</param>
        /// <param name="anchor">Where the level sits in spare space. (0.5, 0.5) = centre.</param>
        public static CameraFitResult Calculate(
            Bounds bounds, Quaternion rotation, Vector3 currentPosition,
            bool orthographic, float verticalFieldOfView, float aspect,
            Rect viewport, float padding, CameraFitMode mode, float widthOrHeight, Vector2 anchor)
        {
            // 1. The level as the camera sees it.
            var toCamera = Quaternion.Inverse(rotation);
            GetLocalExtents(bounds, toCamera, out var localMin, out var localMax);

            float levelWidth = localMax.x - localMin.x + padding * 2f;
            float levelHeight = localMax.y - localMin.y + padding * 2f;
            float levelLeft = localMin.x - padding;
            float levelBottom = localMin.y - padding;

            float usableWidth = Mathf.Max(viewport.width, MinimumViewportFraction);
            float usableHeight = Mathf.Max(viewport.height, MinimumViewportFraction);

            // 2. Half the view height needed to fit the width, and to fit the height.
            float halfHeightForWidth = levelWidth / (2f * aspect * usableWidth);
            float halfHeightForHeight = levelHeight / (2f * usableHeight);
            float halfHeight = mode == CameraFitMode.FitInside
                ? Mathf.Max(halfHeightForWidth, halfHeightForHeight)
                : Mathf.Lerp(halfHeightForWidth, halfHeightForHeight, Mathf.Clamp01(widthOrHeight));

            // 3. Orthographic size, or distance for a perspective camera.
            float viewHeight = halfHeight * 2f;
            float viewWidth = viewHeight * aspect;
            float depth;

            if (orthographic)
            {
                depth = (toCamera * currentPosition).z;
            }
            else
            {
                float tanHalfFov = Mathf.Tan(verticalFieldOfView * 0.5f * Mathf.Deg2Rad);
                float distance = halfHeight / tanHalfFov;
                depth = localMin.z - distance;
            }

            // 4. Place the level at the anchor inside the usable area.
            float spareWidth = viewWidth * usableWidth - levelWidth;
            float spareHeight = viewHeight * usableHeight - levelHeight;

            float cameraX = levelLeft + viewWidth * 0.5f - viewport.x * viewWidth - spareWidth * Mathf.Clamp01(anchor.x);
            float cameraY = levelBottom + viewHeight * 0.5f - viewport.y * viewHeight - spareHeight * Mathf.Clamp01(anchor.y);

            var position = rotation * new Vector3(cameraX, cameraY, depth);
            return new CameraFitResult(position, halfHeight);
        }

        private static void GetLocalExtents(Bounds bounds, Quaternion toCamera, out Vector3 min, out Vector3 max)
        {
            min = Vector3.positiveInfinity;
            max = Vector3.negativeInfinity;

            for (int i = 0; i < 8; i++)
            {
                var corner = new Vector3(
                    (i & 1) == 0 ? bounds.min.x : bounds.max.x,
                    (i & 2) == 0 ? bounds.min.y : bounds.max.y,
                    (i & 4) == 0 ? bounds.min.z : bounds.max.z);

                var local = toCamera * corner;
                min = Vector3.Min(min, local);
                max = Vector3.Max(max, local);
            }
        }
    }
}

using NUnit.Framework;
using UnityEngine;

namespace RapidPro.Display.Tests
{
    /// <summary>
    /// Edit-mode tests for <see cref="CameraFitMath"/>. The level is a 10 x 10 square at the origin.
    /// Portrait screens use aspect 0.5 (e.g. 1080 x 2160), landscape screens aspect 2.
    /// </summary>
    public class CameraFitMathTests
    {
        private const float Tolerance = 0.001f;
        private static readonly Bounds Level = new Bounds(Vector3.zero, new Vector3(10f, 10f, 0f));
        private static readonly Rect FullScreen = new Rect(0f, 0f, 1f, 1f);
        private static readonly Vector2 Centre = new Vector2(0.5f, 0.5f);
        private static readonly Vector3 CameraStart = new Vector3(0f, 0f, -10f);

        private static CameraFitResult FitOrthographic(float aspect, Rect viewport, float padding = 0f,
            CameraFitMode mode = CameraFitMode.FitInside, float widthOrHeight = 0.5f, Vector2? anchor = null)
        {
            return CameraFitMath.Calculate(Level, Quaternion.identity, CameraStart, true, 60f, aspect,
                viewport, padding, mode, widthOrHeight, anchor ?? Centre);
        }

        [Test]
        public void FitInside_Portrait_IsLimitedByWidth()
        {
            // Visible width = size * 2 * aspect = 10 * 2 * 0.5 = 10, exactly the level width.
            var result = FitOrthographic(aspect: 0.5f, FullScreen);

            Assert.AreEqual(10f, result.OrthographicSize, Tolerance);
            Assert.AreEqual(CameraStart, result.Position);
        }

        [Test]
        public void FitInside_Landscape_IsLimitedByHeight()
        {
            var result = FitOrthographic(aspect: 2f, FullScreen);

            Assert.AreEqual(5f, result.OrthographicSize, Tolerance);
        }

        [Test]
        public void Weighted_FitHeight_IgnoresWidth()
        {
            var result = FitOrthographic(aspect: 0.5f, FullScreen, mode: CameraFitMode.Weighted, widthOrHeight: 1f);

            Assert.AreEqual(5f, result.OrthographicSize, Tolerance);
        }

        [Test]
        public void Weighted_HalfWay_BlendsWidthAndHeight()
        {
            // Width needs 10, height needs 5, half-way = 7.5.
            var result = FitOrthographic(aspect: 0.5f, FullScreen, mode: CameraFitMode.Weighted, widthOrHeight: 0.5f);

            Assert.AreEqual(7.5f, result.OrthographicSize, Tolerance);
        }

        [Test]
        public void Padding_AddsSpaceOnEverySide()
        {
            // Level width becomes 10 + 2 * 1 = 12.
            var result = FitOrthographic(aspect: 0.5f, FullScreen, padding: 1f);

            Assert.AreEqual(12f, result.OrthographicSize, Tolerance);
        }

        [Test]
        public void AnchorBottom_PutsLevelOnTheBottomEdge()
        {
            var result = FitOrthographic(aspect: 0.5f, FullScreen, anchor: new Vector2(0.5f, 0f));

            float screenBottom = result.Position.y - result.OrthographicSize;
            Assert.AreEqual(-5f, screenBottom, Tolerance, "The level's bottom (-5) should touch the screen bottom.");
        }

        [Test]
        public void ReservedTop_CentresLevelInTheRemainingSpace()
        {
            // Top 20% kept for the HUD. View height 20, usable 16, from y = -8 to y = 8.
            var viewport = new Rect(0f, 0f, 1f, 0.8f);

            var result = FitOrthographic(aspect: 0.5f, viewport);

            Assert.AreEqual(10f, result.OrthographicSize, Tolerance);
            Assert.AreEqual(2f, result.Position.y, Tolerance);
        }

        [Test]
        public void Perspective_MovesBackUntilLevelFits()
        {
            // Square screen, 60 degree FOV: distance = 5 / tan(30) = 8.66.
            var result = CameraFitMath.Calculate(Level, Quaternion.identity, CameraStart, false, 60f, 1f,
                FullScreen, 0f, CameraFitMode.FitInside, 0.5f, Centre);

            Assert.AreEqual(-8.660f, result.Position.z, Tolerance);
            Assert.AreEqual(0f, result.Position.x, Tolerance);
            Assert.AreEqual(0f, result.Position.y, Tolerance);
        }

        [Test]
        public void Perspective_TopDownCamera_MovesUp()
        {
            // Board lying on the ground (XZ plane), camera looking straight down.
            var board = new Bounds(Vector3.zero, new Vector3(10f, 0f, 10f));
            var lookDown = Quaternion.Euler(90f, 0f, 0f);

            var result = CameraFitMath.Calculate(board, lookDown, new Vector3(0f, 20f, 0f), false, 60f, 1f,
                FullScreen, 0f, CameraFitMode.FitInside, 0.5f, Centre);

            Assert.AreEqual(8.660f, result.Position.y, Tolerance);
            Assert.AreEqual(0f, result.Position.x, Tolerance);
            Assert.AreEqual(0f, result.Position.z, Tolerance);
        }
    }
}

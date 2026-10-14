using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace RapidPro.Display.Tests
{
    /// <summary>
    /// Edit-mode tests for <see cref="ScreenSnapshot"/>, <see cref="SafeAreaFitter"/>,
    /// <see cref="CanvasFitter"/> and <see cref="CameraFitter"/>, using made-up screens.
    /// </summary>
    public class DisplayComponentTests
    {
        private const float Tolerance = 0.001f;

        // A 1000 x 2000 phone with a 100 px notch at the top and a 100 px home bar at the bottom.
        private static readonly ScreenSnapshot NotchedPhone = new ScreenSnapshot(1000, 2000, new Rect(0f, 100f, 1000f, 1800f));
        private static readonly ScreenSnapshot Desktop = new ScreenSnapshot(1920, 1080, new Rect(0f, 0f, 1920f, 1080f));

        private GameObject _gameObject;

        [TearDown]
        public void TearDown()
        {
            if (_gameObject != null)
                Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void ScreenSnapshot_NormalizesSafeArea()
        {
            var safe = NotchedPhone.NormalizedSafeArea;

            Assert.AreEqual(0.05f, safe.yMin, Tolerance);
            Assert.AreEqual(0.95f, safe.yMax, Tolerance);
            Assert.IsTrue(NotchedPhone.IsPortrait);
            Assert.IsFalse(Desktop.IsPortrait);
        }

        [Test]
        public void ScreenSnapshot_DetectsChanges()
        {
            var same = new ScreenSnapshot(1000, 2000, new Rect(0f, 100f, 1000f, 1800f));
            var rotated = new ScreenSnapshot(2000, 1000, new Rect(100f, 0f, 1800f, 1000f));

            Assert.IsTrue(NotchedPhone == same);
            Assert.IsTrue(NotchedPhone != rotated);
        }

        [Test]
        public void SafeArea_AllEdges_MatchesSafeArea()
        {
            SafeAreaFitter.CalculateAnchors(NotchedPhone, true, true, true, true, out var min, out var max);

            Assert.AreEqual(new Vector2(0f, 0.05f), min);
            Assert.AreEqual(new Vector2(1f, 0.95f), max);
        }

        [Test]
        public void SafeArea_BottomOff_StretchesToScreenBottom()
        {
            SafeAreaFitter.CalculateAnchors(NotchedPhone, top: true, bottom: false, left: true, right: true, out var min, out var max);

            Assert.AreEqual(0f, min.y);
            Assert.AreEqual(0.95f, max.y, Tolerance);
        }

        [Test]
        public void SafeAreaFitter_SetsPanelAnchors()
        {
            _gameObject = new GameObject("Safe Panel", typeof(RectTransform));
            var fitter = _gameObject.AddComponent<SafeAreaFitter>();

            fitter.Apply(NotchedPhone);

            var rect = _gameObject.GetComponent<RectTransform>();
            Assert.AreEqual(0.05f, rect.anchorMin.y, Tolerance);
            Assert.AreEqual(0.95f, rect.anchorMax.y, Tolerance);
            Assert.AreEqual(Vector2.zero, rect.offsetMin);
        }

        [Test]
        public void CanvasFitter_Portrait_UsesPortraitReference()
        {
            _gameObject = new GameObject("Canvas");
            var fitter = _gameObject.AddComponent<CanvasFitter>();

            fitter.Apply(NotchedPhone);

            Assert.AreEqual(CanvasScaler.ScaleMode.ScaleWithScreenSize, fitter.Scaler.uiScaleMode);
            Assert.AreEqual(new Vector2(1080f, 1920f), fitter.Scaler.referenceResolution);
            Assert.AreEqual(CanvasScaler.ScreenMatchMode.Expand, fitter.Scaler.screenMatchMode);
        }

        [Test]
        public void CanvasFitter_Landscape_Weighted_UsesLandscapeReferenceAndWeight()
        {
            _gameObject = new GameObject("Canvas");
            var fitter = _gameObject.AddComponent<CanvasFitter>();
            fitter.Mode = CanvasFitMode.Weighted;

            fitter.Apply(Desktop);

            Assert.AreEqual(new Vector2(1920f, 1080f), fitter.Scaler.referenceResolution);
            Assert.AreEqual(CanvasScaler.ScreenMatchMode.MatchWidthOrHeight, fitter.Scaler.screenMatchMode);
            Assert.AreEqual(1f, fitter.Scaler.matchWidthOrHeight);
        }

        [Test]
        public void CameraFitter_Viewport_UsesSafeAreaAndReservedSpace()
        {
            _gameObject = new GameObject("Camera");
            var fitter = _gameObject.AddComponent<CameraFitter>();
            fitter.Configure(CameraFitMode.FitInside, 0.5f, new Vector2(0.5f, 0.5f), 0f,
                useSafeArea: true, reservedTop: 0.1f, reservedBottom: 0f);

            var viewport = fitter.GetViewport(NotchedPhone);

            Assert.AreEqual(0.05f, viewport.yMin, Tolerance);
            Assert.AreEqual(0.85f, viewport.yMax, Tolerance);
        }

        [Test]
        public void CameraFitter_Fit_SetsOrthographicSize()
        {
            _gameObject = new GameObject("Camera");
            var camera = _gameObject.AddComponent<Camera>();
            camera.orthographic = true;
            var fitter = _gameObject.AddComponent<CameraFitter>();
            fitter.Configure(CameraFitMode.FitInside, 0.5f, new Vector2(0.5f, 0.5f), 0f,
                useSafeArea: false, reservedTop: 0f, reservedBottom: 0f);

            fitter.Fit(NotchedPhone, aspect: 0.5f);

            // Default bounds are 10 x 10; a 0.5 aspect screen is limited by width.
            Assert.AreEqual(10f, camera.orthographicSize, Tolerance);
        }
    }
}

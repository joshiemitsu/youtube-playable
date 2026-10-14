namespace RapidPro.Display
{
    /// <summary>How <see cref="CanvasFitter"/> scales the UI when the screen shape differs from the reference.</summary>
    public enum CanvasFitMode
    {
        /// <summary>The whole reference layout always fits on screen. Extra space appears around it. Safest choice.</summary>
        FitInside,

        /// <summary>The layout fills the screen; parts near the edges may be cut off on unusual shapes.</summary>
        Fill,

        /// <summary>Blend between matching the screen width (0) and height (1) with the weight slider.</summary>
        Weighted
    }
}

namespace RapidPro.Display
{
    /// <summary>How <see cref="CameraFitter"/> fits the level into the screen.</summary>
    public enum CameraFitMode
    {
        /// <summary>The whole level is always visible, on every screen shape. Safest choice.</summary>
        FitInside,

        /// <summary>Blend between fitting the level's width (0) and height (1) with the weight slider. Parts may go off screen.</summary>
        Weighted
    }
}

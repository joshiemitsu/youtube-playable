namespace RapidPro.Input
{
    /// <summary>What a pointer did this frame.</summary>
    public enum PointerPhase
    {
        /// <summary>Went down this frame.</summary>
        Began,

        /// <summary>Is down and moved since last frame.</summary>
        Moved,

        /// <summary>Is down and did not move.</summary>
        Stationary,

        /// <summary>Was lifted this frame. This is its last frame.</summary>
        Ended
    }
}

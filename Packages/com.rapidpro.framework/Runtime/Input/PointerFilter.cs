namespace RapidPro.Input
{
    /// <summary>Shared rules for which pointers the built-in recognizers skip.</summary>
    internal static class PointerFilter
    {
        /// <summary>True when the pointer started on UI and the config says to ignore those.</summary>
        public static bool IsIgnored(TrackedPointer pointer, InputConfig config)
        {
            return config.IgnorePointersOverUI && pointer.StartedOverUI;
        }

        /// <summary>True for a pointer a one-finger gesture should use: not ignored and never part of a multi-touch.</summary>
        public static bool IsSingleFinger(TrackedPointer pointer, InputConfig config)
        {
            return !IsIgnored(pointer, config) && !pointer.WasMultiTouch;
        }
    }
}

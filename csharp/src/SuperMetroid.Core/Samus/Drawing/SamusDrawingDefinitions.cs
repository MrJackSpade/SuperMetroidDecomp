namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>The definitions drawing Samus reads: the arm cannon's cover character and directions (#1411).</summary>
/// <remarks>
/// This catalog owns rendering data only. Animation state, the shown body definitions,
/// sprite positions, and frame counters remain on their runtime state objects.
/// </remarks>
public static class SamusDrawingDefinitions
{
    /// <summary>How many vertical offsets each of Samus's body offset lists holds.</summary>
    public static class BodyOffsets
    {
            /// <summary>Sixteen landing bytes plus the adjacent byte read by the final unaligned word.</summary>
            public const int LandingVerticalOffsetByteCount = 17;
            /// <summary>Twelve transition poses with two visual offset bytes each.</summary>
            public const int PostureTransitionVerticalOffsetByteCount = 24;
            /// <summary>Native contiguous drained-body visual offset interval.</summary>
            public const int DrainedVerticalOffsetByteCount = 32;
    }



    /// <summary>The arm-cannon cover's aim directions.</summary>
    public static class ArmCannon
    {
        /// <summary>Number of direction selectors accepted by the native dispatcher.</summary>
        public const int DirectionCount = 10;
    }
}

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
            /// <summary>
            /// Four landing poses with four visual offset bytes each. The game reads each as the low byte of a word whose
            /// high byte, the next landing byte or the byte after the table, the drawn position never shows (#1500).
            /// </summary>
            public const int LandingVerticalOffsetByteCount = 16;
            /// <summary>Twelve transition poses with two visual offset bytes each.</summary>
            public const int PostureTransitionVerticalOffsetByteCount = 24;
            /// <summary>Contiguous drained-body visual offset interval.</summary>
            public const int DrainedVerticalOffsetByteCount = 32;
    }



    /// <summary>The arm-cannon cover's aim directions.</summary>
    public static class ArmCannon
    {
        /// <summary>Number of direction selectors accepted by the dispatcher.</summary>
        public const int DirectionCount = 10;
    }
}

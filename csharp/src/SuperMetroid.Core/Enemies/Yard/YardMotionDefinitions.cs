namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>When a Yard hides, when its turns come back after a slope, and how slowly it must fall to land (#2076).</summary>
internal static class YardMotionDefinitions
{
    /// <summary>How far above Samus, in pixels, a Yard still hides from her gaze; one any higher keeps crawling.</summary>
    internal const int HidingReachAbove = 96;

    /// <summary>How many slope-free passes a crawling Yard makes before it may turn again.</summary>
    internal const int TurnSuppressionPasses = 16;

    /// <summary>
    /// The whole pixels an update below which a falling Yard that meets the floor lands; one falling this fast or faster,
    /// or meeting a ceiling, bounces.
    /// </summary>
    internal const int LandingWholeSpeedLimit = 3;
}

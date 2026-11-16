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

    /// <summary>How far past its next step a hiding Yard looks for the surface it holds, in pixels (#2182).</summary>
    internal const int HidingProbeMargin = 7;

    /// <summary>How far past its next step a crawling Yard's probe of its surface moves, in pixels (#2182).</summary>
    internal const int CrawlProbeMargin = 1;

    /// <summary>The whole pixels an update a shot sends a Yard sideways, the way Samus faces, and up (#2182).</summary>
    internal const int BeamLaunchWholeSpeed = 1;
}

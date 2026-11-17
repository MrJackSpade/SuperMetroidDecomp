namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>
/// The Beetom's crawl, hop and drain animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="BeetomAnimations"/>.
/// </summary>
internal enum BeetomSegment
{
    /// <summary>The leftward crawl's start, its entry cue.</summary>
    CrawlingLeft,
    /// <summary>The leftward crawl's four strides.</summary>
    CrawlingLeftStride,
    /// <summary>
    /// Hopping left: pushing off, stretching its leg and pushing again, then landing with its legs tucked; it keeps
    /// moving off screen.
    /// </summary>
    HopLeft,
    /// <summary>The leftward drain's green gem, shown once.</summary>
    DrainingLeft,
    /// <summary>The leftward drain's red gem pulse.</summary>
    DrainingLeftRed,
    /// <summary>The rightward crawl's start, its entry cue.</summary>
    CrawlingRight,
    /// <summary>The rightward crawl's four strides.</summary>
    CrawlingRightStride,
    /// <summary>
    /// Hopping right: pushing off, stretching its leg and pushing again, then landing with its legs tucked; it keeps
    /// moving off screen.
    /// </summary>
    HopRight,
    /// <summary>The rightward drain's green gem, shown once.</summary>
    DrainingRight,
    /// <summary>The rightward drain's red gem pulse.</summary>
    DrainingRightRed,
}

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
    /// <summary>The hop left animation.</summary>
    HopLeft,
    /// <summary>The leftward drain's green gem, shown once.</summary>
    DrainingLeft,
    /// <summary>The leftward drain's red gem pulse.</summary>
    DrainingLeftRed,
    /// <summary>The rightward crawl's start, its entry cue.</summary>
    CrawlingRight,
    /// <summary>The rightward crawl's four strides.</summary>
    CrawlingRightStride,
    /// <summary>The hop right animation.</summary>
    HopRight,
    /// <summary>The rightward drain's green gem, shown once.</summary>
    DrainingRight,
    /// <summary>The rightward drain's red gem pulse.</summary>
    DrainingRightRed,
}

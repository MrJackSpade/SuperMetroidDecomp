namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>
/// The Beetom's crawl, hop and drain animations as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="BeetomAnimations"/>.
/// </summary>
internal enum BeetomSegment
{
    /// <summary>The crawling left animation.</summary>
    CrawlingLeft,
    /// <summary>The loop.</summary>
    CrawlingLeftLoop,
    /// <summary>The hop left animation.</summary>
    HopLeft,
    /// <summary>The draining left animation.</summary>
    DrainingLeft,
    /// <summary>The loop.</summary>
    DrainingLeftLoop,
    /// <summary>The crawling right animation.</summary>
    CrawlingRight,
    /// <summary>The loop.</summary>
    CrawlingRightLoop,
    /// <summary>The hop right animation.</summary>
    HopRight,
    /// <summary>The draining right animation.</summary>
    DrainingRight,
    /// <summary>The loop.</summary>
    DrainingRightLoop,
}

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// The Sciser, Zero, Viola, orange Zoomer and shared Zoomer/Zeela/Sova surface animations (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="CrawlerAnimations"/>.
/// </summary>
internal enum CrawlerSegment
{
    /// <summary>The Sciser upside right animation.</summary>
    SciserUpsideRight,
    /// <summary>The loop.</summary>
    SciserUpsideRightLoop,
    /// <summary>The Sciser upside left animation.</summary>
    SciserUpsideLeft,
    /// <summary>The loop.</summary>
    SciserUpsideLeftLoop,
    /// <summary>The Sciser upside down animation.</summary>
    SciserUpsideDown,
    /// <summary>The loop.</summary>
    SciserUpsideDownLoop,
    /// <summary>The Sciser upside up animation.</summary>
    SciserUpsideUp,
    /// <summary>The loop.</summary>
    SciserUpsideUpLoop,
    /// <summary>The zero upside right animation.</summary>
    ZeroUpsideRight,
    /// <summary>The loop.</summary>
    ZeroUpsideRightLoop,
    /// <summary>The zero upside left animation.</summary>
    ZeroUpsideLeft,
    /// <summary>The loop.</summary>
    ZeroUpsideLeftLoop,
    /// <summary>The zero upside down animation.</summary>
    ZeroUpsideDown,
    /// <summary>The loop.</summary>
    ZeroUpsideDownLoop,
    /// <summary>The zero upside up animation.</summary>
    ZeroUpsideUp,
    /// <summary>The loop.</summary>
    ZeroUpsideUpLoop,
    /// <summary>The Viola upside down animation.</summary>
    ViolaUpsideDown,
    /// <summary>The Viola upside up animation.</summary>
    ViolaUpsideUp,
    /// <summary>The Viola upside right animation.</summary>
    ViolaUpsideRight,
    /// <summary>The Viola upside left animation.</summary>
    ViolaUpsideLeft,
    /// <summary>The Viola normal animation.</summary>
    ViolaNormal,
    /// <summary>The h Zoomer upside right animation.</summary>
    HZoomerUpsideRight,
    /// <summary>The loop.</summary>
    HZoomerUpsideRightLoop,
    /// <summary>The h Zoomer upside left animation.</summary>
    HZoomerUpsideLeft,
    /// <summary>The loop.</summary>
    HZoomerUpsideLeftLoop,
    /// <summary>The h Zoomer upside down animation.</summary>
    HZoomerUpsideDown,
    /// <summary>The loop.</summary>
    HZoomerUpsideDownLoop,
    /// <summary>The h Zoomer upside up animation.</summary>
    HZoomerUpsideUp,
    /// <summary>The loop.</summary>
    HZoomerUpsideUpLoop,
    /// <summary>The shared upside right animation.</summary>
    SharedUpsideRight,
    /// <summary>The loop.</summary>
    SharedUpsideRightLoop,
    /// <summary>The shared upside left animation.</summary>
    SharedUpsideLeft,
    /// <summary>The loop.</summary>
    SharedUpsideLeftLoop,
    /// <summary>The shared upside down animation.</summary>
    SharedUpsideDown,
    /// <summary>The loop.</summary>
    SharedUpsideDownLoop,
    /// <summary>The shared upside up animation.</summary>
    SharedUpsideUp,
    /// <summary>The loop.</summary>
    SharedUpsideUpLoop,
}

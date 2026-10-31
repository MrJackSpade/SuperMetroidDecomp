namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// The Sciser, Zero, Viola, orange Zoomer and shared Zoomer/Zeela/Sova surface animations as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="CrawlerAnimations"/>.
/// </summary>
internal enum CrawlerProgram
{
    /// <summary>The sciser upside right animation.</summary>
    SciserUpsideRight,
    /// <summary>The loop.</summary>
    SciserUpsideRightLoop,
    /// <summary>The sciser upside left animation.</summary>
    SciserUpsideLeft,
    /// <summary>The loop.</summary>
    SciserUpsideLeftLoop,
    /// <summary>The sciser upside down animation.</summary>
    SciserUpsideDown,
    /// <summary>The loop.</summary>
    SciserUpsideDownLoop,
    /// <summary>The sciser upside up animation.</summary>
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
    /// <summary>The viola upside down animation.</summary>
    ViolaUpsideDown,
    /// <summary>The viola upside up animation.</summary>
    ViolaUpsideUp,
    /// <summary>The viola upside right animation.</summary>
    ViolaUpsideRight,
    /// <summary>The viola upside left animation.</summary>
    ViolaUpsideLeft,
    /// <summary>The viola normal animation.</summary>
    ViolaNormal,
    /// <summary>The h zoomer upside right animation.</summary>
    HZoomerUpsideRight,
    /// <summary>The loop.</summary>
    HZoomerUpsideRightLoop,
    /// <summary>The h zoomer upside left animation.</summary>
    HZoomerUpsideLeft,
    /// <summary>The loop.</summary>
    HZoomerUpsideLeftLoop,
    /// <summary>The h zoomer upside down animation.</summary>
    HZoomerUpsideDown,
    /// <summary>The loop.</summary>
    HZoomerUpsideDownLoop,
    /// <summary>The h zoomer upside up animation.</summary>
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

namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>
/// Fireflea's glow loop as a typed animation (#641, #2369): one wingbeat at each of its seven glows (#2477), which its
/// animation plays from its brightest down to its dimmest and back, in authored order.
/// Their steps are <see cref="FirefleaAnimations"/>.
/// </summary>
internal enum FirefleaSegment
{
    /// <summary>
    /// One beat of its wings, up, level, down and level, at its brightest glow. Its animation beats twice so, dims and
    /// brightens back, repeating.
    /// </summary>
    BrightestWingbeat,
    /// <summary>One beat of its wings at the glow just short of its brightest, as it dims and again as it brightens back.</summary>
    BrighterWingbeat,
    /// <summary>One beat of its wings at its bright glow, the second step down from its brightest.</summary>
    BrightWingbeat,
    /// <summary>One beat of its wings at its middle glow, halfway between its brightest and its dimmest.</summary>
    MidWingbeat,
    /// <summary>One beat of its wings at its dim glow, the third step up from its dimmest.</summary>
    DimWingbeat,
    /// <summary>One beat of its wings at the glow just above its dimmest, as it dims and again as it brightens back.</summary>
    DimmerWingbeat,
    /// <summary>One beat of its wings at its dimmest glow, once a cycle, where it turns to brighten back.</summary>
    DimmestWingbeat,
}

namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>
/// Fireflea's glow loop as a typed animation (#641, #2369): its wingbeat at its brightest glow, which its animation
/// plays twice, and its wingbeats as it dims and brightens back, in authored order.
/// Their steps are <see cref="FirefleaAnimations"/>.
/// </summary>
internal enum FirefleaSegment
{
    /// <summary>
    /// One beat of its wings, up, level, down and level, at its brightest glow. Its animation beats twice so, dims and
    /// brightens back, repeating.
    /// </summary>
    BrightestWingbeat,
    /// <summary>One wingbeat at each glow as it dims, from brighter down to its dimmest.</summary>
    Dimming,
    /// <summary>One wingbeat at each glow as it brightens back, from dimmer up to brighter.</summary>
    Brightening,
}

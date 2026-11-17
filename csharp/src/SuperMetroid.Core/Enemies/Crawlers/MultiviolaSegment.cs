namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// Multiviola's spinning flight loop as a typed animation (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="MultiviolaAnimations"/>.
/// </summary>
internal enum MultiviolaSegment
{
    /// <summary>
    /// Spinning as it flies, its face fading in, brightening to its brightest and fading out again, repeating.
    /// </summary>
    Flying,
}

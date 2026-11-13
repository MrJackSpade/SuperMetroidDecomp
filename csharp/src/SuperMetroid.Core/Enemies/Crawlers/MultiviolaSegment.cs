namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// Multiviola's spinning flight loop as a typed program (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="MultiviolaAnimations"/>.
/// </summary>
internal enum MultiviolaSegment
{
    /// <summary>The flying animation.</summary>
    Flying,
}

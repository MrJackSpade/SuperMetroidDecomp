namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>
/// Fireflea's 52-frame glow loop as a typed program (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="FirefleaAnimations"/>.
/// </summary>
internal enum FirefleaSegment
{
    /// <summary>The loop animation.</summary>
    Loop,
}

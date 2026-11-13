namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>
/// Polyp's stationary pose as a typed program (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="PolypAnimations"/>.
/// </summary>
internal enum PolypSegment
{
    /// <summary>The stationary animation.</summary>
    Stationary,
}

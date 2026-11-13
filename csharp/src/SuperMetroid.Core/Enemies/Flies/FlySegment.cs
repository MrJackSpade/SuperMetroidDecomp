namespace SuperMetroid.Core.Enemies.Flies;

/// <summary>
/// The shared Mellow, Mella and Memu flight loop as a typed program (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="FlyAnimations"/>.
/// </summary>
internal enum FlySegment
{
    /// <summary>The flight animation.</summary>
    Flight,
}

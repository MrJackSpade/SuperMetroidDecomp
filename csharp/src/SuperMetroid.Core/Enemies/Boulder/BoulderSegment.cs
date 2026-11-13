namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>
/// The Boulder's mirrored rolling loops as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="BoulderAnimations"/>.
/// </summary>
internal enum BoulderSegment
{
    /// <summary>The left animation.</summary>
    Left,
    /// <summary>The right animation.</summary>
    Right,
}

namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>
/// The Boulder's mirrored rolling loops as typed programs (#641), one per bank-$A6 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="BoulderAnimations"/>.
/// </summary>
internal enum BoulderProgram
{
    /// <summary>The native list at $A6:86A7.</summary>
    Left,
    /// <summary>The native list at $A6:86CB.</summary>
    Right,
}

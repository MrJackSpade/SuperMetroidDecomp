namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>
/// Choot's idle, jumping and falling animations as typed programs (#641), one per bank-$A2 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="ChootAnimations"/>.
/// </summary>
internal enum ChootProgram
{
    /// <summary>The native list at $A2:D82C.</summary>
    Idle,
    /// <summary>The native list at $A2:D834.</summary>
    Jumping,
    /// <summary>The native list at $A2:D840.</summary>
    Falling,
}

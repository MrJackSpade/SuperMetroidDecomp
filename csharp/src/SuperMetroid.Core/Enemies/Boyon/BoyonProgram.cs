namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>
/// Boyon's idle and bouncing loops as typed programs (#641), one per bank-$A2 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="BoyonAnimations"/>.
/// </summary>
internal enum BoyonProgram
{
    /// <summary>The native list at $A2:86A7.</summary>
    Idle,
    /// <summary>The loop at $A2:86AB.</summary>
    IdleLoop,
    /// <summary>The native list at $A2:86BF.</summary>
    Bouncing,
    /// <summary>The loop at $A2:86C3.</summary>
    BouncingLoop,
}

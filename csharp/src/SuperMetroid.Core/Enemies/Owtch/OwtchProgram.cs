namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>
/// Owtch's left and right shell-cycling animations as typed programs (#641), one per bank-$A2 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="OwtchAnimations"/>.
/// </summary>
internal enum OwtchProgram
{
    /// <summary><c>InstList_Owtch_MovingLeft_0</c> at $A2:A3AB.</summary>
    MovingLeft,
    /// <summary>The native list at $A2:A3AD.</summary>
    MovingLeftLoop,
    /// <summary><c>InstList_Owtch_MovingRight_0</c> at $A2:A3BD.</summary>
    MovingRight,
    /// <summary>The native list at $A2:A3BF.</summary>
    MovingRightLoop,
}

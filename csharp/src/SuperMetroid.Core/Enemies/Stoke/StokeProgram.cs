namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>
/// Stoke's walking and fire-spitting animations as typed programs (#641), one per bank-$A2 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="StokeAnimations"/>.
/// </summary>
internal enum StokeProgram
{
    /// <summary><c>InstList_Stoke_MovingLeft_0</c> at $A2:8932.</summary>
    MovingLeft,
    /// <summary>The native list at $A2:8934.</summary>
    MovingLeftLoop,
    /// <summary><c>InstList_Stoke_AttackingLeft</c> at $A2:8948.</summary>
    AttackingLeft,
    /// <summary><c>InstList_Stoke_MovingRight_0</c> at $A2:8958.</summary>
    MovingRight,
    /// <summary>The native list at $A2:895A.</summary>
    MovingRightLoop,
    /// <summary><c>InstList_Stoke_AttackingRight</c> at $A2:896E.</summary>
    AttackingRight,
}

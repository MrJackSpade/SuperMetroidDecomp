namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// Dragon's idle, wing and attack animations as typed programs (#641), one per bank-$A2 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="DragonAnimations"/>.
/// </summary>
internal enum DragonProgram
{
    /// <summary><c>InstList_Dragon_Idle_FacingLeft</c> at $A2:E59B.</summary>
    IdleFacingLeft,
    /// <summary><c>InstList_Dragon_Wings_FacingLeft</c> at $A2:E5A1.</summary>
    WingsFacingLeft,
    /// <summary><c>InstList_Dragon_Idle_FacingRight</c> at $A2:E5AD.</summary>
    IdleFacingRight,
    /// <summary><c>InstList_Dragon_Wings_FacingRight</c> at $A2:E5B3.</summary>
    WingsFacingRight,
    /// <summary><c>InstList_Dragon_Attacking_FacingLeft</c> at $A2:E5BF.</summary>
    AttackingFacingLeft,
    /// <summary><c>InstList_Dragon_Attacking_FacingRight</c> at $A2:E5D7.</summary>
    AttackingFacingRight,
}

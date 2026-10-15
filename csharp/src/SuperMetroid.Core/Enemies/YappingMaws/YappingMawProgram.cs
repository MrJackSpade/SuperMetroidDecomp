namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// The Yapping Maw's eight attack bites and six cooldown animations as typed programs (#641), one per bank-$A8 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="YappingMawAnimations"/>.
/// </summary>
internal enum YappingMawProgram
{
    /// <summary><c>InstList_YappingMaw_Attacking_FacingUp</c> at $A8:9F6F.</summary>
    AttackingUp,
    /// <summary><c>InstList_YappingMaw_Attacking_FacingUpRight</c> at $A8:9F85.</summary>
    AttackingUpRight,
    /// <summary><c>InstList_YappingMaw_Attacking_FacingRight</c> at $A8:9F9B.</summary>
    AttackingRight,
    /// <summary><c>InstList_YappingMaw_Attacking_FacingDownRight</c> at $A8:9FB1.</summary>
    AttackingDownRight,
    /// <summary><c>InstList_YappingMaw_Attacking_FacingDown</c> at $A8:9FC7.</summary>
    AttackingDown,
    /// <summary><c>InstList_YappingMaw_Attacking_FacingDownLeft</c> at $A8:9FDD.</summary>
    AttackingDownLeft,
    /// <summary><c>InstList_YappingMaw_Attacking_FacingLeft</c> at $A8:9FF3.</summary>
    AttackingLeft,
    /// <summary><c>InstList_YappingMaw_Attacking_FacingUpLeft</c> at $A8:A009.</summary>
    AttackingUpLeft,
    /// <summary><c>InstList_YappingMaw_Cooldown_FacingUpRight</c> at $A8:A01F.</summary>
    CooldownUpRight,
    /// <summary><c>InstList_YappingMaw_Cooldown_FacingUp_0</c> at $A8:A025.</summary>
    CooldownUp,
    /// <summary>The loop at $A8:A027.</summary>
    CooldownUpLoop,
    /// <summary><c>InstList_YappingMaw_Cooldown_FacingUpLeft_0</c> at $A8:A03D.</summary>
    CooldownUpLeft,
    /// <summary>The loop at $A8:A045.</summary>
    CooldownUpLeftLoop,
    /// <summary><c>InstList_YappingMaw_Cooldown_FacingDownRight</c> at $A8:A05B.</summary>
    CooldownDownRight,
    /// <summary><c>InstList_YappingMaw_Cooldown_FacingDown_0</c> at $A8:A061.</summary>
    CooldownDown,
    /// <summary>The loop at $A8:A063.</summary>
    CooldownDownLoop,
    /// <summary><c>InstList_YappingMaw_Cooldown_FacingDownLeft_0</c> at $A8:A079.</summary>
    CooldownDownLeft,
    /// <summary>The loop at $A8:A081.</summary>
    CooldownDownLeftLoop,
}

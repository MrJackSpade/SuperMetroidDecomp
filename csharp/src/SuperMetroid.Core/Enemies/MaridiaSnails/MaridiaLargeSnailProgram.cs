namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>
/// Oum's idle, attacking and rolling animations as typed programs (#641), one per bank-$A2 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="MaridiaLargeSnailAnimations"/>.
/// </summary>
internal enum MaridiaLargeSnailProgram
{
    /// <summary><c>InstList_Oum_FacingLeft_Idle</c> at $A2:CA4B.</summary>
    FacingLeftIdle,
    /// <summary><c>InstList_Oum_FacingLeft_Attacking</c> at $A2:CA51.</summary>
    FacingLeftAttacking,
    /// <summary><c>InstList_Oum_FacingLeft_RollingForwards</c> at $A2:CA8B.</summary>
    FacingLeftRollingForwards,
    /// <summary><c>InstList_Oum_FacingLeft_RollingBackwards</c> at $A2:CAB3.</summary>
    FacingLeftRollingBackwards,
    /// <summary><c>InstList_Oum_FacingRight_Idle</c> at $A2:CADB.</summary>
    FacingRightIdle,
    /// <summary><c>InstList_Oum_FacingRight_Attacking</c> at $A2:CAE1.</summary>
    FacingRightAttacking,
    /// <summary><c>InstList_Oum_FacingRight_RollingForwards</c> at $A2:CB1B.</summary>
    FacingRightRollingForwards,
    /// <summary><c>InstList_Oum_FacingRight_RollingBackwards</c> at $A2:CB43.</summary>
    FacingRightRollingBackwards,
}

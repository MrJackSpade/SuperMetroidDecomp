namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The Etecoon's look, run, wall-jump, hop and flex animations as typed programs (#641), one per bank-$A7 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="EtecoonAnimations"/>.
/// </summary>
internal enum EtecoonProgram
{
    /// <summary><c>InstList_Etecoon_LookRightAtSamusAndRunLeft</c> at $A7:E81E.</summary>
    LookRightAtSamusAndRunLeft,
    /// <summary>The native list at $A7:E824.</summary>
    BeginRunningLeft,
    /// <summary><c>InstList_Etecoon_RunningLeft</c> at $A7:E828.</summary>
    RunningLeft,
    /// <summary><c>InstList_Etecoon_WallJump_0</c> at $A7:E83C.</summary>
    WallJumpLeft,
    /// <summary><c>InstList_Etecoon_WallJump_1</c> at $A7:E840.</summary>
    WallJumpLeftLoop,
    /// <summary><c>InstList_Etecoon_Hopping_FacingLeft</c> at $A7:E854.</summary>
    HoppingFacingLeft,
    /// <summary>The native list at $A7:E85A.</summary>
    ContinueHoppingFacingLeft,
    /// <summary><c>InstList_Etecoon_HitCeiling</c> at $A7:E862.</summary>
    HitCeiling,
    /// <summary><c>InstList_Etecoon_WallJumpLeftEligible</c> at $A7:E870.</summary>
    WallJumpLeftEligible,
    /// <summary><c>InstList_Etecoon_LookLeftAtSamusAndRunRight</c> at $A7:E876.</summary>
    LookLeftAtSamusAndRunRight,
    /// <summary>The native list at $A7:E87C.</summary>
    BeginRunningRight,
    /// <summary><c>InstList_Etecoon_RunningRight</c> at $A7:E880.</summary>
    RunningRight,
    /// <summary><c>InstList_Etecoon_WallJumpRight</c> at $A7:E894.</summary>
    WallJumpRight,
    /// <summary><c>InstList_Etecoon_JumpingRight</c> at $A7:E898.</summary>
    JumpingRight,
    /// <summary><c>InstList_Etecoon_Hopping_FacingRight</c> at $A7:E8AC.</summary>
    HoppingFacingRight,
    /// <summary>The native list at $A7:E8B2.</summary>
    ContinueHoppingFacingRight,
    /// <summary><c>InstList_Etecoon_WallJumpRightEligible</c> at $A7:E8C8.</summary>
    WallJumpRightEligible,
    /// <summary><c>InstList_Etecoon_Initial</c> at $A7:E8CE.</summary>
    Initial,
    /// <summary><c>InstList_Etecoon_Flexing_0</c> at $A7:E8D6.</summary>
    Flexing,
    /// <summary><c>InstList_Etecoon_Flexing_1</c> at $A7:E8DA.</summary>
    FlexingLoop,
}

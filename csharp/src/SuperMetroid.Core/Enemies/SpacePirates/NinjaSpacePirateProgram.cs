namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The ninja Space Pirates' stance, kick, claw, spin-jump and divekick animations as typed programs (#641), one per bank-$B2 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="NinjaSpacePirateAnimations"/>.
/// </summary>
internal enum NinjaSpacePirateProgram
{
    /// <summary><c>InstList_PirateNinja_ProjectileClawAttack_Left</c> at $B2:F15C.</summary>
    ClawAttackLeft,
    /// <summary><c>InstList_PirateNinja_SpinJumpLeft_0</c> at $B2:F1C4.</summary>
    SpinJumpLeft,
    /// <summary>The loop at $B2:F206.</summary>
    SpinJumpLeftLoop,
    /// <summary><c>InstList_PirateNinja_Active_FacingLeft_0</c> at $B2:F22E.</summary>
    ActiveFacingLeft,
    /// <summary>The loop at $B2:F232.</summary>
    ActiveFacingLeftLoop,
    /// <summary><c>InstList_PirateNinja_Flinch_FacingLeft</c> at $B2:F270.</summary>
    FlinchFacingLeft,
    /// <summary><c>InstList_PirateNinja_DivekickLeft_Jump_0</c> at $B2:F27C.</summary>
    DivekickLeftJump,
    /// <summary>The loop at $B2:F28A.</summary>
    DivekickLeftJumpLoop,
    /// <summary><c>InstList_PirateNinja_DivekickLeft_Divekick</c> at $B2:F2A0.</summary>
    DivekickLeftDive,
    /// <summary><c>InstList_PirateNinja_DivekickLeft_WalkToLeftPost_0</c> at $B2:F2B2.</summary>
    WalkToLeftPost,
    /// <summary>The loop at $B2:F2B6.</summary>
    WalkToLeftPostLoop,
    /// <summary><c>InstList_PirateNinja_Initial_FacingLeft_0</c> at $B2:F2DA.</summary>
    InitialFacingLeft,
    /// <summary>The loop at $B2:F2DE.</summary>
    InitialFacingLeftLoop,
    /// <summary><c>InstList_PirateNinja_Land_FacingLeft_0</c> at $B2:F2F8.</summary>
    LandFacingLeft,
    /// <summary>The loop at $B2:F314.</summary>
    LandFacingLeftLoop,
    /// <summary><c>InstList_PirateNinja_StandingKick_FacingLeft</c> at $B2:F32E.</summary>
    KickFacingLeft,
    /// <summary><c>InstList_PirateNinja_ProjectileClawAttack_Right</c> at $B2:F34A.</summary>
    ClawAttackRight,
    /// <summary><c>InstList_PirateNinja_SpinJumpRight_0</c> at $B2:F3B2.</summary>
    SpinJumpRight,
    /// <summary>The loop at $B2:F3F4.</summary>
    SpinJumpRightLoop,
    /// <summary><c>InstList_PirateNinja_Active_FacingRight_0</c> at $B2:F420.</summary>
    ActiveFacingRight,
    /// <summary>The loop at $B2:F424.</summary>
    ActiveFacingRightLoop,
    /// <summary><c>InstList_PirateNinja_Flinch_FacingRight</c> at $B2:F462.</summary>
    FlinchFacingRight,
    /// <summary><c>InstList_PirateNinja_DivekickRight_Jump_0</c> at $B2:F46E.</summary>
    DivekickRightJump,
    /// <summary>The loop at $B2:F47C.</summary>
    DivekickRightJumpLoop,
    /// <summary><c>InstList_PirateNinja_DivekickRight_Divekick</c> at $B2:F492.</summary>
    DivekickRightDive,
    /// <summary><c>InstList_PirateNinja_DivekickRight_WalkToRightPost_0</c> at $B2:F4A4.</summary>
    WalkToRightPost,
    /// <summary>The loop at $B2:F4A8.</summary>
    WalkToRightPostLoop,
    /// <summary><c>InstList_PirateNinja_Initial_FacingRight_0</c> at $B2:F4CC.</summary>
    InitialFacingRight,
    /// <summary>The loop at $B2:F4D0.</summary>
    InitialFacingRightLoop,
    /// <summary><c>InstList_PirateNinja_Land_FacingRight_0</c> at $B2:F4EA.</summary>
    LandFacingRight,
    /// <summary>The loop at $B2:F506.</summary>
    LandFacingRightLoop,
    /// <summary><c>InstList_PirateNinja_StandingKick_FacingRight</c> at $B2:F51A.</summary>
    KickFacingRight,
}

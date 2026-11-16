namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The ninja Space Pirates' stance, kick, claw, spin-jump and divekick animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="NinjaSpacePirateAnimations"/>.
/// </summary>
internal enum NinjaSpacePirateSegment
{
    /// <summary>The claw attack left animation.</summary>
    ClawAttackLeft,
    /// <summary>The spin jump left animation.</summary>
    SpinJumpLeft,
    /// <summary>The active facing left animation.</summary>
    ActiveFacingLeft,
    /// <summary>The flinch facing left animation.</summary>
    FlinchFacingLeft,
    /// <summary>The divekick left jump animation.</summary>
    DivekickLeftJump,
    /// <summary>The divekick left dive animation.</summary>
    DivekickLeftDive,
    /// <summary>The walk to left post animation.</summary>
    WalkToLeftPost,
    /// <summary>The initial facing left animation.</summary>
    InitialFacingLeft,
    /// <summary>The land facing left animation.</summary>
    LandFacingLeft,
    /// <summary>The kick facing left animation.</summary>
    KickFacingLeft,
    /// <summary>The claw attack right animation.</summary>
    ClawAttackRight,
    /// <summary>The spin jump right animation.</summary>
    SpinJumpRight,
    /// <summary>The active facing right animation.</summary>
    ActiveFacingRight,
    /// <summary>The flinch facing right animation.</summary>
    FlinchFacingRight,
    /// <summary>The divekick right jump animation.</summary>
    DivekickRightJump,
    /// <summary>The divekick right dive animation.</summary>
    DivekickRightDive,
    /// <summary>The walk to right post animation.</summary>
    WalkToRightPost,
    /// <summary>The initial facing right animation.</summary>
    InitialFacingRight,
    /// <summary>The land facing right animation.</summary>
    LandFacingRight,
    /// <summary>The kick facing right animation.</summary>
    KickFacingRight,
}

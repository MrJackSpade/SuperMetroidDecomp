namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The ninja Space Pirates' stance, kick, claw, spin-jump and divekick animations (#641): one segment per animation a spawn,
/// branch or wake enters, and the loop each plays on into (#2276), in authored order.
/// Their steps are <see cref="NinjaSpacePirateAnimations"/>.
/// </summary>
internal enum NinjaSpacePirateSegment
{
    /// <summary>The claw attack left animation.</summary>
    ClawAttackLeft,
    /// <summary>The spin jump left animation.</summary>
    SpinJumpLeft,
    /// <summary>The left spin jump's spin, its sound replayed each turn, looped.</summary>
    SpinJumpLeftSpinning,
    /// <summary>The active facing left animation.</summary>
    ActiveFacingLeft,
    /// <summary>The ready stance facing left, looped, each pass signalling the pirate ready to act.</summary>
    ActiveFacingLeftStance,
    /// <summary>The flinch facing left animation.</summary>
    FlinchFacingLeft,
    /// <summary>The divekick left jump animation.</summary>
    DivekickLeftJump,
    /// <summary>The left divekick jump's tucked pose, flashing gold, looped.</summary>
    DivekickLeftJumpFlash,
    /// <summary>The divekick left dive animation.</summary>
    DivekickLeftDive,
    /// <summary>The walk to left post animation.</summary>
    WalkToLeftPost,
    /// <summary>The run to the left post, looped.</summary>
    RunningToLeftPost,
    /// <summary>The initial facing left animation.</summary>
    InitialFacingLeft,
    /// <summary>The first pose facing left: looking at the viewer and away, looped.</summary>
    InitialFacingLeftLookingAround,
    /// <summary>The land facing left animation.</summary>
    LandFacingLeft,
    /// <summary>The ready stance after the landing facing left, looped.</summary>
    LandFacingLeftReady,
    /// <summary>The kick facing left animation.</summary>
    KickFacingLeft,
    /// <summary>The claw attack right animation.</summary>
    ClawAttackRight,
    /// <summary>The spin jump right animation.</summary>
    SpinJumpRight,
    /// <summary>The right spin jump's spin, its sound replayed each turn, looped.</summary>
    SpinJumpRightSpinning,
    /// <summary>The active facing right animation.</summary>
    ActiveFacingRight,
    /// <summary>The ready stance facing right, looped, each pass signalling the pirate ready to act.</summary>
    ActiveFacingRightStance,
    /// <summary>The flinch facing right animation.</summary>
    FlinchFacingRight,
    /// <summary>The divekick right jump animation.</summary>
    DivekickRightJump,
    /// <summary>The right divekick jump's tucked pose, flashing gold, looped.</summary>
    DivekickRightJumpFlash,
    /// <summary>The divekick right dive animation.</summary>
    DivekickRightDive,
    /// <summary>The walk to right post animation.</summary>
    WalkToRightPost,
    /// <summary>The run to the right post, looped.</summary>
    RunningToRightPost,
    /// <summary>The initial facing right animation.</summary>
    InitialFacingRight,
    /// <summary>The first pose facing right: looking at the viewer and away, looped.</summary>
    InitialFacingRightLookingAround,
    /// <summary>The land facing right animation.</summary>
    LandFacingRight,
    /// <summary>The ready stance after the landing facing right, looped.</summary>
    LandFacingRightReady,
    /// <summary>The kick facing right animation.</summary>
    KickFacingRight,
}

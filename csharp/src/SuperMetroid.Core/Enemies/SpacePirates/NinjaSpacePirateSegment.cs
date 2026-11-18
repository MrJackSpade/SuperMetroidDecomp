namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The ninja Space Pirates' stance, kick, claw, spin-jump and divekick animations (#641): one segment per animation a spawn,
/// branch or wake enters, and the loop each plays on into (#2276), in authored order.
/// Their steps are <see cref="NinjaSpacePirateAnimations"/>.
/// </summary>
internal enum NinjaSpacePirateSegment
{
    /// <summary>
    /// Facing left, crouching and raising its claw overhead to throw it twice with the attack sound, then back to its
    /// stance.
    /// </summary>
    ClawAttackLeft,
    /// <summary>
    /// Facing left, throwing a claw with the attack sound, then crouching and springing into a spin jump left.
    /// </summary>
    SpinJumpLeft,
    /// <summary>The left spin jump's spin, its sound replayed each turn, looped.</summary>
    SpinJumpLeftSpinning,
    /// <summary>Facing left as it becomes active, then in its stance.</summary>
    ActiveFacingLeft,
    /// <summary>The ready stance facing left, looped, each pass signalling the pirate ready to act.</summary>
    ActiveFacingLeftStance,
    /// <summary>Flinching facing left, then back to its stance.</summary>
    FlinchFacingLeft,
    /// <summary>Crouching facing left to leap for a divekick, then flashing as it rises.</summary>
    DivekickLeftJump,
    /// <summary>The left divekick jump's tucked pose, flashing gold, looped.</summary>
    DivekickLeftJumpFlash,
    /// <summary>Diving left in its kick, flashing gold, with the attack sound, held.</summary>
    DivekickLeftDive,
    /// <summary>Heading back to its left post, then running there.</summary>
    WalkToLeftPost,
    /// <summary>The run to the left post, looped.</summary>
    RunningToLeftPost,
    /// <summary>Facing left where it waits at the start, then looking around.</summary>
    AwaitingSamusFacingLeft,
    /// <summary>The first pose facing left: looking at the viewer and away, looped.</summary>
    LookingAroundFacingLeft,
    /// <summary>
    /// Landing facing left in its stance colours: standing, looking at the viewer and turning to face right, then
    /// holding ready to divekick.
    /// </summary>
    LandFacingLeft,
    /// <summary>The ready stance after the landing facing left, looped.</summary>
    LandFacingLeftReady,
    /// <summary>
    /// Facing left, chambering and snapping out a long kick with the attack sound, then back to its stance.
    /// </summary>
    KickFacingLeft,
    /// <summary>
    /// Facing right, crouching and raising its claw overhead to throw it twice with the attack sound, then back to its
    /// stance.
    /// </summary>
    ClawAttackRight,
    /// <summary>
    /// Facing right, throwing a claw with the attack sound, then crouching and springing into a spin jump right.
    /// </summary>
    SpinJumpRight,
    /// <summary>The right spin jump's spin, its sound replayed each turn, looped.</summary>
    SpinJumpRightSpinning,
    /// <summary>Facing right as it becomes active, then in its stance.</summary>
    ActiveFacingRight,
    /// <summary>The ready stance facing right, looped, each pass signalling the pirate ready to act.</summary>
    ActiveFacingRightStance,
    /// <summary>Flinching facing right, then back to its stance.</summary>
    FlinchFacingRight,
    /// <summary>Crouching facing right to leap for a divekick, then flashing as it rises.</summary>
    DivekickRightJump,
    /// <summary>The right divekick jump's tucked pose, flashing gold, looped.</summary>
    DivekickRightJumpFlash,
    /// <summary>Diving right in its kick, flashing gold, with the attack sound, held.</summary>
    DivekickRightDive,
    /// <summary>Heading back to its right post, then running there.</summary>
    WalkToRightPost,
    /// <summary>The run to the right post, looped.</summary>
    RunningToRightPost,
    /// <summary>Facing right where it waits at the start, then looking around.</summary>
    AwaitingSamusFacingRight,
    /// <summary>The first pose facing right: looking at the viewer and away, looped.</summary>
    LookingAroundFacingRight,
    /// <summary>
    /// Landing facing right in its stance colours: standing, looking at the viewer and turning to face left, then
    /// holding ready to divekick.
    /// </summary>
    LandFacingRight,
    /// <summary>The ready stance after the landing facing right, looped.</summary>
    LandFacingRightReady,
    /// <summary>
    /// Facing right, chambering and snapping out a long kick with the attack sound, then back to its stance.
    /// </summary>
    KickFacingRight,
}

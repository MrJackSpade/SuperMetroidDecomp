using System.Collections.Frozen;
using SuperMetroid.Core.Input;
using static SuperMetroid.Core.Samus.Poses.SamusPoseInputs;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The input-transition lists Samus's poses select (#1929), each named for the poses it serves: poses that share their
/// transitions share a list. Each list holds its own ordered chords and reads them itself (#2571), so no switch over
/// list identities picks the chords a pose reads.
/// </summary>
internal sealed partial class SamusPoseInputList
{
    /// <summary>The list's name, for diagnostics.</summary>
    private readonly string _name;

    /// <summary>Selects the first satisfied chord of the list for the held and newly pressed actions.</summary>
    private readonly Func<GameplayAction, GameplayAction, SamusPoseInputMatch> _match;

    /// <summary>A list named <paramref name="name"/> whose chords <paramref name="match"/> reads in order.</summary>
    /// <param name="name">The list's name, for diagnostics.</param>
    /// <param name="match">Selects the first satisfied chord of the list.</param>
    private SamusPoseInputList(string name, Func<GameplayAction, GameplayAction, SamusPoseInputMatch> match)
    {
        _name = name;
        _match = match;
    }

    /// <summary>
    /// The list with no transitions, which every pose whose input does not change it selects: turns in the air and while
    /// crouching or falling, posture and morph transitions, the Grapple's poses, the shinespark, Crystal Flash, the X-ray
    /// scope, death, the drained poses, the glitch balls and their knockbacks.
    /// </summary>
    internal static SamusPoseInputList NoInputTransitions { get; } = new(nameof(NoInputTransitions),
        (_, _) => SamusPoseInputRules.MatchNoInputTransitions());

    /// <summary>The input-transition list of both forward-facing poses, in the power suit and suited.</summary>
    internal static SamusPoseInputList ForwardFacing { get; } = new(nameof(ForwardFacing),
        (held, _) => SamusPoseInputRules.MatchForwardFacing(held));

    /// <summary>The input-transition list of the right-facing standing poses and the landings, whatever their aim.</summary>
    internal static SamusPoseInputList StandingAndLandingRight { get; } = new(nameof(StandingAndLandingRight),
        (held, newlyPressed) => SamusPoseInputRules.MatchStandingAndLandingRight(held, newlyPressed));

    /// <summary>The input-transition list of the left-facing standing poses and the landings, whatever their aim.</summary>
    internal static SamusPoseInputList StandingAndLandingLeft { get; } = new(nameof(StandingAndLandingLeft),
        (held, newlyPressed) => SamusPoseInputRules.MatchStandingAndLandingRight(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored());

    /// <summary>The input-transition list of the right-facing running poses, whatever their aim.</summary>
    internal static SamusPoseInputList RunningRight { get; } = new(nameof(RunningRight),
        (held, newlyPressed) => SamusPoseInputRules.MatchRunningRight(held, newlyPressed));

    /// <summary>The input-transition list of the left-facing running poses, whatever their aim.</summary>
    internal static SamusPoseInputList RunningLeft { get; } = new(nameof(RunningLeft),
        (held, newlyPressed) => SamusPoseInputRules.MatchRunningRight(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored());

    /// <summary>The input-transition list of the right-facing jump transitions, whatever their aim.</summary>
    internal static SamusPoseInputList JumpTransitionRight { get; } = new(nameof(JumpTransitionRight),
        (held, _) => SamusPoseInputRules.MatchJumpTransitionRight(held));

    /// <summary>The input-transition list of the left-facing jump transitions, whatever their aim.</summary>
    internal static SamusPoseInputList JumpTransitionLeft { get; } = new(nameof(JumpTransitionLeft),
        (held, _) => SamusPoseInputRules.MatchJumpTransitionLeft(held));

    /// <summary>The input-transition list of the right-facing neutral, forward, aimed-up and diagonal normal jumps.</summary>
    internal static SamusPoseInputList NormalJumpRight { get; } = new(nameof(NormalJumpRight),
        (held, _) => SamusPoseInputRules.MatchNormalJumpRight(held));

    /// <summary>The input-transition list of the left-facing neutral, forward, aimed-up and diagonal normal jumps.</summary>
    internal static SamusPoseInputList NormalJumpLeft { get; } = new(nameof(NormalJumpLeft),
        (held, _) => SamusPoseInputRules.MatchNormalJumpRight(MirrorButtons(held)).Mirrored());

    /// <summary>The input-transition list of the damage boost left poses.</summary>
    internal static SamusPoseInputList DamageBoostLeft { get; } = new(nameof(DamageBoostLeft),
        (held, _) => SamusPoseInputRules.MatchDamageBoost(GameplayAction.Left, held));

    /// <summary>The input-transition list of the damage boost right poses.</summary>
    internal static SamusPoseInputList DamageBoostRight { get; } = new(nameof(DamageBoostRight),
        (held, _) => SamusPoseInputRules.MatchDamageBoost(GameplayAction.Right, held));

    /// <summary>The input-transition list of the spin jump right poses.</summary>
    internal static SamusPoseInputList SpinJumpRight { get; } = new(nameof(SpinJumpRight),
        (held, newlyPressed) => SamusPoseInputRules.MatchSpinRight(SamusPoseId.SpinJumpRightPose, held, newlyPressed));

    /// <summary>The input-transition list of the spin jump left poses.</summary>
    internal static SamusPoseInputList SpinJumpLeft { get; } = new(nameof(SpinJumpLeft),
        (held, newlyPressed) => SamusPoseInputRules.MatchSpinRight(SamusPoseId.SpinJumpRightPose, MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored());

    /// <summary>The input-transition list of the space jump right poses.</summary>
    internal static SamusPoseInputList SpaceJumpRight { get; } = new(nameof(SpaceJumpRight),
        (held, newlyPressed) => SamusPoseInputRules.MatchSpinRight(SamusPoseId.SpaceJumpRightPose, held, newlyPressed));

    /// <summary>The input-transition list of the space jump left poses.</summary>
    internal static SamusPoseInputList SpaceJumpLeft { get; } = new(nameof(SpaceJumpLeft),
        (held, newlyPressed) => SamusPoseInputRules.MatchSpinRight(SamusPoseId.SpaceJumpRightPose, MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored());

    /// <summary>The input-transition list of the screw attack right poses.</summary>
    internal static SamusPoseInputList ScrewAttackRight { get; } = new(nameof(ScrewAttackRight),
        (held, newlyPressed) => SamusPoseInputRules.MatchSpinRight(SamusPoseId.ScrewAttackRightPose, held, newlyPressed));

    /// <summary>The input-transition list of the screw attack left poses.</summary>
    internal static SamusPoseInputList ScrewAttackLeft { get; } = new(nameof(ScrewAttackLeft),
        (held, newlyPressed) => SamusPoseInputRules.MatchSpinRight(SamusPoseId.ScrewAttackRightPose, MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored());

    /// <summary>The input-transition list of the right-facing morph ball on the ground, still or rolling.</summary>
    internal static SamusPoseInputList MorphBallRight { get; } = new(nameof(MorphBallRight),
        (held, newlyPressed) => SamusPoseInputRules.MatchMorphBall(SamusPoseId.UnmorphingTransitionRightPose, held, newlyPressed));

    /// <summary>The input-transition list of the left-facing morph ball on the ground, still or rolling.</summary>
    internal static SamusPoseInputList MorphBallLeft { get; } = new(nameof(MorphBallLeft),
        (held, newlyPressed) => SamusPoseInputRules.MatchMorphBall(SamusPoseId.UnmorphingTransitionLeftPose, held, newlyPressed));

    /// <summary>The input-transition list of the crouching right poses.</summary>
    internal static SamusPoseInputList CrouchingRight { get; } = new(nameof(CrouchingRight),
        (held, newlyPressed) => SamusPoseInputRules.MatchCrouchingRight(held, newlyPressed));

    /// <summary>The input-transition list of the crouching left poses.</summary>
    internal static SamusPoseInputList CrouchingLeft { get; } = new(nameof(CrouchingLeft),
        (held, newlyPressed) => SamusPoseInputRules.MatchCrouchingLeft(held, newlyPressed));

    /// <summary>The input-transition list of the falling right poses.</summary>
    internal static SamusPoseInputList FallingRight { get; } = new(nameof(FallingRight),
        (held, _) => SamusPoseInputRules.MatchFallingRight(held));

    /// <summary>The input-transition list of the falling left poses.</summary>
    internal static SamusPoseInputList FallingLeft { get; } = new(nameof(FallingLeft),
        (held, _) => SamusPoseInputRules.MatchFallingRight(MirrorButtons(held)).Mirrored());

    /// <summary>The input-transition list of the morph ball falling right poses.</summary>
    internal static SamusPoseInputList MorphBallFallingRight { get; } = new(nameof(MorphBallFallingRight),
        (held, newlyPressed) => SamusPoseInputRules.MatchMorphBallFallingRight(held, newlyPressed));

    /// <summary>The input-transition list of the morph ball falling left poses.</summary>
    internal static SamusPoseInputList MorphBallFallingLeft { get; } = new(nameof(MorphBallFallingLeft),
        (held, newlyPressed) => SamusPoseInputRules.MatchMorphBallFallingRight(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored());

    /// <summary>The input-transition list of the left-facing moonwalk poses, whatever their aim.</summary>
    internal static SamusPoseInputList MoonwalkLeft { get; } = new(nameof(MoonwalkLeft),
        (held, newlyPressed) => SamusPoseInputRules.MatchMoonwalkLeft(held, newlyPressed));

    /// <summary>The input-transition list of the right-facing moonwalk poses, whatever their aim.</summary>
    internal static SamusPoseInputList MoonwalkRight { get; } = new(nameof(MoonwalkRight),
        (held, newlyPressed) => SamusPoseInputRules.MatchMoonwalkRight(held, newlyPressed));

    /// <summary>The input-transition list of the knockback right poses.</summary>
    internal static SamusPoseInputList KnockbackRight { get; } = new(nameof(KnockbackRight),
        (held, _) => SamusPoseInputRules.MatchKnockbackRight(held));

    /// <summary>The input-transition list of the knockback left poses.</summary>
    internal static SamusPoseInputList KnockbackLeft { get; } = new(nameof(KnockbackLeft),
        (held, _) => SamusPoseInputRules.MatchKnockbackRight(MirrorButtons(held)).Mirrored());

    /// <summary>The input-transition list of the right-facing spring ball on the ground, still or moving.</summary>
    internal static SamusPoseInputList SpringBallGroundRight { get; } = new(nameof(SpringBallGroundRight),
        (held, newlyPressed) => SamusPoseInputRules.MatchSpringBallGround(SamusPoseId.UnmorphingTransitionRightPose, SamusPoseId.SpringBallJumpRightPose, held, newlyPressed));

    /// <summary>The input-transition list of the left-facing spring ball on the ground, still or moving.</summary>
    internal static SamusPoseInputList SpringBallGroundLeft { get; } = new(nameof(SpringBallGroundLeft),
        (held, newlyPressed) => SamusPoseInputRules.MatchSpringBallGround(SamusPoseId.UnmorphingTransitionLeftPose, SamusPoseId.SpringBallJumpLeftPose, held, newlyPressed));

    /// <summary>The input-transition list of the spring ball falling right poses.</summary>
    internal static SamusPoseInputList SpringBallFallingRight { get; } = new(nameof(SpringBallFallingRight),
        (held, newlyPressed) => SamusPoseInputRules.MatchSpringBallFallingRight(held, newlyPressed));

    /// <summary>The input-transition list of the spring ball falling left poses.</summary>
    internal static SamusPoseInputList SpringBallFallingLeft { get; } = new(nameof(SpringBallFallingLeft),
        (held, newlyPressed) => SamusPoseInputRules.MatchSpringBallFallingRight(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored());

    /// <summary>The input-transition list of the spring ball jump right poses.</summary>
    internal static SamusPoseInputList SpringBallJumpRight { get; } = new(nameof(SpringBallJumpRight),
        (held, newlyPressed) => SamusPoseInputRules.MatchSpringBallJump(GameplayAction.Right, held, newlyPressed));

    /// <summary>The input-transition list of the spring ball jump left poses.</summary>
    internal static SamusPoseInputList SpringBallJumpLeft { get; } = new(nameof(SpringBallJumpLeft),
        (held, newlyPressed) => SamusPoseInputRules.MatchSpringBallJump(GameplayAction.Left, held, newlyPressed));

    /// <summary>The input-transition list of the wall jump right poses.</summary>
    internal static SamusPoseInputList WallJumpRight { get; } = new(nameof(WallJumpRight),
        (held, newlyPressed) => SamusPoseInputRules.MatchWallJumpRight(held, newlyPressed));

    /// <summary>The input-transition list of the wall jump left poses.</summary>
    internal static SamusPoseInputList WallJumpLeft { get; } = new(nameof(WallJumpLeft),
        (held, newlyPressed) => SamusPoseInputRules.MatchWallJumpRight(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored());

    /// <summary>The input-transition list of the ran into wall right poses.</summary>
    internal static SamusPoseInputList RanIntoWallRight { get; } = new(nameof(RanIntoWallRight),
        (held, newlyPressed) => SamusPoseInputRules.MatchRanIntoWallRight(held, newlyPressed));

    /// <summary>The input-transition list of the ran into wall left poses.</summary>
    internal static SamusPoseInputList RanIntoWallLeft { get; } = new(nameof(RanIntoWallLeft),
        (held, newlyPressed) => SamusPoseInputRules.MatchRanIntoWallRight(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored());

    /// <summary>The input-transition list of the normal jump gun extended right poses.</summary>
    internal static SamusPoseInputList NormalJumpGunExtendedRight { get; } = new(nameof(NormalJumpGunExtendedRight),
        (held, _) => SamusPoseInputRules.MatchNormalJumpGunExtendedRight(held));

    /// <summary>The input-transition list of the normal jump gun extended left poses.</summary>
    internal static SamusPoseInputList NormalJumpGunExtendedLeft { get; } = new(nameof(NormalJumpGunExtendedLeft),
        (held, _) => SamusPoseInputRules.MatchNormalJumpGunExtendedRight(MirrorButtons(held)).Mirrored());

    /// <summary>The input-transition list of the normal jump aim down right poses.</summary>
    internal static SamusPoseInputList NormalJumpAimDownRight { get; } = new(nameof(NormalJumpAimDownRight),
        (held, newlyPressed) => SamusPoseInputRules.MatchNormalJumpAimDownRight(held, newlyPressed));

    /// <summary>The input-transition list of the normal jump aim down left poses.</summary>
    internal static SamusPoseInputList NormalJumpAimDownLeft { get; } = new(nameof(NormalJumpAimDownLeft),
        (held, newlyPressed) => SamusPoseInputRules.MatchNormalJumpAimDownLeftOfRight(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored());

    /// <summary>The input-transition list of the unmorphing transition right poses.</summary>
    internal static SamusPoseInputList UnmorphingTransitionRight { get; } = new(nameof(UnmorphingTransitionRight),
        (held, _) => SamusPoseInputRules.MatchUnmorphingTransitionRight(held));

    /// <summary>The input-transition list of the unmorphing transition left poses.</summary>
    internal static SamusPoseInputList UnmorphingTransitionLeft { get; } = new(nameof(UnmorphingTransitionLeft),
        (held, _) => SamusPoseInputRules.MatchUnmorphingTransitionRight(MirrorButtons(held)).Mirrored());

    /// <summary>The input-transition list of the turning right to left poses.</summary>
    internal static SamusPoseInputList TurningRightToLeft { get; } = new(nameof(TurningRightToLeft),
        (held, newlyPressed) => SamusPoseInputRules.MatchTurningRightToLeft(held, newlyPressed));

    /// <summary>The input-transition list of the turning left to right poses.</summary>
    internal static SamusPoseInputList TurningLeftToRight { get; } = new(nameof(TurningLeftToRight),
        (held, newlyPressed) => SamusPoseInputRules.MatchTurningRightToLeft(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored());

    /// <summary>The input-transition list of the turning right to left aim up poses.</summary>
    internal static SamusPoseInputList TurningRightToLeftAimUp { get; } = new(nameof(TurningRightToLeftAimUp),
        (held, newlyPressed) => SamusPoseInputRules.MatchAimedTurningRightToLeft(SamusPoseId.TurningRightToLeftAimUpPose, held, newlyPressed));

    /// <summary>The input-transition list of the turning left to right aim up poses.</summary>
    internal static SamusPoseInputList TurningLeftToRightAimUp { get; } = new(nameof(TurningLeftToRightAimUp),
        (held, newlyPressed) => SamusPoseInputRules.MatchAimedTurningRightToLeft(SamusPoseId.TurningRightToLeftAimUpPose, MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored());

    /// <summary>The input-transition list of the turning right to left aim diagonal down poses.</summary>
    internal static SamusPoseInputList TurningRightToLeftAimDiagonalDown { get; } = new(nameof(TurningRightToLeftAimDiagonalDown),
        (held, newlyPressed) => SamusPoseInputRules.MatchAimedTurningRightToLeft(SamusPoseId.TurningRightToLeftAimDiagonalDownPose, held, newlyPressed));

    /// <summary>The input-transition list of the turning left to right aim diagonal down poses.</summary>
    internal static SamusPoseInputList TurningLeftToRightAimDiagonalDown { get; } = new(nameof(TurningLeftToRightAimDiagonalDown),
        (held, newlyPressed) => SamusPoseInputRules.MatchAimedTurningRightToLeft(SamusPoseId.TurningRightToLeftAimDiagonalDownPose, MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored());

    /// <summary>The input-transition list of the shinespark windup right poses.</summary>
    internal static SamusPoseInputList ShinesparkWindupRight { get; } = new(nameof(ShinesparkWindupRight),
        (held, _) => SamusPoseInputRules.MatchShinesparkWindupRight(held));

    /// <summary>The input-transition list of the shinespark windup left poses.</summary>
    internal static SamusPoseInputList ShinesparkWindupLeft { get; } = new(nameof(ShinesparkWindupLeft),
        (held, _) => SamusPoseInputRules.MatchShinesparkWindupRight(MirrorButtons(held)).Mirrored());

    /// <summary>The input-transition list of the falling aim down right poses.</summary>
    internal static SamusPoseInputList FallingAimDownRight { get; } = new(nameof(FallingAimDownRight),
        (held, newlyPressed) => SamusPoseInputRules.MatchFallingAimDownRight(held, newlyPressed));

    /// <summary>The input-transition list of the falling aim down left poses.</summary>
    internal static SamusPoseInputList FallingAimDownLeft { get; } = new(nameof(FallingAimDownLeft),
        (held, newlyPressed) => SamusPoseInputRules.MatchFallingAimDownRight(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored());

    /// <summary>The input-transition list of the left-facing ball pose Draygon holds Samus in.</summary>
    internal static SamusPoseInputList DraygonHeldBallLeft { get; } = new(nameof(DraygonHeldBallLeft),
        (_, newlyPressed) => SamusPoseInputRules.MatchDraygonHeldBallLeft(newlyPressed));

    /// <summary>The input-transition list of every left-facing pose Draygon's grab holds Samus in.</summary>
    internal static SamusPoseInputList DraygonGrabbedLeft { get; } = new(nameof(DraygonGrabbedLeft),
        (held, _) => SamusPoseInputRules.MatchDraygonGrabbedRight(MirrorButtons(held)).Mirrored());

    /// <summary>The input-transition list of every right-facing pose Draygon's grab holds Samus in.</summary>
    internal static SamusPoseInputList DraygonGrabbedRight { get; } = new(nameof(DraygonGrabbedRight),
        (held, _) => SamusPoseInputRules.MatchDraygonGrabbedRight(held));

    /// <summary>The input-transition list of the moving right gun extended poses.</summary>
    internal static SamusPoseInputList MovingRightGunExtended { get; } = new(nameof(MovingRightGunExtended),
        (held, newlyPressed) => SamusPoseInputRules.MatchMovingRightGunExtended(held, newlyPressed));

    /// <summary>The input-transition list of the moving left gun extended poses.</summary>
    internal static SamusPoseInputList MovingLeftGunExtended { get; } = new(nameof(MovingLeftGunExtended),
        (held, newlyPressed) => SamusPoseInputRules.MatchMovingRightGunExtended(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored());

    /// <summary>The input-transition list of the falling gun extended right poses.</summary>
    internal static SamusPoseInputList FallingGunExtendedRight { get; } = new(nameof(FallingGunExtendedRight),
        (held, _) => SamusPoseInputRules.MatchFallingGunExtendedRight(held));

    /// <summary>The input-transition list of the falling gun extended left poses.</summary>
    internal static SamusPoseInputList FallingGunExtendedLeft { get; } = new(nameof(FallingGunExtendedLeft),
        (held, _) => SamusPoseInputRules.MatchFallingGunExtendedRight(MirrorButtons(held)).Mirrored());

    /// <summary>The input-transition list of the moonwalk turn jump left poses.</summary>
    internal static SamusPoseInputList MoonwalkTurnToSpinJumpLeft { get; } = new(nameof(MoonwalkTurnToSpinJumpLeft),
        (held, newlyPressed) => SamusPoseInputRules.MatchMoonwalkTurnToSpinJumpRight(MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored());

    /// <summary>The input-transition list of the moonwalk turn jump right poses.</summary>
    internal static SamusPoseInputList MoonwalkTurnToSpinJumpRight { get; } = new(nameof(MoonwalkTurnToSpinJumpRight),
        (held, newlyPressed) => SamusPoseInputRules.MatchMoonwalkTurnToSpinJumpRight(held, newlyPressed));

    /// <summary>The input-transition list of the moonwalk turn jump aim up left poses.</summary>
    internal static SamusPoseInputList MoonwalkTurnToSpinJumpAimUpLeft { get; } = new(nameof(MoonwalkTurnToSpinJumpAimUpLeft),
        (held, newlyPressed) => SamusPoseInputRules.MatchMoonwalkTurnToSpinJumpAimedRight(SamusPoseId.MoonwalkTurnToSpinJumpAimUpRightPose, MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored());

    /// <summary>The input-transition list of the moonwalk turn jump aim up right poses.</summary>
    internal static SamusPoseInputList MoonwalkTurnToSpinJumpAimUpRight { get; } = new(nameof(MoonwalkTurnToSpinJumpAimUpRight),
        (held, newlyPressed) => SamusPoseInputRules.MatchMoonwalkTurnToSpinJumpAimedRight(SamusPoseId.MoonwalkTurnToSpinJumpAimUpRightPose, held, newlyPressed));

    /// <summary>The input-transition list of the moonwalk turn jump aim down left poses.</summary>
    internal static SamusPoseInputList MoonwalkTurnToSpinJumpAimDownLeft { get; } = new(nameof(MoonwalkTurnToSpinJumpAimDownLeft),
        (held, newlyPressed) => SamusPoseInputRules.MatchMoonwalkTurnToSpinJumpAimedRight(SamusPoseId.MoonwalkTurnToSpinJumpAimDownRightPose, MirrorButtons(held), MirrorButtons(newlyPressed)).Mirrored());

    /// <summary>The input-transition list of the moonwalk turn jump aim down right poses.</summary>
    internal static SamusPoseInputList MoonwalkTurnToSpinJumpAimDownRight { get; } = new(nameof(MoonwalkTurnToSpinJumpAimDownRight),
        (held, newlyPressed) => SamusPoseInputRules.MatchMoonwalkTurnToSpinJumpAimedRight(SamusPoseId.MoonwalkTurnToSpinJumpAimDownRightPose, held, newlyPressed));

    /// <summary>Selects the first satisfied input chord of the list, in the order the list authors them.</summary>
    /// <param name="held">The held actions the chords read.</param>
    /// <param name="newlyPressed">The newly pressed actions the chords read.</param>
    internal SamusPoseInputMatch Match(GameplayAction held, GameplayAction newlyPressed) => _match(held, newlyPressed);

    /// <summary>
    /// The poses the list can select, found the first time it is asked about by reading the list under every held chord
    /// its transitions read and every part of that chord newly pressed. It is worked out from the list alone, so a snapshot
    /// neither holds nor works it out.
    /// </summary>
    [field: NonSerialized]
    internal FrozenSet<SamusPoseId> SelectableTargets => field ??= SamusPoseInputTransitions.Selectable(this);


    /// <summary>Names the list.</summary>
    public override string ToString() => _name;
}

using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;
using K = SuperMetroid.Core.Enemies.SpacePirates.NinjaSpacePirateCueKind;
using P = SuperMetroid.Core.Enemies.SpacePirates.NinjaSpacePirateSegment;
using SuperMetroid.Core.Enemies.Artwork;
using Beats = SuperMetroid.Core.Animation.AnimationBeats<SuperMetroid.Core.Enemies.Artwork.EnemyFrameId, SuperMetroid.Core.Enemies.SpacePirates.NinjaSpacePirateCue>;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The ninja Space Pirates' stance, kick, claw, spin-jump and divekick animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="NinjaSpacePirateCue"/> with the values it acts on.
/// </summary>
internal static class NinjaSpacePirateAnimations
{
    /// <summary>The updates each of the ninja's ready poses shows for.</summary>
    private const int ReadyPoseUpdates = 10;

    /// <summary>The updates each of the ninja's running poses shows for.</summary>
    private const int RunningPoseUpdates = 5;

    /// <summary>The updates each of the ninja's spinning poses shows for.</summary>
    private const int SpinningPoseUpdates = 1;

    /// <summary>Where the ninja's first claw throw flies from, ahead of a right-facing pirate.</summary>
    private static Offset16<RoomPixels> HighThrow { get; } = new(32, -8);

    /// <summary>Where the ninja's second claw throw flies from, ahead of a right-facing pirate.</summary>
    private static Offset16<RoomPixels> LowThrow { get; } = new(16, 8);

    /// <summary>The ninja's segments facing left.</summary>
    private static FacedSegments LeftSegments { get; } = new(
        P.ClawAttackLeft, P.SpinJumpLeft, P.SpinJumpLeftSpinning, P.ActiveFacingLeft, P.ActiveFacingLeftStance, P.FlinchFacingLeft,
        P.DivekickLeftJump, P.DivekickLeftJumpFlash, P.DivekickLeftDive, P.WalkToLeftPost, P.RunningToLeftPost,
        P.AwaitingSamusFacingLeft, P.LookingAroundFacingLeft, P.LandFacingLeft, P.LandFacingLeftReady, P.KickFacingLeft);

    /// <summary>The ninja's segments facing right.</summary>
    private static FacedSegments RightSegments { get; } = new(
        P.ClawAttackRight, P.SpinJumpRight, P.SpinJumpRightSpinning, P.ActiveFacingRight, P.ActiveFacingRightStance, P.FlinchFacingRight,
        P.DivekickRightJump, P.DivekickRightJumpFlash, P.DivekickRightDive, P.WalkToRightPost, P.RunningToRightPost,
        P.AwaitingSamusFacingRight, P.LookingAroundFacingRight, P.LandFacingRight, P.LandFacingRightReady, P.KickFacingRight);

    /// <summary>
    /// Every segment's beats, each written once over a side (#2557): a landing turns to face the other way, and the
    /// right-facing claw attack's second throw sounds a pose later (#2451).
    /// </summary>
    internal static AnimationSegments<NinjaSpacePirateSegment, EnemyFrameId, NinjaSpacePirateCue> Segments { get; } = new(
        FacedBeats(NinjaSpacePirateSide.Left, NinjaSpacePirateSide.Right, LeftSegments, ClawThrow)
            .Concat(FacedBeats(NinjaSpacePirateSide.Right, NinjaSpacePirateSide.Left, RightSegments, ClawThrowSoundingLate))
            .ToDictionary());

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at, each segment played whole; a loop plays on
    /// from the entry before it (#2276). Each facing starts alike.
    /// </summary>
    private static Dictionary<NinjaSpacePirateSegment, Animation<NinjaSpacePirateSegment>> StartingAt { get; } =
        FacedStarts(LeftSegments).Concat(FacedStarts(RightSegments)).ToDictionary();

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<NinjaSpacePirateSegment> StartingWith(NinjaSpacePirateSegment segment) => StartingAt[segment];

    /// <summary>The segments a ninja plays facing one way.</summary>
    private sealed record FacedSegments(
        P ClawAttack, P SpinJump, P SpinJumpSpinning, P Active, P ActiveStance, P Flinch, P DivekickJump, P DivekickJumpFlash,
        P DivekickDive, P WalkToPost, P RunningToPost, P AwaitingSamus, P LookingAround, P Land, P LandReady, P Kick);

    /// <summary>
    /// The beats of <paramref name="segments"/>, the ninja facing as <paramref name="side"/> does: its claw attack throws
    /// twice, the second throw as <paramref name="secondThrow"/> plays it, and its landing turns to face as
    /// <paramref name="opposite"/> does.
    /// </summary>
    private static Dictionary<P, AnimationBeat<EnemyFrameId, NinjaSpacePirateCue>[]> FacedBeats(NinjaSpacePirateSide side,
        NinjaSpacePirateSide opposite, FacedSegments segments,
        Func<NinjaSpacePirateClawPoses, Offset16<RoomPixels>, AnimationBeat<EnemyFrameId, NinjaSpacePirateCue>[]> secondThrow) => new()
    {
        [segments.ClawAttack] =
        [
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.StandingStill)),
            .. ClawThrow(side.Claw, side.Ahead(HighThrow)),
            .. secondThrow(side.Claw, side.Ahead(LowThrow)),
        ],
        [segments.SpinJump] =
        [
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.StandingStill)),
            .. ClawThrow(side.Claw, side.Ahead(HighThrow)),
            Cue(new NinjaSpacePirateCue.Signal(K.ResetSpinSpeed)),
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.StandingStill)),
            Show(side.Claw.Crouch, 8),
            Cue(new NinjaSpacePirateCue.BeginHeading(NinjaSpacePirateAction.SpinJumpRising, side.Facing)),
        ],
        [segments.SpinJumpSpinning] =
        [
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.SpinJump)),
            .. Beats.Steady(SpinningPoseUpdates, side.Spinning),
        ],
        [segments.Active] =
        [
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.Active)),
        ],
        [segments.ActiveStance] =
        [
            .. Ready(side),
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.StandingStill)),
            Cue(new NinjaSpacePirateCue.Signal(K.StartActive)),
        ],
        [segments.Flinch] =
        [
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.StandingStill)),
            Show(side.Flinch, 16),
        ],
        [segments.DivekickJump] =
        [
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.StandingStill)),
            Show(side.LeapCrouch, 8),
            Cue(new NinjaSpacePirateCue.Signal(K.StartDivekick)),
            Cue(new NinjaSpacePirateCue.BeginHeading(NinjaSpacePirateAction.DivekickJump, side.Facing)),
        ],
        [segments.DivekickJumpFlash] =
        [
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.Stance)),
            Show(side.TuckedJump, 4),
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.GoldWallFlash)),
            Show(side.TuckedJump, 4),
        ],
        [segments.DivekickDive] =
        [
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.GoldWallFlash)),
            Cue(new NinjaSpacePirateCue.BeginHeading(NinjaSpacePirateAction.DivekickDive, side.Facing)),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Hold(side.DiveKick),
        ],
        [segments.WalkToPost] =
        [
            Cue(new NinjaSpacePirateCue.BeginHeading(NinjaSpacePirateAction.DivekickWalkToPost, side.Facing)),
        ],
        [segments.RunningToPost] = Beats.Steady(RunningPoseUpdates, side.Running),
        [segments.AwaitingSamus] =
        [
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.AwaitingSamus)),
        ],
        [segments.LookingAround] =
        [
            Show(side.LookingAtViewer, 32),
            Show(side.Standing, 10),
            Show(side.LookingAway, 32),
            Show(side.Standing, 10),
            Show(side.LookingAtViewer, 32),
        ],
        [segments.Land] =
        [
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.Stance)),
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.StandingStill)),
            Show(side.Standing, 4),
            Show(side.LookingAtViewer, 8),
            Show(EnemyFrameId.NinjaPirateFacingViewer, 4),
            Show(opposite.Standing, 4),
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.ReadyToDivekick)),
        ],
        [segments.LandReady] = Ready(opposite),
        [segments.Kick] =
        [
            Cue(new NinjaSpacePirateCue.Begin(NinjaSpacePirateAction.StandingStill)),
            Show(side.KickChambered, 4),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(side.KickExtending, 4),
            Show(side.KickExtended, 32),
            Show(side.KickExtending, 4),
        ],
    };

    /// <summary>The animations a ninja facing one way starts at its <paramref name="segments"/>.</summary>
    private static Dictionary<P, Animation<P>> FacedStarts(FacedSegments segments) => new()
    {
        [segments.ClawAttack] = Active([segments.ClawAttack], segments.Active, segments.ActiveStance),
        [segments.SpinJump] = Animation<P>.LeadingInto([Segments.Whole(segments.SpinJump)], [Segments.Whole(segments.SpinJumpSpinning)]),
        [segments.Active] = Active([], segments.Active, segments.ActiveStance),
        [segments.Flinch] = Active([segments.Flinch], segments.Active, segments.ActiveStance),
        [segments.DivekickJump] = Animation<P>.LeadingInto([Segments.Whole(segments.DivekickJump)], [Segments.Whole(segments.DivekickJumpFlash)]),
        [segments.DivekickDive] = new([Segments.Whole(segments.DivekickDive)], AnimationEnd.Holds),
        [segments.WalkToPost] = Animation<P>.LeadingInto([Segments.Whole(segments.WalkToPost)], [Segments.Whole(segments.RunningToPost)]),
        [segments.AwaitingSamus] = Animation<P>.LeadingInto([Segments.Whole(segments.AwaitingSamus)], [Segments.Whole(segments.LookingAround)]),
        [segments.Land] = Animation<P>.LeadingInto([Segments.Whole(segments.Land)], [Segments.Whole(segments.LandReady)]),
        [segments.Kick] = Active([segments.Kick], segments.Active, segments.ActiveStance),
    };

    /// <summary>The ready stance <paramref name="side"/> shows, its feet close, apart, wide and apart again.</summary>
    private static AnimationBeat<EnemyFrameId, NinjaSpacePirateCue>[] Ready(NinjaSpacePirateSide side) =>
        Beats.Steady(ReadyPoseUpdates, side.ReadyFeetClose, side.ReadyFeetApart, side.ReadyFeetWide, side.ReadyFeetApart);

    /// <summary>
    /// Plays each of <paramref name="before"/> whole, then the active stance's entry, <paramref name="entry"/>, once and its
    /// stance <paramref name="stance"/> for good (#2414).
    /// </summary>
    private static Animation<P> Active(IReadOnlyList<P> before, P entry, P stance) =>
        Animation<P>.LeadingInto([.. before.Select(Segments.Whole), Segments.Whole(entry)], [Segments.Whole(stance)]);

    /// <summary>
    /// A claw throw on <paramref name="poses"/> (#2451): the wind-up, the claw thrown from <paramref name="offset"/> the way the
    /// pirate faces with the attack sound, and the follow-through.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, NinjaSpacePirateCue>[] ClawThrow(NinjaSpacePirateClawPoses poses, Offset16<RoomPixels> offset) =>
    [
        .. ClawWindUp(poses),
        Cue(new NinjaSpacePirateCue.ThrowClaw(Throw: poses.Facing, Offset: offset)),
        Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
        Show(poses.ClawsReady, 2),
        Show(poses.ClawsSweptDown, 2),
        Show(poses.UprightClawsLowered, 2),
    ];

    /// <summary>
    /// A claw throw as <see cref="ClawThrow"/> plays it, but with the attack sound a pose later, after the follow-through's
    /// first pose (#2451): the right-facing claw attack's second throw sounds so.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, NinjaSpacePirateCue>[] ClawThrowSoundingLate(NinjaSpacePirateClawPoses poses, Offset16<RoomPixels> offset) =>
    [
        .. ClawWindUp(poses),
        Cue(new NinjaSpacePirateCue.ThrowClaw(Throw: poses.Facing, Offset: offset)),
        Show(poses.ClawsReady, 2),
        Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
        Show(poses.ClawsSweptDown, 2),
        Show(poses.UprightClawsLowered, 2),
    ];

    /// <summary>A claw throw's wind-up on <paramref name="poses"/>: crouching, then raising the claw overhead and swinging it forward.</summary>
    private static AnimationBeat<EnemyFrameId, NinjaSpacePirateCue>[] ClawWindUp(NinjaSpacePirateClawPoses poses) =>
    [
        Show(poses.Crouch, 5),
        Show(poses.ClawsReady, 5),
        Show(poses.ClawDrawnBack, 5),
        Show(poses.ClawRaisedHigh, 5),
        Show(poses.ClawRaisedOverhead, 5),
        Show(poses.ClawSwungForward, 2),
    ];

    /// <summary>Shows <paramref name="frame"/> for good: a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyFrameId, NinjaSpacePirateCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, NinjaSpacePirateCue>.HeldFrame(frame);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NinjaSpacePirateCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NinjaSpacePirateCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, NinjaSpacePirateCue> Cue(NinjaSpacePirateCue cue) => new AnimationBeat<EnemyFrameId, NinjaSpacePirateCue>.Event(cue);
}

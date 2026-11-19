using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.SpacePirates.WallSpacePirateCueKind;
using P = SuperMetroid.Core.Enemies.SpacePirates.WallSpacePirateSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The wall Space Pirates' climb, jump and laser animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="WallSpacePirateCue"/> with the values it acts on.
/// </summary>
internal static class WallSpacePirateAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<WallSpacePirateSegment, EnemyFrameId, WallSpacePirateCue> Segments { get; } = new(
        new Dictionary<WallSpacePirateSegment, AnimationBeat<EnemyFrameId, WallSpacePirateCue>[]>
    {
        [P.FireAndJumpLeft] =
        [
            Cue(new WallSpacePirateCue.Begin(WallSpacePirateAction.Attacking)),
            Show(EnemyFrameId.WallPirateOnRightWallClawsTogether, 9),
            Show(EnemyFrameId.WallPirateOnRightWallClawsSpread, 15),
            Cue(new WallSpacePirateCue.FireLaser(HorizontalDirection.Left)),
            Wait(32),
            Cue(new WallSpacePirateCue.PrepareJump(HorizontalDirection.Left)),
            Cue(new WallSpacePirateCue.BeginOnWall(WallSpacePirateAction.WallJumping, HorizontalDirection.Left)),
            Cue(new WallSpacePirateCue.Signal(K.PlayAttackSound)),
            Show(EnemyFrameId.WallPirateReachingForLeftWall, 10),
            Hold(EnemyFrameId.WallPirateCurledLeapingLeft),
        ],
        [P.LandedOnLeftWall] =
        [
            Cue(new WallSpacePirateCue.BeginOnWall(WallSpacePirateAction.Climbing, HorizontalDirection.Left)),
            Show(EnemyFrameId.WallPirateReachingForLeftWall, 10),
        ],
        [P.MovingUpLeftWall] =
        [
            Cue(new WallSpacePirateCue.BeginOnWall(WallSpacePirateAction.Climbing, HorizontalDirection.Left)),
        ],
        [P.ClimbingUpLeftWall] = Climbing(WallSpacePirateClimbPoses.LeftWall, -ClimbStepPixels),
        [P.ChooseLeftWallClimb] =
        [
            Cue(new WallSpacePirateCue.ChooseClimb(HorizontalDirection.Left)),
        ],
        [P.MovingDownLeftWall] =
        [
            Cue(new WallSpacePirateCue.BeginOnWall(WallSpacePirateAction.Climbing, HorizontalDirection.Left)),
        ],
        [P.ClimbingDownLeftWall] = Climbing(WallSpacePirateClimbPoses.LeftWall, ClimbStepPixels),
        [P.FireAndJumpRight] =
        [
            Cue(new WallSpacePirateCue.Begin(WallSpacePirateAction.Attacking)),
            Show(EnemyFrameId.WallPirateOnLeftWallClawsTogether, 9),
            Show(EnemyFrameId.WallPirateOnLeftWallClawsSpread, 1),
            Cue(new WallSpacePirateCue.FireLaser(HorizontalDirection.Right)),
            Wait(32),
            Cue(new WallSpacePirateCue.PrepareJump(HorizontalDirection.Right)),
            Cue(new WallSpacePirateCue.BeginOnWall(WallSpacePirateAction.WallJumping, HorizontalDirection.Right)),
            Cue(new WallSpacePirateCue.Signal(K.PlayAttackSound)),
            Show(EnemyFrameId.WallPirateReachingForRightWall, 10),
            Hold(EnemyFrameId.WallPirateCurledLeapingRight),
        ],
        [P.LandedOnRightWall] =
        [
            Cue(new WallSpacePirateCue.BeginOnWall(WallSpacePirateAction.Climbing, HorizontalDirection.Right)),
            Show(EnemyFrameId.WallPirateReachingForRightWall, 10),
        ],
        [P.MovingDownRightWall] =
        [
            Cue(new WallSpacePirateCue.BeginOnWall(WallSpacePirateAction.Climbing, HorizontalDirection.Right)),
        ],
        [P.ClimbingDownRightWall] = Climbing(WallSpacePirateClimbPoses.RightWall, ClimbStepPixels),
        [P.ChooseRightWallClimb] =
        [
            Cue(new WallSpacePirateCue.ChooseClimb(HorizontalDirection.Right)),
        ],
        [P.MovingUpRightWall] =
        [
            Cue(new WallSpacePirateCue.BeginOnWall(WallSpacePirateAction.Climbing, HorizontalDirection.Right)),
        ],
        [P.ClimbingUpRightWall] = Climbing(WallSpacePirateClimbPoses.RightWall, -ClimbStepPixels),
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    private static IReadOnlyDictionary<WallSpacePirateSegment, Animation<WallSpacePirateSegment>> StartingAt { get; } = new Dictionary<WallSpacePirateSegment, Animation<WallSpacePirateSegment>>
    {
        [P.FireAndJumpLeft] = new([Segments.Whole(P.FireAndJumpLeft)], AnimationEnd.Holds),
        [P.LandedOnLeftWall] = new([Segments.Whole(P.LandedOnLeftWall), .. Climb(P.MovingUpLeftWall, P.ClimbingUpLeftWall, P.ChooseLeftWallClimb)], AnimationEnd.Holds),
        [P.MovingUpLeftWall] = new(Climb(P.MovingUpLeftWall, P.ClimbingUpLeftWall, P.ChooseLeftWallClimb), AnimationEnd.Holds),
        [P.MovingDownLeftWall] = new(Climb(P.MovingDownLeftWall, P.ClimbingDownLeftWall, P.ChooseLeftWallClimb), AnimationEnd.Holds),
        [P.FireAndJumpRight] = new([Segments.Whole(P.FireAndJumpRight)], AnimationEnd.Holds),
        [P.LandedOnRightWall] = new([Segments.Whole(P.LandedOnRightWall), .. Climb(P.MovingDownRightWall, P.ClimbingDownRightWall, P.ChooseRightWallClimb)], AnimationEnd.Holds),
        [P.MovingDownRightWall] = new(Climb(P.MovingDownRightWall, P.ClimbingDownRightWall, P.ChooseRightWallClimb), AnimationEnd.Holds),
        [P.MovingUpRightWall] = new(Climb(P.MovingUpRightWall, P.ClimbingUpRightWall, P.ChooseRightWallClimb), AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<WallSpacePirateSegment> StartingWith(WallSpacePirateSegment segment) => StartingAt[segment];

    /// <summary>
    /// A climb (#2276): its entry cue <paramref name="entry"/>, the climb <paramref name="climb"/> four times, then the
    /// choice <paramref name="choice"/> of the next.
    /// </summary>
    private static AnimationPhase<WallSpacePirateSegment>[] Climb(WallSpacePirateSegment entry, WallSpacePirateSegment climb, WallSpacePirateSegment choice) =>
        [Segments.Whole(entry), Segments.Repeated(climb, ClimbRepeats), Segments.Whole(choice)];

    /// <summary>The times a climb plays before the pirate chooses the next.</summary>
    private const int ClimbRepeats = 4;

    /// <summary>The whole pixels each step of a climb moves the pirate along its wall.</summary>
    private const int ClimbStepPixels = 3;

    /// <summary>
    /// A climb on <paramref name="poses"/>'s wall (#2451): its poses in order, each after the first a step of
    /// <paramref name="climbPixels"/> down the wall (negative up) later, so a climb up and a climb down one wall show the
    /// same poses.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, WallSpacePirateCue>[] Climbing(WallSpacePirateClimbPoses poses, int climbPixels) =>
    [
        Show(poses.LegsHanging, 10),
        Cue(new WallSpacePirateCue.Climb(poses.Wall, climbPixels)),
        Show(poses.KneesBending, 8),
        Cue(new WallSpacePirateCue.Climb(poses.Wall, climbPixels)),
        Show(poses.KneesTucked, 5),
        Cue(new WallSpacePirateCue.Climb(poses.Wall, climbPixels)),
        Show(poses.LegsPushing, 8),
        Cue(new WallSpacePirateCue.Climb(poses.Wall, climbPixels)),
        Show(poses.LegsStretched, 10),
        Cue(new WallSpacePirateCue.Climb(poses.Wall, climbPixels)),
        Show(poses.LegsPushing, 8),
        Cue(new WallSpacePirateCue.Climb(poses.Wall, climbPixels)),
        Show(poses.KneesTucked, 5),
        Cue(new WallSpacePirateCue.Climb(poses.Wall, climbPixels)),
        Show(poses.KneesBending, 8),
    ];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, WallSpacePirateCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, WallSpacePirateCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good: a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyFrameId, WallSpacePirateCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, WallSpacePirateCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, WallSpacePirateCue> Cue(WallSpacePirateCue cue) => new AnimationBeat<EnemyFrameId, WallSpacePirateCue>.Event(cue);

    /// <summary>Waits <paramref name="updates"/> updates on the current frame.</summary>
    private static AnimationBeat<EnemyFrameId, WallSpacePirateCue> Wait(int updates) => new AnimationBeat<EnemyFrameId, WallSpacePirateCue>.Pause(updates);
}

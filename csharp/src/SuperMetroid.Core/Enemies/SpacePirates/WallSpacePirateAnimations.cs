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
            Show(EnemyFrameId.WallPirateCurledLeapingLeft, 1),
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
        [P.ClimbingUpLeftWall] =
        [
            Show(EnemyFrameId.WallPirateOnLeftWallLegsHanging, 10),
            Cue(new WallSpacePirateCue.Climb(Wall: HorizontalDirection.Left, ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateOnLeftWallKneesBending, 8),
            Cue(new WallSpacePirateCue.Climb(Wall: HorizontalDirection.Left, ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateOnLeftWallKneesTucked, 5),
            Cue(new WallSpacePirateCue.Climb(Wall: HorizontalDirection.Left, ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateOnLeftWallLegsPushing, 8),
            Cue(new WallSpacePirateCue.Climb(Wall: HorizontalDirection.Left, ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateOnLeftWallLegsStretched, 10),
            Cue(new WallSpacePirateCue.Climb(Wall: HorizontalDirection.Left, ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateOnLeftWallLegsPushing, 8),
            Cue(new WallSpacePirateCue.Climb(Wall: HorizontalDirection.Left, ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateOnLeftWallKneesTucked, 5),
            Cue(new WallSpacePirateCue.Climb(Wall: HorizontalDirection.Left, ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateOnLeftWallKneesBending, 8),
        ],
        [P.ChooseLeftWallClimb] =
        [
            Cue(new WallSpacePirateCue.ChooseClimb(HorizontalDirection.Left)),
        ],
        [P.MovingDownLeftWall] =
        [
            Cue(new WallSpacePirateCue.BeginOnWall(WallSpacePirateAction.Climbing, HorizontalDirection.Left)),
        ],
        [P.ClimbingDownLeftWall] =
        [
            Show(EnemyFrameId.WallPirateOnLeftWallLegsHanging, 10),
            Cue(new WallSpacePirateCue.Climb(Wall: HorizontalDirection.Left, ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateOnLeftWallKneesBending, 8),
            Cue(new WallSpacePirateCue.Climb(Wall: HorizontalDirection.Left, ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateOnLeftWallKneesTucked, 5),
            Cue(new WallSpacePirateCue.Climb(Wall: HorizontalDirection.Left, ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateOnLeftWallLegsPushing, 8),
            Cue(new WallSpacePirateCue.Climb(Wall: HorizontalDirection.Left, ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateOnLeftWallLegsStretched, 10),
            Cue(new WallSpacePirateCue.Climb(Wall: HorizontalDirection.Left, ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateOnLeftWallLegsPushing, 8),
            Cue(new WallSpacePirateCue.Climb(Wall: HorizontalDirection.Left, ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateOnLeftWallKneesTucked, 5),
            Cue(new WallSpacePirateCue.Climb(Wall: HorizontalDirection.Left, ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateOnLeftWallKneesBending, 8),
        ],
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
            Show(EnemyFrameId.WallPirateCurledLeapingRight, 1),
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
        [P.ClimbingDownRightWall] =
        [
            Show(EnemyFrameId.WallPirateOnRightWallLegsHanging, 10),
            Cue(new WallSpacePirateCue.Climb(Wall: HorizontalDirection.Right, ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateOnRightWallKneesBending, 8),
            Cue(new WallSpacePirateCue.Climb(Wall: HorizontalDirection.Right, ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateOnRightWallKneesTucked, 5),
            Cue(new WallSpacePirateCue.Climb(Wall: HorizontalDirection.Right, ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateOnRightWallLegsPushing, 8),
            Cue(new WallSpacePirateCue.Climb(Wall: HorizontalDirection.Right, ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateOnRightWallLegsStretched, 10),
            Cue(new WallSpacePirateCue.Climb(Wall: HorizontalDirection.Right, ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateOnRightWallLegsPushing, 8),
            Cue(new WallSpacePirateCue.Climb(Wall: HorizontalDirection.Right, ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateOnRightWallKneesTucked, 5),
            Cue(new WallSpacePirateCue.Climb(Wall: HorizontalDirection.Right, ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateOnRightWallKneesBending, 8),
        ],
        [P.ChooseRightWallClimb] =
        [
            Cue(new WallSpacePirateCue.ChooseClimb(HorizontalDirection.Right)),
        ],
        [P.MovingUpRightWall] =
        [
            Cue(new WallSpacePirateCue.BeginOnWall(WallSpacePirateAction.Climbing, HorizontalDirection.Right)),
        ],
        [P.ClimbingUpRightWall] =
        [
            Show(EnemyFrameId.WallPirateOnRightWallLegsHanging, 10),
            Cue(new WallSpacePirateCue.Climb(Wall: HorizontalDirection.Right, ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateOnRightWallKneesBending, 8),
            Cue(new WallSpacePirateCue.Climb(Wall: HorizontalDirection.Right, ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateOnRightWallKneesTucked, 5),
            Cue(new WallSpacePirateCue.Climb(Wall: HorizontalDirection.Right, ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateOnRightWallLegsPushing, 8),
            Cue(new WallSpacePirateCue.Climb(Wall: HorizontalDirection.Right, ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateOnRightWallLegsStretched, 10),
            Cue(new WallSpacePirateCue.Climb(Wall: HorizontalDirection.Right, ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateOnRightWallLegsPushing, 8),
            Cue(new WallSpacePirateCue.Climb(Wall: HorizontalDirection.Right, ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateOnRightWallKneesTucked, 5),
            Cue(new WallSpacePirateCue.Climb(Wall: HorizontalDirection.Right, ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateOnRightWallKneesBending, 8),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<WallSpacePirateSegment, Animation<WallSpacePirateSegment>> StartingAt { get; } = new Dictionary<WallSpacePirateSegment, Animation<WallSpacePirateSegment>>
    {
        [P.FireAndJumpLeft] = new([Whole(P.FireAndJumpLeft)], AnimationEnd.Holds),
        [P.LandedOnLeftWall] = new([Whole(P.LandedOnLeftWall), .. Climb(P.MovingUpLeftWall, P.ClimbingUpLeftWall, P.ChooseLeftWallClimb)], AnimationEnd.Holds),
        [P.MovingUpLeftWall] = new(Climb(P.MovingUpLeftWall, P.ClimbingUpLeftWall, P.ChooseLeftWallClimb), AnimationEnd.Holds),
        [P.MovingDownLeftWall] = new(Climb(P.MovingDownLeftWall, P.ClimbingDownLeftWall, P.ChooseLeftWallClimb), AnimationEnd.Holds),
        [P.FireAndJumpRight] = new([Whole(P.FireAndJumpRight)], AnimationEnd.Holds),
        [P.LandedOnRightWall] = new([Whole(P.LandedOnRightWall), .. Climb(P.MovingDownRightWall, P.ClimbingDownRightWall, P.ChooseRightWallClimb)], AnimationEnd.Holds),
        [P.MovingDownRightWall] = new(Climb(P.MovingDownRightWall, P.ClimbingDownRightWall, P.ChooseRightWallClimb), AnimationEnd.Holds),
        [P.MovingUpRightWall] = new(Climb(P.MovingUpRightWall, P.ClimbingUpRightWall, P.ChooseRightWallClimb), AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<WallSpacePirateSegment> StartingWith(WallSpacePirateSegment segment) => StartingAt[segment];

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<WallSpacePirateSegment> Whole(WallSpacePirateSegment segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>
    /// A climb (#2276): its entry cue <paramref name="entry"/>, the climb <paramref name="climb"/> four times, then the
    /// choice <paramref name="choice"/> of the next.
    /// </summary>
    private static AnimationPhase<WallSpacePirateSegment>[] Climb(WallSpacePirateSegment entry, WallSpacePirateSegment climb, WallSpacePirateSegment choice) =>
        [Whole(entry), new(climb, 0, Segments.Beats(climb).Count, ClimbRepeats), Whole(choice)];

    /// <summary>The times a climb plays before the pirate chooses the next.</summary>
    private const int ClimbRepeats = 4;

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, WallSpacePirateCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, WallSpacePirateCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, WallSpacePirateCue> Cue(WallSpacePirateCue cue) => new AnimationBeat<EnemyFrameId, WallSpacePirateCue>.Event(cue);

    /// <summary>Waits <paramref name="updates"/> updates on the current frame.</summary>
    private static AnimationBeat<EnemyFrameId, WallSpacePirateCue> Wait(int updates) => new AnimationBeat<EnemyFrameId, WallSpacePirateCue>.Pause(updates);
}

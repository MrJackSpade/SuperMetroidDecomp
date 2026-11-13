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
            Cue(new WallSpacePirateCue.SetAction(Action: WallSpacePirateAction.AttackAnimationOnRightWall)),
            Show(EnemyFrameId.WallPirateFireAndJumpLeftFirst, 9),
            Show(EnemyFrameId.WallPirateFireAndJumpLeftSecond, 15),
            Cue(new WallSpacePirateCue.Signal(K.FireLaserLeft)),
            Wait(32),
            Cue(new WallSpacePirateCue.Signal(K.PrepareJumpLeft)),
            Cue(new WallSpacePirateCue.SetAction(Action: WallSpacePirateAction.WallJumpingLeft)),
            Cue(new WallSpacePirateCue.Signal(K.PlayAttackSound)),
            Show(EnemyFrameId.WallPirateFireAndJumpLeftThird, 10),
            Show(EnemyFrameId.WallPirateFireAndJumpLeftFourth, 1),
        ],
        [P.LandedOnLeftWall] =
        [
            Cue(new WallSpacePirateCue.SetAction(Action: WallSpacePirateAction.ClimbingLeftWall)),
            Show(EnemyFrameId.WallPirateFireAndJumpLeftThird, 10),
        ],
        [P.MovingUpLeftWall] =
        [
            Cue(new WallSpacePirateCue.SetAction(Action: WallSpacePirateAction.ClimbingLeftWall)),
        ],
        [P.MovingUpLeftWallLoop] =
        [
            Show(EnemyFrameId.WallPirateMovingUpLeftWallLoopFirst, 10),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateMovingUpLeftWallLoopSecond, 8),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateMovingUpLeftWallLoopThird, 5),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateMovingUpLeftWallLoopFourth, 8),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateMovingUpLeftWallLoopFifth, 10),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateMovingUpLeftWallLoopFourth, 8),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateMovingUpLeftWallLoopThird, 5),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateMovingUpLeftWallLoopSecond, 8),
            Cue(new WallSpacePirateCue.Signal(K.ChooseLeftWallDirection)),
        ],
        [P.MovingDownLeftWall] =
        [
            Cue(new WallSpacePirateCue.SetAction(Action: WallSpacePirateAction.ClimbingLeftWall)),
        ],
        [P.MovingDownLeftWallLoop] =
        [
            Show(EnemyFrameId.WallPirateMovingUpLeftWallLoopFirst, 10),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateMovingUpLeftWallLoopSecond, 8),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateMovingUpLeftWallLoopThird, 5),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateMovingUpLeftWallLoopFourth, 8),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateMovingUpLeftWallLoopFifth, 10),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateMovingUpLeftWallLoopFourth, 8),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateMovingUpLeftWallLoopThird, 5),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateMovingUpLeftWallLoopSecond, 8),
            Cue(new WallSpacePirateCue.Signal(K.ChooseLeftWallDirection)),
        ],
        [P.FireAndJumpRight] =
        [
            Cue(new WallSpacePirateCue.SetAction(Action: WallSpacePirateAction.AttackAnimationOnLeftWall)),
            Show(EnemyFrameId.WallPirateFireAndJumpRightFirst, 9),
            Show(EnemyFrameId.WallPirateFireAndJumpRightSecond, 1),
            Cue(new WallSpacePirateCue.Signal(K.FireLaserRight)),
            Wait(32),
            Cue(new WallSpacePirateCue.Signal(K.PrepareJumpRight)),
            Cue(new WallSpacePirateCue.SetAction(Action: WallSpacePirateAction.WallJumpingRight)),
            Cue(new WallSpacePirateCue.Signal(K.PlayAttackSound)),
            Show(EnemyFrameId.WallPirateFireAndJumpRightThird, 10),
            Show(EnemyFrameId.WallPirateFireAndJumpRightFourth, 1),
        ],
        [P.LandedOnRightWall] =
        [
            Cue(new WallSpacePirateCue.SetAction(Action: WallSpacePirateAction.ClimbingRightWall)),
            Show(EnemyFrameId.WallPirateFireAndJumpRightThird, 10),
        ],
        [P.MovingDownRightWall] =
        [
            Cue(new WallSpacePirateCue.SetAction(Action: WallSpacePirateAction.ClimbingRightWall)),
        ],
        [P.MovingDownRightWallLoop] =
        [
            Show(EnemyFrameId.WallPirateMovingDownRightWallLoopFirst, 10),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateMovingDownRightWallLoopSecond, 8),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateMovingDownRightWallLoopThird, 5),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateMovingDownRightWallLoopFourth, 8),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateMovingDownRightWallLoopFifth, 10),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateMovingDownRightWallLoopFourth, 8),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateMovingDownRightWallLoopThird, 5),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateMovingDownRightWallLoopSecond, 8),
            Cue(new WallSpacePirateCue.Signal(K.ChooseRightWallDirection)),
        ],
        [P.MovingUpRightWall] =
        [
            Cue(new WallSpacePirateCue.SetAction(Action: WallSpacePirateAction.ClimbingRightWall)),
        ],
        [P.MovingUpRightWallLoop] =
        [
            Show(EnemyFrameId.WallPirateMovingDownRightWallLoopFirst, 10),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateMovingDownRightWallLoopSecond, 8),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateMovingDownRightWallLoopThird, 5),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateMovingDownRightWallLoopFourth, 8),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateMovingDownRightWallLoopFifth, 10),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateMovingDownRightWallLoopFourth, 8),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateMovingDownRightWallLoopThird, 5),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateMovingDownRightWallLoopSecond, 8),
            Cue(new WallSpacePirateCue.Signal(K.ChooseRightWallDirection)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<WallSpacePirateSegment, Animation<WallSpacePirateSegment>> StartingAt { get; } = new Dictionary<WallSpacePirateSegment, Animation<WallSpacePirateSegment>>
    {
        [P.FireAndJumpLeft] = new([new(P.FireAndJumpLeft, 0, 10, 1)], AnimationEnd.Holds),
        [P.LandedOnLeftWall] = new([new(P.LandedOnLeftWall, 0, 2, 1), new(P.MovingUpLeftWall, 0, 1, 1), new(P.MovingUpLeftWallLoop, 0, 15, 4), new(P.MovingUpLeftWallLoop, 15, 1, 1)], AnimationEnd.Holds),
        [P.MovingUpLeftWall] = new([new(P.MovingUpLeftWall, 0, 1, 1), new(P.MovingUpLeftWallLoop, 0, 15, 4), new(P.MovingUpLeftWallLoop, 15, 1, 1)], AnimationEnd.Holds),
        [P.MovingDownLeftWall] = new([new(P.MovingDownLeftWall, 0, 1, 1), new(P.MovingDownLeftWallLoop, 0, 15, 4), new(P.MovingDownLeftWallLoop, 15, 1, 1)], AnimationEnd.Holds),
        [P.FireAndJumpRight] = new([new(P.FireAndJumpRight, 0, 10, 1)], AnimationEnd.Holds),
        [P.LandedOnRightWall] = new([new(P.LandedOnRightWall, 0, 2, 1), new(P.MovingDownRightWall, 0, 1, 1), new(P.MovingDownRightWallLoop, 0, 15, 4), new(P.MovingDownRightWallLoop, 15, 1, 1)], AnimationEnd.Holds),
        [P.MovingDownRightWall] = new([new(P.MovingDownRightWall, 0, 1, 1), new(P.MovingDownRightWallLoop, 0, 15, 4), new(P.MovingDownRightWallLoop, 15, 1, 1)], AnimationEnd.Holds),
        [P.MovingUpRightWall] = new([new(P.MovingUpRightWall, 0, 1, 1), new(P.MovingUpRightWallLoop, 0, 15, 4), new(P.MovingUpRightWallLoop, 15, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<WallSpacePirateSegment> StartingWith(WallSpacePirateSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, WallSpacePirateCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, WallSpacePirateCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, WallSpacePirateCue> Cue(WallSpacePirateCue cue) => new AnimationBeat<EnemyFrameId, WallSpacePirateCue>.Event(cue);

    /// <summary>Waits <paramref name="updates"/> updates on the current frame.</summary>
    private static AnimationBeat<EnemyFrameId, WallSpacePirateCue> Wait(int updates) => new AnimationBeat<EnemyFrameId, WallSpacePirateCue>.Pause(updates);
}

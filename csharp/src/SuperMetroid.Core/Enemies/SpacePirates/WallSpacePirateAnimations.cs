using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.SpacePirates.WallSpacePirateCueKind;
using P = SuperMetroid.Core.Enemies.SpacePirates.WallSpacePirateProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The wall Space Pirates' climb, jump and laser animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="WallSpacePirateCue"/> with its typed operands.
/// </summary>
internal static class WallSpacePirateAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<WallSpacePirateProgram, EnemyFrameId, WallSpacePirateCue> Segments { get; } = new(
        new Dictionary<WallSpacePirateProgram, AnimationBeat<EnemyFrameId, WallSpacePirateCue>[]>
    {
        [P.FireAndJumpLeft] =
        [
            Cue(new WallSpacePirateCue.SetFunction(Function: WallSpacePirateFunction.AttackAnimationOnRightWall)),
            Show(EnemyFrameId.WallPirateFireJumpLeft0, 9),
            Show(EnemyFrameId.WallPirateFireJumpLeft1, 15),
            Cue(new WallSpacePirateCue.Command(K.FireLaserLeft)),
            Wait(32),
            Cue(new WallSpacePirateCue.Command(K.PrepareJumpLeft)),
            Cue(new WallSpacePirateCue.SetFunction(Function: WallSpacePirateFunction.WallJumpingLeft)),
            Cue(new WallSpacePirateCue.Command(K.PlayAttackSound)),
            Show(EnemyFrameId.WallPirateFireJumpLeft2, 10),
            Show(EnemyFrameId.WallPirateFireJumpLeft3, 1),
        ],
        [P.LandedOnLeftWall] =
        [
            Cue(new WallSpacePirateCue.SetFunction(Function: WallSpacePirateFunction.ClimbingLeftWall)),
            Show(EnemyFrameId.WallPirateFireJumpLeft2, 10),
        ],
        [P.MovingUpLeftWall] =
        [
            Cue(new WallSpacePirateCue.SetFunction(Function: WallSpacePirateFunction.ClimbingLeftWall)),
        ],
        [P.MovingUpLeftWallLoop] =
        [
            Show(EnemyFrameId.WallPirateClimbLeft0, 10),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateClimbLeft1, 8),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateClimbLeft2, 5),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateClimbLeft3, 8),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateClimbLeft4, 10),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateClimbLeft3, 8),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateClimbLeft2, 5),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateClimbLeft1, 8),
            Cue(new WallSpacePirateCue.Command(K.ChooseLeftWallDirection)),
        ],
        [P.MovingDownLeftWall] =
        [
            Cue(new WallSpacePirateCue.SetFunction(Function: WallSpacePirateFunction.ClimbingLeftWall)),
        ],
        [P.MovingDownLeftWallLoop] =
        [
            Show(EnemyFrameId.WallPirateClimbLeft0, 10),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateClimbLeft1, 8),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateClimbLeft2, 5),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateClimbLeft3, 8),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateClimbLeft4, 10),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateClimbLeft3, 8),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateClimbLeft2, 5),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateClimbLeft1, 8),
            Cue(new WallSpacePirateCue.Command(K.ChooseLeftWallDirection)),
        ],
        [P.FireAndJumpRight] =
        [
            Cue(new WallSpacePirateCue.SetFunction(Function: WallSpacePirateFunction.AttackAnimationOnLeftWall)),
            Show(EnemyFrameId.WallPirateFireJumpRight0, 9),
            Show(EnemyFrameId.WallPirateFireJumpRight1, 1),
            Cue(new WallSpacePirateCue.Command(K.FireLaserRight)),
            Wait(32),
            Cue(new WallSpacePirateCue.Command(K.PrepareJumpRight)),
            Cue(new WallSpacePirateCue.SetFunction(Function: WallSpacePirateFunction.WallJumpingRight)),
            Cue(new WallSpacePirateCue.Command(K.PlayAttackSound)),
            Show(EnemyFrameId.WallPirateFireJumpRight2, 10),
            Show(EnemyFrameId.WallPirateFireJumpRight3, 1),
        ],
        [P.LandedOnRightWall] =
        [
            Cue(new WallSpacePirateCue.SetFunction(Function: WallSpacePirateFunction.ClimbingRightWall)),
            Show(EnemyFrameId.WallPirateFireJumpRight2, 10),
        ],
        [P.MovingDownRightWall] =
        [
            Cue(new WallSpacePirateCue.SetFunction(Function: WallSpacePirateFunction.ClimbingRightWall)),
        ],
        [P.MovingDownRightWallLoop] =
        [
            Show(EnemyFrameId.WallPirateClimbRight0, 10),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateClimbRight1, 8),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateClimbRight2, 5),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateClimbRight3, 8),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateClimbRight4, 10),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateClimbRight3, 8),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateClimbRight2, 5),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: 3)),
            Show(EnemyFrameId.WallPirateClimbRight1, 8),
            Cue(new WallSpacePirateCue.Command(K.ChooseRightWallDirection)),
        ],
        [P.MovingUpRightWall] =
        [
            Cue(new WallSpacePirateCue.SetFunction(Function: WallSpacePirateFunction.ClimbingRightWall)),
        ],
        [P.MovingUpRightWallLoop] =
        [
            Show(EnemyFrameId.WallPirateClimbRight0, 10),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateClimbRight1, 8),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateClimbRight2, 5),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateClimbRight3, 8),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateClimbRight4, 10),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateClimbRight3, 8),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateClimbRight2, 5),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: -3)),
            Show(EnemyFrameId.WallPirateClimbRight1, 8),
            Cue(new WallSpacePirateCue.Command(K.ChooseRightWallDirection)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<WallSpacePirateProgram, Animation<WallSpacePirateProgram>> StartingAt { get; } = new Dictionary<WallSpacePirateProgram, Animation<WallSpacePirateProgram>>
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
    internal static Animation<WallSpacePirateProgram> StartingWith(WallSpacePirateProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, WallSpacePirateCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, WallSpacePirateCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, WallSpacePirateCue> Cue(WallSpacePirateCue cue) => new AnimationBeat<EnemyFrameId, WallSpacePirateCue>.Event(cue);

    /// <summary>Waits <paramref name="updates"/> updates on the current frame.</summary>
    private static AnimationBeat<EnemyFrameId, WallSpacePirateCue> Wait(int updates) => new AnimationBeat<EnemyFrameId, WallSpacePirateCue>.Pause(updates);
}

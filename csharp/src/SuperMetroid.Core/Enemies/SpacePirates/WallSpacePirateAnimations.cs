using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.SpacePirates.WallSpacePirateCueKind;
using P = SuperMetroid.Core.Enemies.SpacePirates.WallSpacePirateProgram;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The wall Space Pirates' climb, jump and laser animations as typed programs (#641). Each
/// frame shows a <see cref="WallSpacePirateFrame"/> pose and each callback becomes a <see cref="WallSpacePirateCue"/> with its typed operands.
/// </summary>
internal static class WallSpacePirateAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<WallSpacePirateProgram, WallSpacePirateFrame, WallSpacePirateCue> Segments { get; } = new(
        new Dictionary<WallSpacePirateProgram, AnimationBeat<WallSpacePirateFrame, WallSpacePirateCue>[]>
    {
        [P.FireAndJumpLeft] =
        [
            Cue(new WallSpacePirateCue.SetFunction(Function: WallSpacePirateFunction.AttackAnimationOnRightWall)),
            Show(14, 9),
            Show(15, 15),
            Cue(new WallSpacePirateCue.Command(K.FireLaserLeft)),
            Wait(32),
            Cue(new WallSpacePirateCue.Command(K.PrepareJumpLeft)),
            Cue(new WallSpacePirateCue.SetFunction(Function: WallSpacePirateFunction.WallJumpingLeft)),
            Cue(new WallSpacePirateCue.Command(K.PlayAttackSound)),
            Show(16, 10),
            Show(17, 1),
        ],
        [P.LandedOnLeftWall] =
        [
            Cue(new WallSpacePirateCue.SetFunction(Function: WallSpacePirateFunction.ClimbingLeftWall)),
            Show(16, 10),
        ],
        [P.MovingUpLeftWall] =
        [
            Cue(new WallSpacePirateCue.SetFunction(Function: WallSpacePirateFunction.ClimbingLeftWall)),
        ],
        [P.MovingUpLeftWallLoop] =
        [
            Show(0, 10),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: -3)),
            Show(1, 8),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: -3)),
            Show(2, 5),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: -3)),
            Show(3, 8),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: -3)),
            Show(4, 10),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: -3)),
            Show(3, 8),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: -3)),
            Show(2, 5),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: -3)),
            Show(1, 8),
            Cue(new WallSpacePirateCue.Command(K.ChooseLeftWallDirection)),
        ],
        [P.MovingDownLeftWall] =
        [
            Cue(new WallSpacePirateCue.SetFunction(Function: WallSpacePirateFunction.ClimbingLeftWall)),
        ],
        [P.MovingDownLeftWallLoop] =
        [
            Show(0, 10),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: 3)),
            Show(1, 8),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: 3)),
            Show(2, 5),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: 3)),
            Show(3, 8),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: 3)),
            Show(4, 10),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: 3)),
            Show(3, 8),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: 3)),
            Show(2, 5),
            Cue(new WallSpacePirateCue.ClimbLeftWall(ClimbPixels: 3)),
            Show(1, 8),
            Cue(new WallSpacePirateCue.Command(K.ChooseLeftWallDirection)),
        ],
        [P.FireAndJumpRight] =
        [
            Cue(new WallSpacePirateCue.SetFunction(Function: WallSpacePirateFunction.AttackAnimationOnLeftWall)),
            Show(5, 9),
            Show(6, 1),
            Cue(new WallSpacePirateCue.Command(K.FireLaserRight)),
            Wait(32),
            Cue(new WallSpacePirateCue.Command(K.PrepareJumpRight)),
            Cue(new WallSpacePirateCue.SetFunction(Function: WallSpacePirateFunction.WallJumpingRight)),
            Cue(new WallSpacePirateCue.Command(K.PlayAttackSound)),
            Show(7, 10),
            Show(8, 1),
        ],
        [P.LandedOnRightWall] =
        [
            Cue(new WallSpacePirateCue.SetFunction(Function: WallSpacePirateFunction.ClimbingRightWall)),
            Show(7, 10),
        ],
        [P.MovingDownRightWall] =
        [
            Cue(new WallSpacePirateCue.SetFunction(Function: WallSpacePirateFunction.ClimbingRightWall)),
        ],
        [P.MovingDownRightWallLoop] =
        [
            Show(9, 10),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: 3)),
            Show(10, 8),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: 3)),
            Show(11, 5),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: 3)),
            Show(12, 8),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: 3)),
            Show(13, 10),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: 3)),
            Show(12, 8),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: 3)),
            Show(11, 5),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: 3)),
            Show(10, 8),
            Cue(new WallSpacePirateCue.Command(K.ChooseRightWallDirection)),
        ],
        [P.MovingUpRightWall] =
        [
            Cue(new WallSpacePirateCue.SetFunction(Function: WallSpacePirateFunction.ClimbingRightWall)),
        ],
        [P.MovingUpRightWallLoop] =
        [
            Show(9, 10),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: -3)),
            Show(10, 8),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: -3)),
            Show(11, 5),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: -3)),
            Show(12, 8),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: -3)),
            Show(13, 10),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: -3)),
            Show(12, 8),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: -3)),
            Show(11, 5),
            Cue(new WallSpacePirateCue.ClimbRightWall(ClimbPixels: -3)),
            Show(10, 8),
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

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<WallSpacePirateFrame, WallSpacePirateCue> Show(int pose, int duration) => new AnimationBeat<WallSpacePirateFrame, WallSpacePirateCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<WallSpacePirateFrame, WallSpacePirateCue> Cue(WallSpacePirateCue cue) => new AnimationBeat<WallSpacePirateFrame, WallSpacePirateCue>.Event(cue);

    /// <summary>Waits <paramref name="updates"/> updates on the current frame.</summary>
    private static AnimationBeat<WallSpacePirateFrame, WallSpacePirateCue> Wait(int updates) => new AnimationBeat<WallSpacePirateFrame, WallSpacePirateCue>.Pause(updates);
}

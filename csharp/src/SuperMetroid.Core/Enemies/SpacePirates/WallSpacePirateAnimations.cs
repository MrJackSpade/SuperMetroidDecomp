using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.SpacePirates.WallSpacePirateCueKind;
using P = SuperMetroid.Core.Enemies.SpacePirates.WallSpacePirateProgram;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The wall Space Pirates' climb, jump and laser animations as typed programs (#641), in place of their bank-$B2 lists. Generated from the verified native lists: each
/// frame shows a <see cref="WallSpacePirateFrame"/> pose and each callback becomes a <see cref="WallSpacePirateCue"/> with its typed operands.
/// </summary>
internal static class WallSpacePirateAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<WallSpacePirateProgram, WallSpacePirateFrame, WallSpacePirateCue> Programs { get; } = new(
        new Dictionary<WallSpacePirateProgram, AnimationProgramStep<WallSpacePirateProgram, WallSpacePirateFrame, WallSpacePirateCue>[]>
    {
        [P.FireAndJumpLeft] =
        [
            Cue(new(K.SetFunction, Function: WallSpacePirateFunction.AttackAnimationOnRightWall)),
            Show(14, 9),
            Show(15, 15),
            Cue(new(K.FireLaserLeft)),
            Wait(32),
            Cue(new(K.PrepareJumpLeft)),
            Cue(new(K.SetFunction, Function: WallSpacePirateFunction.WallJumpingLeft)),
            Cue(new(K.PlayAttackSound)),
            Show(16, 10),
            Show(17, 1),
            Hold,
        ],
        [P.LandedOnLeftWall] =
        [
            Cue(new(K.SetFunction, Function: WallSpacePirateFunction.ClimbingLeftWall)),
            Show(16, 10),
            FallInto(P.MovingUpLeftWall),
        ],
        [P.MovingUpLeftWall] =
        [
            Cue(new(K.SetFunction, Function: WallSpacePirateFunction.ClimbingLeftWall)),
            SetCounter(4),
            FallInto(P.MovingUpLeftWallLoop),
        ],
        [P.MovingUpLeftWallLoop] =
        [
            Show(0, 10),
            Cue(new(K.ClimbLeftWall, ClimbPixels: -3)),
            Show(1, 8),
            Cue(new(K.ClimbLeftWall, ClimbPixels: -3)),
            Show(2, 5),
            Cue(new(K.ClimbLeftWall, ClimbPixels: -3)),
            Show(3, 8),
            Cue(new(K.ClimbLeftWall, ClimbPixels: -3)),
            Show(4, 10),
            Cue(new(K.ClimbLeftWall, ClimbPixels: -3)),
            Show(3, 8),
            Cue(new(K.ClimbLeftWall, ClimbPixels: -3)),
            Show(2, 5),
            Cue(new(K.ClimbLeftWall, ClimbPixels: -3)),
            Show(1, 8),
            RepeatWhileCounting(P.MovingUpLeftWallLoop),
            Cue(new(K.ChooseLeftWallDirection)),
            Hold,
        ],
        [P.MovingDownLeftWall] =
        [
            Cue(new(K.SetFunction, Function: WallSpacePirateFunction.ClimbingLeftWall)),
            SetCounter(4),
            FallInto(P.MovingDownLeftWallLoop),
        ],
        [P.MovingDownLeftWallLoop] =
        [
            Show(0, 10),
            Cue(new(K.ClimbLeftWall, ClimbPixels: 3)),
            Show(1, 8),
            Cue(new(K.ClimbLeftWall, ClimbPixels: 3)),
            Show(2, 5),
            Cue(new(K.ClimbLeftWall, ClimbPixels: 3)),
            Show(3, 8),
            Cue(new(K.ClimbLeftWall, ClimbPixels: 3)),
            Show(4, 10),
            Cue(new(K.ClimbLeftWall, ClimbPixels: 3)),
            Show(3, 8),
            Cue(new(K.ClimbLeftWall, ClimbPixels: 3)),
            Show(2, 5),
            Cue(new(K.ClimbLeftWall, ClimbPixels: 3)),
            Show(1, 8),
            RepeatWhileCounting(P.MovingDownLeftWallLoop),
            Cue(new(K.ChooseLeftWallDirection)),
            Hold,
        ],
        [P.FireAndJumpRight] =
        [
            Cue(new(K.SetFunction, Function: WallSpacePirateFunction.AttackAnimationOnLeftWall)),
            Show(5, 9),
            Show(6, 1),
            Cue(new(K.FireLaserRight)),
            Wait(32),
            Cue(new(K.PrepareJumpRight)),
            Cue(new(K.SetFunction, Function: WallSpacePirateFunction.WallJumpingRight)),
            Cue(new(K.PlayAttackSound)),
            Show(7, 10),
            Show(8, 1),
            Hold,
        ],
        [P.LandedOnRightWall] =
        [
            Cue(new(K.SetFunction, Function: WallSpacePirateFunction.ClimbingRightWall)),
            Show(7, 10),
            FallInto(P.MovingDownRightWall),
        ],
        [P.MovingDownRightWall] =
        [
            Cue(new(K.SetFunction, Function: WallSpacePirateFunction.ClimbingRightWall)),
            SetCounter(4),
            FallInto(P.MovingDownRightWallLoop),
        ],
        [P.MovingDownRightWallLoop] =
        [
            Show(9, 10),
            Cue(new(K.ClimbRightWall, ClimbPixels: 3)),
            Show(10, 8),
            Cue(new(K.ClimbRightWall, ClimbPixels: 3)),
            Show(11, 5),
            Cue(new(K.ClimbRightWall, ClimbPixels: 3)),
            Show(12, 8),
            Cue(new(K.ClimbRightWall, ClimbPixels: 3)),
            Show(13, 10),
            Cue(new(K.ClimbRightWall, ClimbPixels: 3)),
            Show(12, 8),
            Cue(new(K.ClimbRightWall, ClimbPixels: 3)),
            Show(11, 5),
            Cue(new(K.ClimbRightWall, ClimbPixels: 3)),
            Show(10, 8),
            RepeatWhileCounting(P.MovingDownRightWallLoop),
            Cue(new(K.ChooseRightWallDirection)),
            Hold,
        ],
        [P.MovingUpRightWall] =
        [
            Cue(new(K.SetFunction, Function: WallSpacePirateFunction.ClimbingRightWall)),
            SetCounter(4),
            FallInto(P.MovingUpRightWallLoop),
        ],
        [P.MovingUpRightWallLoop] =
        [
            Show(9, 10),
            Cue(new(K.ClimbRightWall, ClimbPixels: -3)),
            Show(10, 8),
            Cue(new(K.ClimbRightWall, ClimbPixels: -3)),
            Show(11, 5),
            Cue(new(K.ClimbRightWall, ClimbPixels: -3)),
            Show(12, 8),
            Cue(new(K.ClimbRightWall, ClimbPixels: -3)),
            Show(13, 10),
            Cue(new(K.ClimbRightWall, ClimbPixels: -3)),
            Show(12, 8),
            Cue(new(K.ClimbRightWall, ClimbPixels: -3)),
            Show(11, 5),
            Cue(new(K.ClimbRightWall, ClimbPixels: -3)),
            Show(10, 8),
            RepeatWhileCounting(P.MovingUpRightWallLoop),
            Cue(new(K.ChooseRightWallDirection)),
            Hold,
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params WallSpacePirateProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => WallSpacePirateFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<WallSpacePirateProgram, WallSpacePirateFrame, WallSpacePirateCue> Show(int pose, int duration) => AnimationProgramStep<WallSpacePirateProgram, WallSpacePirateFrame, WallSpacePirateCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<WallSpacePirateProgram, WallSpacePirateFrame, WallSpacePirateCue> Cue(WallSpacePirateCue cue) => AnimationProgramStep<WallSpacePirateProgram, WallSpacePirateFrame, WallSpacePirateCue>.Publish(cue);

    /// <summary>Loads the loop counter with <paramref name="count"/>.</summary>
    private static AnimationProgramStep<WallSpacePirateProgram, WallSpacePirateFrame, WallSpacePirateCue> SetCounter(int count) => AnimationProgramStep<WallSpacePirateProgram, WallSpacePirateFrame, WallSpacePirateCue>.SetCounter(count);

    /// <summary>Repeats from <paramref name="program"/> while the loop counter counts down.</summary>
    private static AnimationProgramStep<WallSpacePirateProgram, WallSpacePirateFrame, WallSpacePirateCue> RepeatWhileCounting(WallSpacePirateProgram program) => AnimationProgramStep<WallSpacePirateProgram, WallSpacePirateFrame, WallSpacePirateCue>.RepeatWhileCounting(program);

    /// <summary>Waits <paramref name="updates"/> updates on the current frame.</summary>
    private static AnimationProgramStep<WallSpacePirateProgram, WallSpacePirateFrame, WallSpacePirateCue> Wait(int updates) => AnimationProgramStep<WallSpacePirateProgram, WallSpacePirateFrame, WallSpacePirateCue>.Wait(updates);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<WallSpacePirateProgram, WallSpacePirateFrame, WallSpacePirateCue> FallInto(WallSpacePirateProgram program) => AnimationProgramStep<WallSpacePirateProgram, WallSpacePirateFrame, WallSpacePirateCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<WallSpacePirateProgram, WallSpacePirateFrame, WallSpacePirateCue> Hold => AnimationProgramStep<WallSpacePirateProgram, WallSpacePirateFrame, WallSpacePirateCue>.Hold;

}

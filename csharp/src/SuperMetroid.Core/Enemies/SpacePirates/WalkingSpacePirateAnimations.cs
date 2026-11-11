using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.SpacePirates.WalkingSpacePirateCueKind;
using P = SuperMetroid.Core.Enemies.SpacePirates.WalkingSpacePirateProgram;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The walking Space Pirates' walk, look, flinch and laser animations as typed programs (#641). Each
/// frame shows a <see cref="WalkingSpacePirateFrame"/> pose and each callback becomes a <see cref="WalkingSpacePirateCue"/> with its typed operands.
/// </summary>
internal static class WalkingSpacePirateAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<WalkingSpacePirateProgram, WalkingSpacePirateFrame, WalkingSpacePirateCue> Segments { get; } = new(
        new Dictionary<WalkingSpacePirateProgram, AnimationBeat<WalkingSpacePirateFrame, WalkingSpacePirateCue>[]>
    {
        [P.FlinchFacingLeft] =
        [
            Cue(new WalkingSpacePirateCue.SetFunction(Function: WalkingSpacePirateFunction.NoOperation)),
            Show(35, 16),
        ],
        [P.FlinchFacingRight] =
        [
            Cue(new WalkingSpacePirateCue.SetFunction(Function: WalkingSpacePirateFunction.NoOperation)),
            Show(36, 16),
        ],
        [P.WalkingLeft] =
        [
            Cue(new WalkingSpacePirateCue.SetFunction(Function: WalkingSpacePirateFunction.WalkingLeft)),
        ],
        [P.WalkingLeftLoop] =
        [
            Show(0, 10),
            Show(1, 10),
            Show(2, 10),
            Show(3, 10),
            Show(4, 10),
            Show(5, 10),
            Show(6, 10),
            Show(7, 10),
        ],
        [P.FireLasersLeft] =
        [
            Cue(new WalkingSpacePirateCue.SetFunction(Function: WalkingSpacePirateFunction.AnimationOwnedNoOperation)),
            Show(16, 24),
            Show(17, 8),
            Show(18, 8),
            Show(19, 8),
            Cue(new WalkingSpacePirateCue.FireLaserLeft(LaserHeight: 8)),
            Show(20, 8),
            Cue(new WalkingSpacePirateCue.FireLaserLeft(LaserHeight: 2)),
            Show(21, 24),
            Cue(new WalkingSpacePirateCue.FireLaserLeft(LaserHeight: -8)),
            Show(20, 8),
            Show(19, 8),
            Show(18, 8),
            Show(17, 8),
            Cue(new WalkingSpacePirateCue.Command(K.ChooseMovement)),
        ],
        [P.LookingFacingLeft] =
        [
            Cue(new WalkingSpacePirateCue.SetFunction(Function: WalkingSpacePirateFunction.AnimationOwnedNoOperation)),
            Show(28, 32),
            Show(29, 10),
            Show(30, 32),
            Show(29, 10),
            Show(28, 32),
            Show(34, 8),
        ],
        [P.WalkingRight] =
        [
            Cue(new WalkingSpacePirateCue.SetFunction(Function: WalkingSpacePirateFunction.WalkingRight)),
        ],
        [P.WalkingRightLoop] =
        [
            Show(8, 10),
            Show(9, 10),
            Show(10, 10),
            Show(11, 10),
            Show(12, 10),
            Show(13, 10),
            Show(14, 10),
            Show(15, 10),
        ],
        [P.FireLasersRight] =
        [
            Cue(new WalkingSpacePirateCue.SetFunction(Function: WalkingSpacePirateFunction.AnimationOwnedNoOperation)),
            Show(22, 24),
            Show(23, 8),
            Show(24, 8),
            Show(25, 8),
            Cue(new WalkingSpacePirateCue.FireLaserRight(LaserHeight: 8)),
            Show(26, 8),
            Cue(new WalkingSpacePirateCue.FireLaserRight(LaserHeight: 2)),
            Show(27, 24),
            Cue(new WalkingSpacePirateCue.FireLaserRight(LaserHeight: -8)),
            Show(26, 8),
            Show(25, 8),
            Show(24, 8),
            Show(23, 8),
            Cue(new WalkingSpacePirateCue.Command(K.ChooseMovement)),
        ],
        [P.LookingFacingRight] =
        [
            Cue(new WalkingSpacePirateCue.SetFunction(Function: WalkingSpacePirateFunction.AnimationOwnedNoOperation)),
            Show(31, 32),
            Show(32, 10),
            Show(33, 32),
            Show(32, 10),
            Show(31, 32),
            Show(34, 8),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<WalkingSpacePirateProgram, Animation<WalkingSpacePirateProgram>> StartingAt { get; } = new Dictionary<WalkingSpacePirateProgram, Animation<WalkingSpacePirateProgram>>
    {
        [P.FlinchFacingLeft] = new([new(P.FlinchFacingLeft, 0, 2, 1), new(P.WalkingLeft, 0, 1, 1), new(P.WalkingLeftLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(2)),
        [P.FlinchFacingRight] = new([new(P.FlinchFacingRight, 0, 2, 1), new(P.WalkingRight, 0, 1, 1), new(P.WalkingRightLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(2)),
        [P.WalkingLeft] = new([new(P.WalkingLeft, 0, 1, 1), new(P.WalkingLeftLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(1)),
        [P.WalkingLeftLoop] = new([new(P.WalkingLeftLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FireLasersLeft] = new([new(P.FireLasersLeft, 0, 15, 1), new(P.LookingFacingLeft, 0, 7, 1), new(P.WalkingRight, 0, 1, 1), new(P.WalkingRightLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(3)),
        [P.LookingFacingLeft] = new([new(P.LookingFacingLeft, 0, 7, 1), new(P.WalkingRight, 0, 1, 1), new(P.WalkingRightLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(2)),
        [P.WalkingRight] = new([new(P.WalkingRight, 0, 1, 1), new(P.WalkingRightLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(1)),
        [P.WalkingRightLoop] = new([new(P.WalkingRightLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FireLasersRight] = new([new(P.FireLasersRight, 0, 15, 1), new(P.LookingFacingRight, 0, 7, 1), new(P.WalkingLeft, 0, 1, 1), new(P.WalkingLeftLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(3)),
        [P.LookingFacingRight] = new([new(P.LookingFacingRight, 0, 7, 1), new(P.WalkingLeft, 0, 1, 1), new(P.WalkingLeftLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(2)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<WalkingSpacePirateProgram> StartingWith(WalkingSpacePirateProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<WalkingSpacePirateFrame, WalkingSpacePirateCue> Show(int pose, int duration) => new AnimationBeat<WalkingSpacePirateFrame, WalkingSpacePirateCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<WalkingSpacePirateFrame, WalkingSpacePirateCue> Cue(WalkingSpacePirateCue cue) => new AnimationBeat<WalkingSpacePirateFrame, WalkingSpacePirateCue>.Event(cue);
}

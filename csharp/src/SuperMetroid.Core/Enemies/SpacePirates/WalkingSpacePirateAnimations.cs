using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.SpacePirates.WalkingSpacePirateCueKind;
using P = SuperMetroid.Core.Enemies.SpacePirates.WalkingSpacePirateProgram;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The walking Space Pirates' walk, look, flinch and laser animations as typed programs (#641), in place of their bank-$B2 lists. Generated from the verified native lists: each
/// frame shows a <see cref="WalkingSpacePirateFrame"/> pose and each callback becomes a <see cref="WalkingSpacePirateCue"/> with its typed operands.
/// </summary>
internal static class WalkingSpacePirateAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<WalkingSpacePirateProgram, WalkingSpacePirateFrame, WalkingSpacePirateCue> Programs { get; } = new(
        new Dictionary<WalkingSpacePirateProgram, AnimationProgramStep<WalkingSpacePirateProgram, WalkingSpacePirateFrame, WalkingSpacePirateCue>[]>
    {
        [P.FlinchFacingLeft] =
        [
            Cue(new(K.SetFunction, Function: WalkingSpacePirateFunction.NoOperation)),
            Show(35, 16),
            GoTo(P.WalkingLeft),
        ],
        [P.FlinchFacingRight] =
        [
            Cue(new(K.SetFunction, Function: WalkingSpacePirateFunction.NoOperation)),
            Show(36, 16),
            GoTo(P.WalkingRight),
        ],
        [P.WalkingLeft] =
        [
            Cue(new(K.SetFunction, Function: WalkingSpacePirateFunction.WalkingLeft)),
            FallInto(P.WalkingLeftLoop),
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
            GoTo(P.WalkingLeftLoop),
        ],
        [P.FireLasersLeft] =
        [
            Cue(new(K.SetFunction, Function: WalkingSpacePirateFunction.AnimationOwnedNoOperation)),
            Show(16, 24),
            Show(17, 8),
            Show(18, 8),
            Show(19, 8),
            Cue(new(K.FireLaserLeft, LaserHeight: 8)),
            Show(20, 8),
            Cue(new(K.FireLaserLeft, LaserHeight: 2)),
            Show(21, 24),
            Cue(new(K.FireLaserLeft, LaserHeight: -8)),
            Show(20, 8),
            Show(19, 8),
            Show(18, 8),
            Show(17, 8),
            Cue(new(K.ChooseMovement)),
            FallInto(P.LookingFacingLeft),
        ],
        [P.LookingFacingLeft] =
        [
            Cue(new(K.SetFunction, Function: WalkingSpacePirateFunction.AnimationOwnedNoOperation)),
            Show(28, 32),
            Show(29, 10),
            Show(30, 32),
            Show(29, 10),
            Show(28, 32),
            Show(34, 8),
            GoTo(P.WalkingRight),
        ],
        [P.WalkingRight] =
        [
            Cue(new(K.SetFunction, Function: WalkingSpacePirateFunction.WalkingRight)),
            FallInto(P.WalkingRightLoop),
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
            GoTo(P.WalkingRightLoop),
        ],
        [P.FireLasersRight] =
        [
            Cue(new(K.SetFunction, Function: WalkingSpacePirateFunction.AnimationOwnedNoOperation)),
            Show(22, 24),
            Show(23, 8),
            Show(24, 8),
            Show(25, 8),
            Cue(new(K.FireLaserRight, LaserHeight: 8)),
            Show(26, 8),
            Cue(new(K.FireLaserRight, LaserHeight: 2)),
            Show(27, 24),
            Cue(new(K.FireLaserRight, LaserHeight: -8)),
            Show(26, 8),
            Show(25, 8),
            Show(24, 8),
            Show(23, 8),
            Cue(new(K.ChooseMovement)),
            FallInto(P.LookingFacingRight),
        ],
        [P.LookingFacingRight] =
        [
            Cue(new(K.SetFunction, Function: WalkingSpacePirateFunction.AnimationOwnedNoOperation)),
            Show(31, 32),
            Show(32, 10),
            Show(33, 32),
            Show(32, 10),
            Show(31, 32),
            Show(34, 8),
            GoTo(P.WalkingLeft),
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params WalkingSpacePirateProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => WalkingSpacePirateFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<WalkingSpacePirateProgram, WalkingSpacePirateFrame, WalkingSpacePirateCue> Show(int pose, int duration) => AnimationProgramStep<WalkingSpacePirateProgram, WalkingSpacePirateFrame, WalkingSpacePirateCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<WalkingSpacePirateProgram, WalkingSpacePirateFrame, WalkingSpacePirateCue> Cue(WalkingSpacePirateCue cue) => AnimationProgramStep<WalkingSpacePirateProgram, WalkingSpacePirateFrame, WalkingSpacePirateCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<WalkingSpacePirateProgram, WalkingSpacePirateFrame, WalkingSpacePirateCue> GoTo(WalkingSpacePirateProgram program) => AnimationProgramStep<WalkingSpacePirateProgram, WalkingSpacePirateFrame, WalkingSpacePirateCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<WalkingSpacePirateProgram, WalkingSpacePirateFrame, WalkingSpacePirateCue> FallInto(WalkingSpacePirateProgram program) => AnimationProgramStep<WalkingSpacePirateProgram, WalkingSpacePirateFrame, WalkingSpacePirateCue>.FallInto(program);

}

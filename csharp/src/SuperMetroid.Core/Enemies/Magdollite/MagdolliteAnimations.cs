using K = SuperMetroid.Core.Enemies.Magdollite.MagdolliteCueKind;
using P = SuperMetroid.Core.Enemies.Magdollite.MagdolliteProgram;
using SuperMetroid.Core.Enemies.Magdollite.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// The Magdollite head's idle, submerge and emerge animations, its body's pillar stacks and its overlay's throw and cap animations as typed programs (#641), in place of their bank-$A8 lists. Generated from the verified native lists: each
/// frame shows a <see cref="MagdolliteFrame"/> pose and each callback becomes a <see cref="MagdolliteCue"/> with its typed operands.
/// </summary>
internal static class MagdolliteAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<MagdolliteProgram, MagdolliteFrame, MagdolliteCue> Programs { get; } = new(
        new Dictionary<MagdolliteProgram, AnimationProgramStep<MagdolliteProgram, MagdolliteFrame, MagdolliteCue>[]>
    {
        // $A8:AC9C
        [P.LeftIdle] =
        [
            Show(0, 13),
            Show(1, 13),
            Show(2, 13),
            Show(1, 13),
            GoTo(P.LeftIdle),
        ],
        // $A8:ACB0
        [P.LeftThrow] =
        [
            Cue(new(K.PlaySoundIfOnScreen, Value: 97)),
            Cue(new(K.SetAnimationBusy)),
            Cue(new(K.StartCooldown)),
            Show(3, 26),
            Show(4, 8),
            Cue(new(K.ShiftLeftUp4)),
            Show(5, 5),
            Cue(new(K.SpawnLava)),
            Show(5, 5),
            Cue(new(K.SpawnLava)),
            Show(5, 5),
            Cue(new(K.SpawnLava)),
            Cue(new(K.ShiftLeftUp4Again)),
            Show(6, 5),
            Cue(new(K.ClearAnimationBusy)),
            Hold,
        ],
        // $A8:ACDE
        [P.LeftSubmerge] =
        [
            Cue(new(K.SetAnimationBusy)),
            Show(1, 5),
            Cue(new(K.MoveDown2)),
            Show(1, 5),
            Show(9, 5),
            Show(10, 5),
            Cue(new(K.MoveDown2)),
            Show(10, 5),
            Cue(new(K.SinkAndShowParts)),
            SetCounter(24),
            FallInto(P.LeftRaisePillar),
        ],
        // $A8:ACFE
        [P.LeftRaisePillar] =
        [
            Cue(new(K.MoveWithPillarUp1)),
            Show(8, 1),
            RepeatWhileCounting(P.LeftRaisePillar),
            Cue(new(K.ClearAnimationBusy)),
            Hold,
        ],
        // $A8:AD0C
        [P.LeftEmerge] =
        [
            Cue(new(K.SetAnimationBusy)),
            Cue(new(K.RestoreOrigins)),
            SetCounter(24),
            FallInto(P.LeftLowerPillar),
        ],
        // $A8:AD14
        [P.LeftLowerPillar] =
        [
            Cue(new(K.MoveWithPillarDown1)),
            Show(8, 1),
            RepeatWhileCounting(P.LeftLowerPillar),
            Cue(new(K.SurfaceAndHideParts)),
            Show(10, 5),
            Cue(new(K.MoveUp2)),
            Show(10, 5),
            Show(9, 5),
            Show(1, 5),
            Cue(new(K.MoveUp2)),
            Show(1, 5),
            Cue(new(K.ClearAnimationBusy)),
            Hold,
        ],
        // $A8:AD3C
        [P.RightIdle] =
        [
            Show(11, 13),
            Show(12, 13),
            Show(13, 13),
            Show(12, 13),
            GoTo(P.RightIdle),
        ],
        // $A8:AD50
        [P.RightThrow] =
        [
            Cue(new(K.PlaySoundIfOnScreen, Value: 97)),
            Cue(new(K.SetAnimationBusy)),
            Cue(new(K.StartCooldown)),
            Show(14, 26),
            Show(15, 8),
            Cue(new(K.ShiftRightUp4)),
            Show(16, 5),
            Cue(new(K.SpawnLava)),
            Show(16, 5),
            Cue(new(K.SpawnLava)),
            Show(16, 5),
            Cue(new(K.SpawnLava)),
            Cue(new(K.ShiftRightUp8)),
            Show(17, 5),
            Cue(new(K.ClearAnimationBusy)),
            Hold,
        ],
        // $A8:AD7E
        [P.RightSubmerge] =
        [
            Cue(new(K.SetAnimationBusy)),
            Show(12, 5),
            Cue(new(K.MoveDown2)),
            Show(12, 5),
            Show(19, 5),
            Show(20, 5),
            Cue(new(K.MoveDown2)),
            Show(20, 5),
            Cue(new(K.SinkAndShowParts)),
            SetCounter(24),
            FallInto(P.RightRaisePillar),
        ],
        // $A8:AD9E
        [P.RightRaisePillar] =
        [
            Cue(new(K.MoveWithPillarUp1)),
            Show(18, 1),
            RepeatWhileCounting(P.RightRaisePillar),
            Cue(new(K.ClearAnimationBusy)),
            Hold,
        ],
        // $A8:ADAC
        [P.RightEmerge] =
        [
            Cue(new(K.SetAnimationBusy)),
            Cue(new(K.RestoreOrigins)),
            SetCounter(24),
            FallInto(P.RightLowerPillar),
        ],
        // $A8:ADB4
        [P.RightLowerPillar] =
        [
            Cue(new(K.MoveWithPillarDown1)),
            Show(18, 1),
            RepeatWhileCounting(P.RightLowerPillar),
            Cue(new(K.SurfaceAndHideParts)),
            Show(20, 5),
            Cue(new(K.MoveUp2)),
            Show(20, 5),
            Show(19, 5),
            Show(12, 5),
            Cue(new(K.MoveUp2)),
            Show(12, 5),
            Cue(new(K.ClearAnimationBusy)),
            Hold,
        ],
        // $A8:ADDC
        [P.PillarPhase0] =
        [
            Show(21, 1),
            Hold,
        ],
        // $A8:ADE2
        [P.PillarPhase1] =
        [
            Show(22, 1),
            Hold,
        ],
        // $A8:ADE8
        [P.PillarPhase2] =
        [
            Show(23, 1),
            Hold,
        ],
        // $A8:ADEE
        [P.PillarPhase3] =
        [
            Show(24, 1),
            Hold,
        ],
        // $A8:ADF4
        [P.PillarPhase4] =
        [
            Show(25, 1),
            Hold,
        ],
        // $A8:ADFA
        [P.PillarPhase5] =
        [
            Show(26, 1),
            Hold,
        ],
        // $A8:AE00
        [P.PillarPhase6] =
        [
            Show(27, 1),
            Hold,
        ],
        // $A8:AE06
        [P.PillarPhase7] =
        [
            Show(28, 1),
            Hold,
        ],
        // $A8:AE0C
        [P.PillarCap] =
        [
            Show(7, 1),
            Hold,
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params MagdolliteProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => MagdolliteFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<MagdolliteProgram, MagdolliteFrame, MagdolliteCue> Show(int pose, int duration) => AnimationProgramStep<MagdolliteProgram, MagdolliteFrame, MagdolliteCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<MagdolliteProgram, MagdolliteFrame, MagdolliteCue> Cue(MagdolliteCue cue) => AnimationProgramStep<MagdolliteProgram, MagdolliteFrame, MagdolliteCue>.Publish(cue);

    /// <summary>Loads the loop counter with <paramref name="count"/>.</summary>
    private static AnimationProgramStep<MagdolliteProgram, MagdolliteFrame, MagdolliteCue> SetCounter(int count) => AnimationProgramStep<MagdolliteProgram, MagdolliteFrame, MagdolliteCue>.SetCounter(count);

    /// <summary>Repeats from <paramref name="program"/> while the loop counter counts down.</summary>
    private static AnimationProgramStep<MagdolliteProgram, MagdolliteFrame, MagdolliteCue> RepeatWhileCounting(MagdolliteProgram program) => AnimationProgramStep<MagdolliteProgram, MagdolliteFrame, MagdolliteCue>.RepeatWhileCounting(program);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<MagdolliteProgram, MagdolliteFrame, MagdolliteCue> GoTo(MagdolliteProgram program) => AnimationProgramStep<MagdolliteProgram, MagdolliteFrame, MagdolliteCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<MagdolliteProgram, MagdolliteFrame, MagdolliteCue> FallInto(MagdolliteProgram program) => AnimationProgramStep<MagdolliteProgram, MagdolliteFrame, MagdolliteCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<MagdolliteProgram, MagdolliteFrame, MagdolliteCue> Hold => AnimationProgramStep<MagdolliteProgram, MagdolliteFrame, MagdolliteCue>.Hold;

}

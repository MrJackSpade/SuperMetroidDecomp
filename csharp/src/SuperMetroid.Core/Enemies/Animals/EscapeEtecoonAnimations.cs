using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Animals.EscapeEtecoonCueKind;
using P = SuperMetroid.Core.Enemies.Animals.EscapeEtecoonProgram;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The escaping Etecoons' typed programs (#641), in place of their bank-$B3 lists. Generated from the verified native lists: each
/// frame shows a <see cref="EscapeEtecoonFrame"/> pose and each callback becomes a <see cref="EscapeEtecoonCue"/> with its typed operands.
/// </summary>
internal static class EscapeEtecoonAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<EscapeEtecoonProgram, EscapeEtecoonFrame, EscapeEtecoonCue> Programs { get; } = new(
        new Dictionary<EscapeEtecoonProgram, AnimationProgramStep<EscapeEtecoonProgram, EscapeEtecoonFrame, EscapeEtecoonCue>[]>
    {
        [P.RunningLeftLowTide] =
        [
            Cue(new EscapeEtecoonCue.GoToIfAcidHigh(Target: P.RunningLeftHighTide)),
            FallInto(P.RunningLeftLowTideLoop),
        ],
        [P.RunningLeftLowTideLoop] =
        [
            Show(0, 5),
            Show(1, 5),
            Show(2, 5),
            Show(1, 5),
            GoTo(P.RunningLeftLowTideLoop),
        ],
        [P.RunningLeftHighTide] =
        [
            Show(0, 3),
            Show(1, 3),
            Show(2, 3),
            Show(1, 3),
            GoTo(P.RunningLeftHighTide),
        ],
        [P.RunningRightLowTide] =
        [
            Cue(new EscapeEtecoonCue.GoToIfAcidHigh(Target: P.RunningRightHighTide)),
            FallInto(P.RunningRightLowTideLoop),
        ],
        [P.RunningRightLowTideLoop] =
        [
            Show(3, 6),
            Show(4, 6),
            Show(5, 6),
            Show(4, 6),
            GoTo(P.RunningRightLowTideLoop),
        ],
        [P.RunningRightHighTide] =
        [
            Show(3, 3),
            Show(4, 3),
            Show(5, 3),
            Show(4, 3),
            GoTo(P.RunningRightHighTide),
        ],
        [P.RunningForEscape] =
        [
            Cue(new EscapeEtecoonCue.SetPreInstruction(PreInstruction: EscapeEtecoonPreInstruction.EscapeRight)),
            FallInto(P.RunningForEscapeLoop),
        ],
        [P.RunningForEscapeLoop] =
        [
            Show(3, 3),
            Show(4, 3),
            Show(5, 3),
            Show(4, 3),
            GoTo(P.RunningForEscapeLoop),
        ],
        [P.Stationary] =
        [
            Show(6, 64),
            Show(7, 8),
            Show(9, 64),
            Show(8, 8),
            GoTo(P.Stationary),
        ],
        [P.ExpressGratitudeThenEscape] =
        [
            Cue(new EscapeEtecoonCue.Command(K.ClearPreInstruction)),
            SetCounter(8),
            FallInto(P.ExpressGratitudeThenEscapeLoop),
        ],
        [P.ExpressGratitudeThenEscapeLoop] =
        [
            Show(0, 8),
            Cue(new EscapeEtecoonCue.Move(Offset: -3)),
            Show(1, 8),
            Cue(new EscapeEtecoonCue.Move(Offset: -3)),
            Show(2, 8),
            Cue(new EscapeEtecoonCue.Move(Offset: -3)),
            Show(1, 8),
            Cue(new EscapeEtecoonCue.Move(Offset: -3)),
            RepeatWhileCounting(P.ExpressGratitudeThenEscapeLoop),
            Show(6, 64),
            Show(7, 8),
            GoTo(P.RunningForEscape),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<EscapeEtecoonProgram, EscapeEtecoonFrame, EscapeEtecoonCue> Show(int pose, int duration) => AnimationProgramStep<EscapeEtecoonProgram, EscapeEtecoonFrame, EscapeEtecoonCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<EscapeEtecoonProgram, EscapeEtecoonFrame, EscapeEtecoonCue> Cue(EscapeEtecoonCue cue) => AnimationProgramStep<EscapeEtecoonProgram, EscapeEtecoonFrame, EscapeEtecoonCue>.Publish(cue);

    /// <summary>Loads the loop counter with <paramref name="count"/>.</summary>
    private static AnimationProgramStep<EscapeEtecoonProgram, EscapeEtecoonFrame, EscapeEtecoonCue> SetCounter(int count) => AnimationProgramStep<EscapeEtecoonProgram, EscapeEtecoonFrame, EscapeEtecoonCue>.SetCounter(count);

    /// <summary>Repeats from <paramref name="program"/> while the loop counter counts down.</summary>
    private static AnimationProgramStep<EscapeEtecoonProgram, EscapeEtecoonFrame, EscapeEtecoonCue> RepeatWhileCounting(EscapeEtecoonProgram program) => AnimationProgramStep<EscapeEtecoonProgram, EscapeEtecoonFrame, EscapeEtecoonCue>.RepeatWhileCounting(program);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<EscapeEtecoonProgram, EscapeEtecoonFrame, EscapeEtecoonCue> GoTo(EscapeEtecoonProgram program) => AnimationProgramStep<EscapeEtecoonProgram, EscapeEtecoonFrame, EscapeEtecoonCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<EscapeEtecoonProgram, EscapeEtecoonFrame, EscapeEtecoonCue> FallInto(EscapeEtecoonProgram program) => AnimationProgramStep<EscapeEtecoonProgram, EscapeEtecoonFrame, EscapeEtecoonCue>.FallInto(program);

}

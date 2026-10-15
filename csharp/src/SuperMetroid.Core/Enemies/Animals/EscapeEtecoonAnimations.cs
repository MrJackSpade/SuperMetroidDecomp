using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Animals.EscapeEtecoonCueKind;
using P = SuperMetroid.Core.Enemies.Animals.EscapeEtecoonProgram;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.Core.Fx;

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
        // $B3:E556
        [P.RunningLeftLowTide] =
        [
            Cue(new(K.GoToIfAcidHigh, Target: P.RunningLeftHighTide)),
            FallInto(P.RunningLeftLowTideLoop),
        ],
        // $B3:E55A
        [P.RunningLeftLowTideLoop] =
        [
            Show(0, 5),
            Show(1, 5),
            Show(2, 5),
            Show(1, 5),
            GoTo(P.RunningLeftLowTideLoop),
        ],
        // $B3:E56E
        [P.RunningLeftHighTide] =
        [
            Show(0, 3),
            Show(1, 3),
            Show(2, 3),
            Show(1, 3),
            GoTo(P.RunningLeftHighTide),
        ],
        // $B3:E582
        [P.RunningRightLowTide] =
        [
            Cue(new(K.GoToIfAcidHigh, Target: P.RunningRightHighTide)),
            FallInto(P.RunningRightLowTideLoop),
        ],
        // $B3:E586
        [P.RunningRightLowTideLoop] =
        [
            Show(3, 6),
            Show(4, 6),
            Show(5, 6),
            Show(4, 6),
            GoTo(P.RunningRightLowTideLoop),
        ],
        // $B3:E59A
        [P.RunningRightHighTide] =
        [
            Show(3, 3),
            Show(4, 3),
            Show(5, 3),
            Show(4, 3),
            GoTo(P.RunningRightHighTide),
        ],
        // $B3:E5AE
        [P.RunningForEscape] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: EscapeEtecoonPreInstruction.EscapeRight)),
            FallInto(P.RunningForEscapeLoop),
        ],
        // $B3:E5B2
        [P.RunningForEscapeLoop] =
        [
            Show(3, 3),
            Show(4, 3),
            Show(5, 3),
            Show(4, 3),
            GoTo(P.RunningForEscapeLoop),
        ],
        // $B3:E5C6
        [P.Stationary] =
        [
            Show(6, 64),
            Show(7, 8),
            Show(9, 64),
            Show(8, 8),
            GoTo(P.Stationary),
        ],
        // $B3:E5DA
        [P.ExpressGratitudeThenEscape] =
        [
            Cue(new(K.ClearPreInstruction)),
            SetCounter(8),
            FallInto(P.ExpressGratitudeThenEscapeLoop),
        ],
        // $B3:E5E0
        [P.ExpressGratitudeThenEscapeLoop] =
        [
            Show(0, 8),
            Cue(new(K.Move, Offset: -3)),
            Show(1, 8),
            Cue(new(K.Move, Offset: -3)),
            Show(2, 8),
            Cue(new(K.Move, Offset: -3)),
            Show(1, 8),
            Cue(new(K.Move, Offset: -3)),
            RepeatWhileCounting(P.ExpressGratitudeThenEscapeLoop),
            Show(6, 64),
            Show(7, 8),
            GoTo(P.RunningForEscape),
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params EscapeEtecoonProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => EscapeEtecoonFrameDefinitions.Spritemap(step.Frame));

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

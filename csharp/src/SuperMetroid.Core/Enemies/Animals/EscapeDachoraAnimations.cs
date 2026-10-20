using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Animals.EscapeDachoraCueKind;
using P = SuperMetroid.Core.Enemies.Animals.EscapeDachoraProgram;
using SuperMetroid.Core.Enemies.Animals.Artwork;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The escaping Dachora's typed programs (#641), in place of their bank-$B3 lists. Generated from the verified native lists: each
/// frame shows a <see cref="EscapeDachoraFrame"/> pose and each callback becomes a <see cref="EscapeDachoraCue"/> with its typed operands.
/// </summary>
internal static class EscapeDachoraAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<EscapeDachoraProgram, EscapeDachoraFrame, EscapeDachoraCue> Programs { get; } = new(
        new Dictionary<EscapeDachoraProgram, AnimationProgramStep<EscapeDachoraProgram, EscapeDachoraFrame, EscapeDachoraCue>[]>
    {
        [P.RunningAroundLowTide] =
        [
            SetCounter(5),
            FallInto(P.RunningAroundLowTideLeft),
        ],
        [P.RunningAroundLowTideLeft] =
        [
            Show(0, 3),
            Cue(new(K.MoveLeft)),
            Show(1, 3),
            Cue(new(K.MoveLeft)),
            Show(2, 3),
            Cue(new(K.MoveLeft)),
            Show(3, 3),
            Cue(new(K.MoveLeft)),
            Show(4, 3),
            Cue(new(K.MoveLeft)),
            Show(5, 3),
            Cue(new(K.MoveLeft)),
            Cue(new(K.GoToIfAcidHigh, Target: P.RunningAroundHighTideLeftLoop)),
            Cue(new(K.GoToIfCrittersEscaped, Target: P.RunningForEscape)),
            RepeatWhileCounting(P.RunningAroundLowTideLeft),
            SetCounter(5),
            FallInto(P.RunningAroundLowTideRight),
        ],
        [P.RunningAroundLowTideRight] =
        [
            Show(6, 3),
            Cue(new(K.MoveRight)),
            Show(7, 3),
            Cue(new(K.MoveRight)),
            Show(8, 3),
            Cue(new(K.MoveRight)),
            Show(9, 3),
            Cue(new(K.MoveRight)),
            Show(10, 3),
            Cue(new(K.MoveRight)),
            Show(11, 3),
            Cue(new(K.MoveRight)),
            Cue(new(K.GoToIfAcidHigh, Target: P.RunningAroundHighTideRightLoop)),
            Cue(new(K.GoToIfCrittersEscaped, Target: P.RunningForEscapeAccelerating)),
            RepeatWhileCounting(P.RunningAroundLowTideRight),
            GoTo(P.RunningAroundLowTide),
        ],
        [P.RunningAroundHighTide] =
        [
            SetCounter(5),
            FallInto(P.RunningAroundHighTideLeft),
        ],
        [P.RunningAroundHighTideLeft] =
        [
            Show(0, 2),
            Cue(new(K.MoveLeft)),
            Show(1, 2),
            Cue(new(K.MoveLeft)),
            Show(2, 2),
            Cue(new(K.MoveLeft)),
            Show(3, 2),
            Cue(new(K.MoveLeft)),
            Show(4, 2),
            Cue(new(K.MoveLeft)),
            Show(5, 2),
            Cue(new(K.MoveLeft)),
            Cue(new(K.GoToIfCrittersEscaped, Target: P.RunningForEscape)),
            FallInto(P.RunningAroundHighTideLeftLoop),
        ],
        [P.RunningAroundHighTideLeftLoop] =
        [
            RepeatWhileCounting(P.RunningAroundHighTideLeft),
            SetCounter(5),
            FallInto(P.RunningAroundHighTideRight),
        ],
        [P.RunningAroundHighTideRight] =
        [
            Show(6, 2),
            Cue(new(K.MoveRight)),
            Show(7, 2),
            Cue(new(K.MoveRight)),
            Show(8, 2),
            Cue(new(K.MoveRight)),
            Show(9, 2),
            Cue(new(K.MoveRight)),
            Show(10, 2),
            Cue(new(K.MoveRight)),
            Show(11, 2),
            Cue(new(K.MoveRight)),
            Cue(new(K.GoToIfCrittersEscaped, Target: P.RunningForEscapeAccelerating)),
            FallInto(P.RunningAroundHighTideRightLoop),
        ],
        [P.RunningAroundHighTideRightLoop] =
        [
            RepeatWhileCounting(P.RunningAroundHighTideRight),
            GoTo(P.RunningAroundHighTide),
        ],
        [P.RunningForEscape] =
        [
            Show(0, 30),
            FallInto(P.RunningForEscapeAccelerating),
        ],
        [P.RunningForEscapeAccelerating] =
        [
            Show(6, 90),
            Cue(new(K.MoveRight)),
            Show(7, 5),
            Cue(new(K.MoveRight)),
            Show(8, 5),
            Cue(new(K.MoveRight)),
            Show(9, 4),
            Cue(new(K.MoveRight)),
            Show(10, 4),
            Cue(new(K.MoveRight)),
            Show(11, 4),
            Cue(new(K.MoveRight)),
            Show(6, 3),
            Cue(new(K.MoveRight)),
            Show(7, 3),
            Cue(new(K.MoveRight)),
            Show(8, 3),
            Cue(new(K.MoveRight)),
            Show(9, 2),
            Cue(new(K.MoveRight)),
            Show(10, 2),
            Cue(new(K.MoveRight)),
            Show(11, 2),
            Cue(new(K.MoveRight)),
            FallInto(P.RunningForEscapeMaximumSpeed),
        ],
        [P.RunningForEscapeMaximumSpeed] =
        [
            Show(6, 1),
            Cue(new(K.MoveRight)),
            Show(7, 1),
            Cue(new(K.MoveRight)),
            Show(8, 1),
            Cue(new(K.MoveRight)),
            Show(9, 1),
            Cue(new(K.MoveRight)),
            Show(10, 1),
            Cue(new(K.MoveRight)),
            Show(11, 1),
            Cue(new(K.MoveRight)),
            GoTo(P.RunningForEscapeMaximumSpeed),
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params EscapeDachoraProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => EscapeDachoraFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<EscapeDachoraProgram, EscapeDachoraFrame, EscapeDachoraCue> Show(int pose, int duration) => AnimationProgramStep<EscapeDachoraProgram, EscapeDachoraFrame, EscapeDachoraCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<EscapeDachoraProgram, EscapeDachoraFrame, EscapeDachoraCue> Cue(EscapeDachoraCue cue) => AnimationProgramStep<EscapeDachoraProgram, EscapeDachoraFrame, EscapeDachoraCue>.Publish(cue);

    /// <summary>Loads the loop counter with <paramref name="count"/>.</summary>
    private static AnimationProgramStep<EscapeDachoraProgram, EscapeDachoraFrame, EscapeDachoraCue> SetCounter(int count) => AnimationProgramStep<EscapeDachoraProgram, EscapeDachoraFrame, EscapeDachoraCue>.SetCounter(count);

    /// <summary>Repeats from <paramref name="program"/> while the loop counter counts down.</summary>
    private static AnimationProgramStep<EscapeDachoraProgram, EscapeDachoraFrame, EscapeDachoraCue> RepeatWhileCounting(EscapeDachoraProgram program) => AnimationProgramStep<EscapeDachoraProgram, EscapeDachoraFrame, EscapeDachoraCue>.RepeatWhileCounting(program);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<EscapeDachoraProgram, EscapeDachoraFrame, EscapeDachoraCue> GoTo(EscapeDachoraProgram program) => AnimationProgramStep<EscapeDachoraProgram, EscapeDachoraFrame, EscapeDachoraCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<EscapeDachoraProgram, EscapeDachoraFrame, EscapeDachoraCue> FallInto(EscapeDachoraProgram program) => AnimationProgramStep<EscapeDachoraProgram, EscapeDachoraFrame, EscapeDachoraCue>.FallInto(program);

}

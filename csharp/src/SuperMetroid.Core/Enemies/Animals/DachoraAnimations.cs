using P = SuperMetroid.Core.Enemies.Animals.DachoraProgram;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The Dachora's run, idle, blink, shinespark charge, echo and fall animations as typed programs (#641), in place of their bank-$A7 lists. Generated from the verified native lists: each
/// frame shows a <see cref="DachoraFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class DachoraAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<DachoraProgram, DachoraFrame, NoAnimationCue> Programs { get; } = new(
        new Dictionary<DachoraProgram, AnimationProgramStep<DachoraProgram, DachoraFrame, NoAnimationCue>[]>
    {
        [P.RunningLeft] =
        [
            Show(0, 5),
            Show(1, 5),
            Show(2, 5),
            Show(3, 5),
            Show(4, 5),
            Show(5, 5),
            GoTo(P.RunningLeft),
        ],
        [P.RunningLeftFast] =
        [
            Show(0, 3),
            Show(1, 3),
            Show(2, 3),
            Show(3, 3),
            Show(4, 3),
            Show(5, 3),
            GoTo(P.RunningLeftFast),
        ],
        [P.RunningLeftVeryFast] =
        [
            Show(0, 1),
            Show(1, 1),
            Show(2, 1),
            Show(3, 1),
            Show(4, 1),
            Show(5, 1),
            GoTo(P.RunningLeftVeryFast),
        ],
        [P.IdleLeft] =
        [
            Show(7, 48),
            Show(10, 10),
            Show(11, 7),
            Show(12, 7),
            Show(13, 7),
            Show(12, 7),
            Show(11, 7),
            Show(12, 7),
            Show(13, 7),
            Show(12, 7),
            Show(10, 10),
            GoTo(P.IdleLeft),
        ],
        [P.BlinkLeft] =
        [
            Show(7, 11),
            Show(8, 8),
            Show(9, 8),
            Show(7, 4),
            Show(8, 4),
            Show(9, 4),
            Show(7, 10),
            Show(8, 5),
            Show(9, 11),
            GoTo(P.BlinkLeft),
        ],
        [P.EchoLeft] =
        [
            Show(6, 10),
            GoTo(P.EchoLeft),
        ],
        [P.FallingLeft] =
        [
            Show(4, 5),
            GoTo(P.FallingLeft),
        ],
        [P.RunningRight] =
        [
            Show(14, 5),
            Show(15, 5),
            Show(16, 5),
            Show(17, 5),
            Show(18, 5),
            Show(19, 5),
            GoTo(P.RunningRight),
        ],
        [P.RunningRightFast] =
        [
            Show(14, 3),
            Show(15, 3),
            Show(16, 3),
            Show(17, 3),
            Show(18, 3),
            Show(19, 3),
            GoTo(P.RunningRightFast),
        ],
        [P.RunningRightVeryFast] =
        [
            Show(14, 1),
            Show(15, 1),
            Show(16, 1),
            Show(17, 1),
            Show(18, 1),
            Show(19, 1),
            GoTo(P.RunningRightVeryFast),
        ],
        [P.IdleRight] =
        [
            Show(22, 48),
            Show(25, 10),
            Show(26, 7),
            Show(27, 7),
            Show(28, 7),
            Show(27, 7),
            Show(26, 7),
            Show(27, 7),
            Show(28, 7),
            Show(27, 7),
            Show(25, 10),
            GoTo(P.IdleRight),
        ],
        [P.BlinkRight] =
        [
            Show(22, 11),
            Show(23, 8),
            Show(24, 8),
            Show(22, 4),
            Show(23, 4),
            Show(24, 4),
            Show(22, 10),
            Show(23, 5),
            Show(24, 11),
            GoTo(P.BlinkRight),
        ],
        [P.ChargeRight] =
        [
            Show(20, 1),
            Hold,
        ],
        [P.EchoRight] =
        [
            Show(21, 10),
            GoTo(P.EchoRight),
        ],
        [P.FallingRight] =
        [
            Show(18, 5),
            GoTo(P.FallingRight),
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params DachoraProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => DachoraFrameDefinitions.Spritemap(step.Frame));

    /// <summary>The program a sleeping list resumes into when its owner steps the list past the sleep.</summary>
    internal static DachoraProgram AfterSleep(DachoraProgram program) => program switch
    {
        P.ChargeRight => P.EchoRight,
        P.RunningLeft or
            P.RunningLeftFast or
            P.RunningLeftVeryFast or
            P.IdleLeft or
            P.BlinkLeft or
            P.EchoLeft or
            P.FallingLeft or
            P.RunningRight or
            P.RunningRightFast or
            P.RunningRightVeryFast or
            P.IdleRight or
            P.BlinkRight or
            P.EchoRight or
            P.FallingRight => throw new ArgumentOutOfRangeException(nameof(program), program, "The program does not sleep before authored code."),
        _ => throw new ArgumentOutOfRangeException(nameof(program), program, "The program does not sleep before authored code."),
    };

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<DachoraProgram, DachoraFrame, NoAnimationCue> Show(int pose, int duration) => AnimationProgramStep<DachoraProgram, DachoraFrame, NoAnimationCue>.Show(new(pose), duration);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<DachoraProgram, DachoraFrame, NoAnimationCue> GoTo(DachoraProgram program) => AnimationProgramStep<DachoraProgram, DachoraFrame, NoAnimationCue>.GoTo(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<DachoraProgram, DachoraFrame, NoAnimationCue> Hold => AnimationProgramStep<DachoraProgram, DachoraFrame, NoAnimationCue>.Hold;

}

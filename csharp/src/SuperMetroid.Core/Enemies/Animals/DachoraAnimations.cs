using P = SuperMetroid.Core.Enemies.Animals.DachoraProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The Dachora's run, idle, blink, shinespark charge, echo and fall animations as typed programs (#641). Each
/// frame shows a <see cref="DachoraFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class DachoraAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<DachoraProgram, DachoraFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<DachoraProgram, AnimationBeat<DachoraFrame, NoAnimationCue>[]>
    {
        [P.RunningLeft] =
        [
            Show(0, 5),
            Show(1, 5),
            Show(2, 5),
            Show(3, 5),
            Show(4, 5),
            Show(5, 5),
        ],
        [P.RunningLeftFast] =
        [
            Show(0, 3),
            Show(1, 3),
            Show(2, 3),
            Show(3, 3),
            Show(4, 3),
            Show(5, 3),
        ],
        [P.RunningLeftVeryFast] =
        [
            Show(0, 1),
            Show(1, 1),
            Show(2, 1),
            Show(3, 1),
            Show(4, 1),
            Show(5, 1),
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
        ],
        [P.EchoLeft] =
        [
            Show(6, 10),
        ],
        [P.FallingLeft] =
        [
            Show(4, 5),
        ],
        [P.RunningRight] =
        [
            Show(14, 5),
            Show(15, 5),
            Show(16, 5),
            Show(17, 5),
            Show(18, 5),
            Show(19, 5),
        ],
        [P.RunningRightFast] =
        [
            Show(14, 3),
            Show(15, 3),
            Show(16, 3),
            Show(17, 3),
            Show(18, 3),
            Show(19, 3),
        ],
        [P.RunningRightVeryFast] =
        [
            Show(14, 1),
            Show(15, 1),
            Show(16, 1),
            Show(17, 1),
            Show(18, 1),
            Show(19, 1),
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
        ],
        [P.ChargeRight] =
        [
            Show(20, 1),
        ],
        [P.EchoRight] =
        [
            Show(21, 10),
        ],
        [P.FallingRight] =
        [
            Show(18, 5),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<DachoraProgram, Animation<DachoraProgram>> StartingAt { get; } = new Dictionary<DachoraProgram, Animation<DachoraProgram>>
    {
        [P.RunningLeft] = new([new(P.RunningLeft, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RunningLeftFast] = new([new(P.RunningLeftFast, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RunningLeftVeryFast] = new([new(P.RunningLeftVeryFast, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.IdleLeft] = new([new(P.IdleLeft, 0, 11, 1)], AnimationEnd.LoopsFrom(0)),
        [P.BlinkLeft] = new([new(P.BlinkLeft, 0, 9, 1)], AnimationEnd.LoopsFrom(0)),
        [P.EchoLeft] = new([new(P.EchoLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FallingLeft] = new([new(P.FallingLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RunningRight] = new([new(P.RunningRight, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RunningRightFast] = new([new(P.RunningRightFast, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RunningRightVeryFast] = new([new(P.RunningRightVeryFast, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.IdleRight] = new([new(P.IdleRight, 0, 11, 1)], AnimationEnd.LoopsFrom(0)),
        [P.BlinkRight] = new([new(P.BlinkRight, 0, 9, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargeRight] = new([new(P.ChargeRight, 0, 1, 1)], AnimationEnd.Holds),
        [P.EchoRight] = new([new(P.EchoRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FallingRight] = new([new(P.FallingRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<DachoraProgram> StartingWith(DachoraProgram segment) => StartingAt[segment];

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
    private static AnimationBeat<DachoraFrame, NoAnimationCue> Show(int pose, int duration) => new AnimationBeat<DachoraFrame, NoAnimationCue>.Frame(new(pose), duration);
}

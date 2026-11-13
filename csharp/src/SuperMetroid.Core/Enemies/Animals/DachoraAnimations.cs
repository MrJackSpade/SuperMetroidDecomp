using P = SuperMetroid.Core.Enemies.Animals.DachoraProgram;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The Dachora's run, idle, blink, shinespark charge, echo and fall animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and its lists use no owner callbacks.
/// </summary>
internal static class DachoraAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<DachoraProgram, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<DachoraProgram, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.RunningLeft] =
        [
            Show(EnemyFrameId.Dachora00, 5),
            Show(EnemyFrameId.Dachora01, 5),
            Show(EnemyFrameId.Dachora02, 5),
            Show(EnemyFrameId.Dachora03, 5),
            Show(EnemyFrameId.Dachora04, 5),
            Show(EnemyFrameId.Dachora05, 5),
        ],
        [P.RunningLeftFast] =
        [
            Show(EnemyFrameId.Dachora00, 3),
            Show(EnemyFrameId.Dachora01, 3),
            Show(EnemyFrameId.Dachora02, 3),
            Show(EnemyFrameId.Dachora03, 3),
            Show(EnemyFrameId.Dachora04, 3),
            Show(EnemyFrameId.Dachora05, 3),
        ],
        [P.RunningLeftVeryFast] =
        [
            Show(EnemyFrameId.Dachora00, 1),
            Show(EnemyFrameId.Dachora01, 1),
            Show(EnemyFrameId.Dachora02, 1),
            Show(EnemyFrameId.Dachora03, 1),
            Show(EnemyFrameId.Dachora04, 1),
            Show(EnemyFrameId.Dachora05, 1),
        ],
        [P.IdleLeft] =
        [
            Show(EnemyFrameId.Dachora06, 48),
            Show(EnemyFrameId.Dachora07, 10),
            Show(EnemyFrameId.Dachora08, 7),
            Show(EnemyFrameId.Dachora09, 7),
            Show(EnemyFrameId.Dachora10, 7),
            Show(EnemyFrameId.Dachora09, 7),
            Show(EnemyFrameId.Dachora08, 7),
            Show(EnemyFrameId.Dachora09, 7),
            Show(EnemyFrameId.Dachora10, 7),
            Show(EnemyFrameId.Dachora09, 7),
            Show(EnemyFrameId.Dachora07, 10),
        ],
        [P.BlinkLeft] =
        [
            Show(EnemyFrameId.Dachora06, 11),
            Show(EnemyFrameId.Dachora11, 8),
            Show(EnemyFrameId.Dachora12, 8),
            Show(EnemyFrameId.Dachora06, 4),
            Show(EnemyFrameId.Dachora11, 4),
            Show(EnemyFrameId.Dachora12, 4),
            Show(EnemyFrameId.Dachora06, 10),
            Show(EnemyFrameId.Dachora11, 5),
            Show(EnemyFrameId.Dachora12, 11),
        ],
        [P.EchoLeft] =
        [
            Show(EnemyFrameId.Dachora13, 10),
        ],
        [P.FallingLeft] =
        [
            Show(EnemyFrameId.Dachora04, 5),
        ],
        [P.RunningRight] =
        [
            Show(EnemyFrameId.Dachora14, 5),
            Show(EnemyFrameId.Dachora15, 5),
            Show(EnemyFrameId.Dachora16, 5),
            Show(EnemyFrameId.Dachora17, 5),
            Show(EnemyFrameId.Dachora18, 5),
            Show(EnemyFrameId.Dachora19, 5),
        ],
        [P.RunningRightFast] =
        [
            Show(EnemyFrameId.Dachora14, 3),
            Show(EnemyFrameId.Dachora15, 3),
            Show(EnemyFrameId.Dachora16, 3),
            Show(EnemyFrameId.Dachora17, 3),
            Show(EnemyFrameId.Dachora18, 3),
            Show(EnemyFrameId.Dachora19, 3),
        ],
        [P.RunningRightVeryFast] =
        [
            Show(EnemyFrameId.Dachora14, 1),
            Show(EnemyFrameId.Dachora15, 1),
            Show(EnemyFrameId.Dachora16, 1),
            Show(EnemyFrameId.Dachora17, 1),
            Show(EnemyFrameId.Dachora18, 1),
            Show(EnemyFrameId.Dachora19, 1),
        ],
        [P.IdleRight] =
        [
            Show(EnemyFrameId.Dachora20, 48),
            Show(EnemyFrameId.Dachora21, 10),
            Show(EnemyFrameId.Dachora22, 7),
            Show(EnemyFrameId.Dachora23, 7),
            Show(EnemyFrameId.Dachora24, 7),
            Show(EnemyFrameId.Dachora23, 7),
            Show(EnemyFrameId.Dachora22, 7),
            Show(EnemyFrameId.Dachora23, 7),
            Show(EnemyFrameId.Dachora24, 7),
            Show(EnemyFrameId.Dachora23, 7),
            Show(EnemyFrameId.Dachora21, 10),
        ],
        [P.BlinkRight] =
        [
            Show(EnemyFrameId.Dachora20, 11),
            Show(EnemyFrameId.Dachora25, 8),
            Show(EnemyFrameId.Dachora26, 8),
            Show(EnemyFrameId.Dachora20, 4),
            Show(EnemyFrameId.Dachora25, 4),
            Show(EnemyFrameId.Dachora26, 4),
            Show(EnemyFrameId.Dachora20, 10),
            Show(EnemyFrameId.Dachora25, 5),
            Show(EnemyFrameId.Dachora26, 11),
        ],
        [P.ChargeRight] =
        [
            Show(EnemyFrameId.Dachora27, 1),
        ],
        [P.EchoRight] =
        [
            Show(EnemyFrameId.Dachora28, 10),
        ],
        [P.FallingRight] =
        [
            Show(EnemyFrameId.Dachora18, 5),
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

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}

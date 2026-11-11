using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Skultera.SkulteraCueKind;
using P = SuperMetroid.Core.Enemies.Skultera.SkulteraProgram;

namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>
/// Skultera's swimming and turning animations as typed programs (#641). Each
/// frame shows a <see cref="SkulteraFrame"/> pose and each callback becomes a <see cref="SkulteraCue"/> with its typed operands.
/// </summary>
internal static class SkulteraAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<SkulteraProgram, SkulteraFrame, SkulteraCue> Segments { get; } = new(
        new Dictionary<SkulteraProgram, AnimationBeat<SkulteraFrame, SkulteraCue>[]>
    {
        [P.SwimmingLeft] = [],
        [P.SwimmingLeftLoop] =
        [
            Show(0, 14),
            Show(1, 14),
            Show(2, 14),
        ],
        [P.TurningRight] =
        [
            Show(3, 13),
            Show(4, 10),
            Show(5, 8),
            Show(6, 6),
            Show(7, 6),
            Show(8, 8),
            Show(9, 10),
            Show(10, 13),
            Cue(new(K.FinishTurn)),
        ],
        [P.SwimmingRight] = [],
        [P.SwimmingRightLoop] =
        [
            Show(11, 14),
            Show(12, 14),
            Show(13, 14),
        ],
        [P.TurningLeft] =
        [
            Show(14, 13),
            Show(15, 10),
            Show(16, 8),
            Show(17, 6),
            Show(18, 6),
            Show(19, 8),
            Show(20, 10),
            Show(21, 13),
            Cue(new(K.FinishTurn)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<SkulteraProgram, Animation<SkulteraProgram>> StartingAt { get; } = new Dictionary<SkulteraProgram, Animation<SkulteraProgram>>
    {
        [P.SwimmingLeft] = new([new(P.SwimmingLeftLoop, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SwimmingLeftLoop] = new([new(P.SwimmingLeftLoop, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.TurningRight] = new([new(P.TurningRight, 0, 9, 1)], AnimationEnd.Holds),
        [P.SwimmingRight] = new([new(P.SwimmingRightLoop, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SwimmingRightLoop] = new([new(P.SwimmingRightLoop, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.TurningLeft] = new([new(P.TurningLeft, 0, 9, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SkulteraProgram> StartingWith(SkulteraProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<SkulteraFrame, SkulteraCue> Show(int pose, int duration) => new AnimationBeat<SkulteraFrame, SkulteraCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<SkulteraFrame, SkulteraCue> Cue(SkulteraCue cue) => new AnimationBeat<SkulteraFrame, SkulteraCue>.Event(cue);
}

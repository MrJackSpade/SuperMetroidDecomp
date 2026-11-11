using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Rios.NorfairRioCueKind;
using P = SuperMetroid.Core.Enemies.Rios.NorfairRioProgram;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The Norfair Rio's idle and swoop animations and its flames' animations as typed programs (#641). Each
/// frame shows a <see cref="NorfairRioFrame"/> pose and each callback becomes a <see cref="NorfairRioCue"/> with its typed operands.
/// </summary>
internal static class NorfairRioAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<NorfairRioProgram, NorfairRioFrame, NorfairRioCue> Segments { get; } = new(
        new Dictionary<NorfairRioProgram, AnimationBeat<NorfairRioFrame, NorfairRioCue>[]>
    {
        [P.Idle] =
        [
            Cue(new(K.FlamesAt8)),
            Show(1, 13),
            Show(0, 18),
            Show(1, 13),
            Show(2, 18),
        ],
        [P.StartDescending] =
        [
            Cue(new(K.FlamesAt8)),
            Show(0, 1),
            Cue(new(K.FlamesAt4)),
            Show(9, 1),
            Cue(new(K.FlamesAt0)),
            Show(10, 1),
            Cue(new(K.FlamesAtMinus4)),
            Show(11, 1),
            Cue(new(K.FlamesAtMinus12)),
            Show(12, 1),
            Cue(new(K.FlamesAtMinus16)),
            Show(13, 1),
            Cue(new(K.SwoopStartFinished)),
        ],
        [P.Descending] =
        [
            Cue(new(K.FlamesAtMinus12)),
            Show(6, 6),
            Show(7, 5),
            Show(8, 8),
            Show(7, 6),
        ],
        [P.StartAscending] =
        [
            Cue(new(K.FlamesAtMinus16)),
            Show(13, 1),
            Cue(new(K.FlamesAtMinus12)),
            Show(12, 1),
            Cue(new(K.FlamesAtMinus4)),
            Show(11, 1),
            Cue(new(K.FlamesAt0)),
            Show(10, 1),
            Cue(new(K.FlamesAt4)),
            Show(9, 1),
            Cue(new(K.FlamesAt8)),
            Show(0, 1),
            Cue(new(K.FlamesAt8)),
            Show(1, 1),
            Cue(new(K.FlamesAt12)),
            Show(2, 1),
            Cue(new(K.SwoopStartFinished)),
        ],
        [P.Ascending] =
        [
            Cue(new(K.FlamesAt12)),
            Show(14, 6),
            Show(15, 5),
            Show(16, 8),
            Show(15, 6),
        ],
        [P.FlamesAscending] =
        [
            Show(3, 6),
            Show(4, 5),
            Show(5, 8),
            Show(4, 6),
        ],
        [P.FlamesDescending] =
        [
            Show(17, 6),
            Show(18, 5),
            Show(19, 8),
            Show(18, 6),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<NorfairRioProgram, Animation<NorfairRioProgram>> StartingAt { get; } = new Dictionary<NorfairRioProgram, Animation<NorfairRioProgram>>
    {
        [P.Idle] = new([new(P.Idle, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.StartDescending] = new([new(P.StartDescending, 0, 13, 1)], AnimationEnd.Holds),
        [P.Descending] = new([new(P.Descending, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.StartAscending] = new([new(P.StartAscending, 0, 17, 1)], AnimationEnd.Holds),
        [P.Ascending] = new([new(P.Ascending, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FlamesAscending] = new([new(P.FlamesAscending, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FlamesDescending] = new([new(P.FlamesDescending, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<NorfairRioProgram> StartingWith(NorfairRioProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<NorfairRioFrame, NorfairRioCue> Show(int pose, int duration) => new AnimationBeat<NorfairRioFrame, NorfairRioCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<NorfairRioFrame, NorfairRioCue> Cue(NorfairRioCue cue) => new AnimationBeat<NorfairRioFrame, NorfairRioCue>.Event(cue);
}

using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Rios.NorfairRioCueKind;
using P = SuperMetroid.Core.Enemies.Rios.NorfairRioProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The Norfair Rio's idle and swoop animations and its flames' animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="NorfairRioCue"/> with its typed operands.
/// </summary>
internal static class NorfairRioAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<NorfairRioProgram, EnemyFrameId, NorfairRioCue> Segments { get; } = new(
        new Dictionary<NorfairRioProgram, AnimationBeat<EnemyFrameId, NorfairRioCue>[]>
    {
        [P.Idle] =
        [
            Cue(new(K.FlamesAt8)),
            Show(EnemyFrameId.NorfairRioPose1, 13),
            Show(EnemyFrameId.NorfairRioPose0, 18),
            Show(EnemyFrameId.NorfairRioPose1, 13),
            Show(EnemyFrameId.NorfairRioPose2, 18),
        ],
        [P.StartDescending] =
        [
            Cue(new(K.FlamesAt8)),
            Show(EnemyFrameId.NorfairRioPose0, 1),
            Cue(new(K.FlamesAt4)),
            Show(EnemyFrameId.NorfairRioPose9, 1),
            Cue(new(K.FlamesAt0)),
            Show(EnemyFrameId.NorfairRioPose10, 1),
            Cue(new(K.FlamesAtMinus4)),
            Show(EnemyFrameId.NorfairRioPose11, 1),
            Cue(new(K.FlamesAtMinus12)),
            Show(EnemyFrameId.NorfairRioPose12, 1),
            Cue(new(K.FlamesAtMinus16)),
            Show(EnemyFrameId.NorfairRioPose13, 1),
            Cue(new(K.SwoopStartFinished)),
        ],
        [P.Descending] =
        [
            Cue(new(K.FlamesAtMinus12)),
            Show(EnemyFrameId.NorfairRioPose6, 6),
            Show(EnemyFrameId.NorfairRioPose7, 5),
            Show(EnemyFrameId.NorfairRioPose8, 8),
            Show(EnemyFrameId.NorfairRioPose7, 6),
        ],
        [P.StartAscending] =
        [
            Cue(new(K.FlamesAtMinus16)),
            Show(EnemyFrameId.NorfairRioPose13, 1),
            Cue(new(K.FlamesAtMinus12)),
            Show(EnemyFrameId.NorfairRioPose12, 1),
            Cue(new(K.FlamesAtMinus4)),
            Show(EnemyFrameId.NorfairRioPose11, 1),
            Cue(new(K.FlamesAt0)),
            Show(EnemyFrameId.NorfairRioPose10, 1),
            Cue(new(K.FlamesAt4)),
            Show(EnemyFrameId.NorfairRioPose9, 1),
            Cue(new(K.FlamesAt8)),
            Show(EnemyFrameId.NorfairRioPose0, 1),
            Cue(new(K.FlamesAt8)),
            Show(EnemyFrameId.NorfairRioPose1, 1),
            Cue(new(K.FlamesAt12)),
            Show(EnemyFrameId.NorfairRioPose2, 1),
            Cue(new(K.SwoopStartFinished)),
        ],
        [P.Ascending] =
        [
            Cue(new(K.FlamesAt12)),
            Show(EnemyFrameId.NorfairRioPose14, 6),
            Show(EnemyFrameId.NorfairRioPose15, 5),
            Show(EnemyFrameId.NorfairRioPose16, 8),
            Show(EnemyFrameId.NorfairRioPose15, 6),
        ],
        [P.FlamesAscending] =
        [
            Show(EnemyFrameId.NorfairRioPose3, 6),
            Show(EnemyFrameId.NorfairRioPose4, 5),
            Show(EnemyFrameId.NorfairRioPose5, 8),
            Show(EnemyFrameId.NorfairRioPose4, 6),
        ],
        [P.FlamesDescending] =
        [
            Show(EnemyFrameId.NorfairRioPose17, 6),
            Show(EnemyFrameId.NorfairRioPose18, 5),
            Show(EnemyFrameId.NorfairRioPose19, 8),
            Show(EnemyFrameId.NorfairRioPose18, 6),
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

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NorfairRioCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NorfairRioCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, NorfairRioCue> Cue(NorfairRioCue cue) => new AnimationBeat<EnemyFrameId, NorfairRioCue>.Event(cue);
}

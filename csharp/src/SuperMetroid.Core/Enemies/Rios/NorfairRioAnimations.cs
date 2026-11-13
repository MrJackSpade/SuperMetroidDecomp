using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Rios.NorfairRioCueKind;
using P = SuperMetroid.Core.Enemies.Rios.NorfairRioSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The Norfair Rio's idle and swoop animations and its flames' animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="NorfairRioCue"/> naming what it does.
/// </summary>
internal static class NorfairRioAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<NorfairRioSegment, EnemyFrameId, NorfairRioCue> Segments { get; } = new(
        new Dictionary<NorfairRioSegment, AnimationBeat<EnemyFrameId, NorfairRioCue>[]>
    {
        [P.Idle] =
        [
            Cue(new(K.FlamesAt8)),
            Show(EnemyFrameId.NorfairRioIdleFirst, 13),
            Show(EnemyFrameId.NorfairRioIdleSecond, 18),
            Show(EnemyFrameId.NorfairRioIdleFirst, 13),
            Show(EnemyFrameId.NorfairRioIdleThird, 18),
        ],
        [P.StartDescending] =
        [
            Cue(new(K.FlamesAt8)),
            Show(EnemyFrameId.NorfairRioIdleSecond, 1),
            Cue(new(K.FlamesAt4)),
            Show(EnemyFrameId.NorfairRioStartDescendingSecond, 1),
            Cue(new(K.FlamesAt0)),
            Show(EnemyFrameId.NorfairRioStartDescendingThird, 1),
            Cue(new(K.FlamesAtMinus4)),
            Show(EnemyFrameId.NorfairRioStartDescendingFourth, 1),
            Cue(new(K.FlamesAtMinus12)),
            Show(EnemyFrameId.NorfairRioStartDescendingFifth, 1),
            Cue(new(K.FlamesAtMinus16)),
            Show(EnemyFrameId.NorfairRioStartDescendingSixth, 1),
            Cue(new(K.SwoopStartFinished)),
        ],
        [P.Descending] =
        [
            Cue(new(K.FlamesAtMinus12)),
            Show(EnemyFrameId.NorfairRioDescendingFirst, 6),
            Show(EnemyFrameId.NorfairRioDescendingSecond, 5),
            Show(EnemyFrameId.NorfairRioDescendingThird, 8),
            Show(EnemyFrameId.NorfairRioDescendingSecond, 6),
        ],
        [P.StartAscending] =
        [
            Cue(new(K.FlamesAtMinus16)),
            Show(EnemyFrameId.NorfairRioStartDescendingSixth, 1),
            Cue(new(K.FlamesAtMinus12)),
            Show(EnemyFrameId.NorfairRioStartDescendingFifth, 1),
            Cue(new(K.FlamesAtMinus4)),
            Show(EnemyFrameId.NorfairRioStartDescendingFourth, 1),
            Cue(new(K.FlamesAt0)),
            Show(EnemyFrameId.NorfairRioStartDescendingThird, 1),
            Cue(new(K.FlamesAt4)),
            Show(EnemyFrameId.NorfairRioStartDescendingSecond, 1),
            Cue(new(K.FlamesAt8)),
            Show(EnemyFrameId.NorfairRioIdleSecond, 1),
            Cue(new(K.FlamesAt8)),
            Show(EnemyFrameId.NorfairRioIdleFirst, 1),
            Cue(new(K.FlamesAt12)),
            Show(EnemyFrameId.NorfairRioIdleThird, 1),
            Cue(new(K.SwoopStartFinished)),
        ],
        [P.Ascending] =
        [
            Cue(new(K.FlamesAt12)),
            Show(EnemyFrameId.NorfairRioAscendingFirst, 6),
            Show(EnemyFrameId.NorfairRioAscendingSecond, 5),
            Show(EnemyFrameId.NorfairRioAscendingThird, 8),
            Show(EnemyFrameId.NorfairRioAscendingSecond, 6),
        ],
        [P.FlamesAscending] =
        [
            Show(EnemyFrameId.NorfairRioFlamesAscendingFirst, 6),
            Show(EnemyFrameId.NorfairRioFlamesAscendingSecond, 5),
            Show(EnemyFrameId.NorfairRioFlamesAscendingThird, 8),
            Show(EnemyFrameId.NorfairRioFlamesAscendingSecond, 6),
        ],
        [P.FlamesDescending] =
        [
            Show(EnemyFrameId.NorfairRioFlamesDescendingFirst, 6),
            Show(EnemyFrameId.NorfairRioFlamesDescendingSecond, 5),
            Show(EnemyFrameId.NorfairRioFlamesDescendingThird, 8),
            Show(EnemyFrameId.NorfairRioFlamesDescendingSecond, 6),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<NorfairRioSegment, Animation<NorfairRioSegment>> StartingAt { get; } = new Dictionary<NorfairRioSegment, Animation<NorfairRioSegment>>
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
    internal static Animation<NorfairRioSegment> StartingWith(NorfairRioSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NorfairRioCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NorfairRioCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, NorfairRioCue> Cue(NorfairRioCue cue) => new AnimationBeat<EnemyFrameId, NorfairRioCue>.Event(cue);
}

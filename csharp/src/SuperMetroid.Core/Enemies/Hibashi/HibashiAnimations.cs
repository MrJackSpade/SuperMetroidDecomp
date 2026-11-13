using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Hibashi.HibashiCueKind;
using P = SuperMetroid.Core.Enemies.Hibashi.HibashiSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>
/// The Hibashi fire pillar's eruption graphics and its collision part as typed animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="HibashiCue"/> with the values it acts on.
/// </summary>
internal static class HibashiAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<HibashiSegment, EnemyFrameId, HibashiCue> Segments { get; } = new(
        new Dictionary<HibashiSegment, AnimationBeat<EnemyFrameId, HibashiCue>[]>
    {
        [P.Graphics] =
        [
            Cue(new(K.PlayEruptionSound)),
            Show(EnemyFrameId.HibashiEruptionFirst, 2),
            Cue(new(K.ActivityFrame, Value: 0)),
            Show(EnemyFrameId.HibashiEruptionSecond, 2),
            Cue(new(K.ActivityFrame, Value: 1)),
            Show(EnemyFrameId.HibashiEruptionThird, 2),
            Cue(new(K.ActivityFrame, Value: 2)),
            Show(EnemyFrameId.HibashiEruptionFourth, 2),
            Cue(new(K.ActivityFrame, Value: 3)),
            Show(EnemyFrameId.HibashiEruptionFifth, 1),
            Cue(new(K.ActivityFrame, Value: 4)),
            Show(EnemyFrameId.HibashiEruptionSixth, 1),
            Cue(new(K.ActivityFrame, Value: 5)),
            Show(EnemyFrameId.HibashiEruptionSeventh, 1),
            Cue(new(K.ActivityFrame, Value: 6)),
            Show(EnemyFrameId.HibashiEruptionEighth, 1),
            Cue(new(K.ActivityFrame, Value: 7)),
            Show(EnemyFrameId.HibashiEruptionNinth, 2),
            Cue(new(K.ActivityFrame, Value: 8)),
            Show(EnemyFrameId.HibashiEruptionTenth, 2),
            Cue(new(K.ActivityFrame, Value: 9)),
            Show(EnemyFrameId.HibashiEruptionEleventh, 2),
            Cue(new(K.ActivityFrame, Value: 10)),
            Show(EnemyFrameId.HibashiEruptionTwelfth, 2),
            Cue(new(K.ActivityFrame, Value: 11)),
            Show(EnemyFrameId.HibashiEruptionThirteenth, 2),
            Cue(new(K.ActivityFrame, Value: 12)),
            Show(EnemyFrameId.HibashiEruptionFourteenth, 2),
            Cue(new(K.ActivityFrame, Value: 13)),
            Show(EnemyFrameId.HibashiEruptionFifteenth, 2),
            Cue(new(K.ActivityFrame, Value: 14)),
            Show(EnemyFrameId.HibashiEruptionSixteenth, 2),
            Cue(new(K.ActivityFrame, Value: 15)),
            Show(EnemyFrameId.HibashiEruptionSeventeenth, 4),
            Cue(new(K.ActivityFrame, Value: 16)),
            Show(EnemyFrameId.HibashiEruptionEighteenth, 4),
            Cue(new(K.ActivityFrame, Value: 17)),
            Show(EnemyFrameId.HibashiEruptionNineteenth, 4),
            Cue(new(K.ActivityFrame, Value: 18)),
            Show(EnemyFrameId.HibashiEruptionTwentieth, 4),
            Cue(new(K.ActivityFrame, Value: 19)),
            Show(EnemyFrameId.HibashiEruptionTwentyFirst, 4),
            Cue(new(K.ActivityFrame, Value: 20)),
            Show(EnemyFrameId.HibashiEruptionTwentySecond, 4),
            Cue(new(K.ActivityFrame, Value: 21)),
            Show(EnemyFrameId.HibashiEruptionTwentyThird, 4),
            Cue(new(K.FinishActivity)),
        ],
        [P.Hitbox] =
        [
            Show(EnemyFrameId.HibashiEruptionFirst, 2),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<HibashiSegment, Animation<HibashiSegment>> StartingAt { get; } = new Dictionary<HibashiSegment, Animation<HibashiSegment>>
    {
        [P.Graphics] = new([new(P.Graphics, 0, 47, 1)], AnimationEnd.Holds),
        [P.Hitbox] = new([new(P.Hitbox, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<HibashiSegment> StartingWith(HibashiSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, HibashiCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, HibashiCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, HibashiCue> Cue(HibashiCue cue) => new AnimationBeat<EnemyFrameId, HibashiCue>.Event(cue);
}

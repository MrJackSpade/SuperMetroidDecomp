using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Bosses.MotherBrain.RinkaCueKind;
using P = SuperMetroid.Core.Bosses.MotherBrain.RinkaSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The ordinary and Mother Brain Rinkas' hide, fire and spin animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="RinkaCue"/> naming what it does.
/// </summary>
internal static class RinkaAnimations
{
    /// <summary>How long a Rinka waits hidden, white hot, before it fires.</summary>
    private const int HiddenWait = 64;

    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<RinkaSegment, EnemyFrameId, RinkaCue> Segments { get; } = new(
        new Dictionary<RinkaSegment, AnimationBeat<EnemyFrameId, RinkaCue>[]>
    {
        [P.Ordinary] =
        [
            Cue(new(K.Hide)),
            Show(EnemyFrameId.RinkaWhiteHot, HiddenWait),
            Cue(new(K.Fire)),
        ],
        [P.Spin] =
        [
            Show(EnemyFrameId.RinkaWhiteHot, 16),
            Show(EnemyFrameId.RinkaYellow, 8),
            Show(EnemyFrameId.RinkaOrange, 7),
            Show(EnemyFrameId.RinkaRed, 6),
            Show(EnemyFrameId.RinkaDarkRed, 5),
            Show(EnemyFrameId.RinkaRed, 6),
            Show(EnemyFrameId.RinkaOrange, 7),
            Show(EnemyFrameId.RinkaYellow, 8),
        ],
        [P.Special] =
        [
            Cue(new(K.HideAndProcessOffScreen)),
            Show(EnemyFrameId.RinkaWhiteHot, HiddenWait),
            Cue(new(K.Fire)),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at: each Rinka hides, fires and then spins
    /// for good (#2250).
    /// </summary>
    internal static IReadOnlyDictionary<RinkaSegment, Animation<RinkaSegment>> StartingAt { get; } = new Dictionary<RinkaSegment, Animation<RinkaSegment>>
    {
        [P.Ordinary] = new([Whole(P.Ordinary), Whole(P.Spin)], AnimationEnd.LoopsFrom(1)),
        [P.Special] = new([Whole(P.Special), Whole(P.Spin)], AnimationEnd.LoopsFrom(1)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<RinkaSegment> StartingWith(RinkaSegment segment) => StartingAt[segment];

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<RinkaSegment> Whole(RinkaSegment segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, RinkaCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, RinkaCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, RinkaCue> Cue(RinkaCue cue) => new AnimationBeat<EnemyFrameId, RinkaCue>.Event(cue);
}

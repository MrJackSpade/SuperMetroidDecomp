using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Bosses.MotherBrain.RinkaCueKind;
using P = SuperMetroid.Core.Bosses.MotherBrain.RinkaSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The ordinary and Mother Brain Rinkas' hide, fire and spin animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="RinkaCue"/> with its typed operands.
/// </summary>
internal static class RinkaAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<RinkaSegment, EnemyFrameId, RinkaCue> Segments { get; } = new(
        new Dictionary<RinkaSegment, AnimationBeat<EnemyFrameId, RinkaCue>[]>
    {
        [P.Ordinary] =
        [
            Cue(new(K.Hide)),
            Show(EnemyFrameId.RinkaOrdinary, 64),
            Cue(new(K.Fire)),
        ],
        [P.OrdinaryLoop] =
        [
            Show(EnemyFrameId.RinkaOrdinary, 16),
            Show(EnemyFrameId.RinkaOrdinaryLoopSecond, 8),
            Show(EnemyFrameId.RinkaOrdinaryLoopThird, 7),
            Show(EnemyFrameId.RinkaOrdinaryLoopFourth, 6),
            Show(EnemyFrameId.RinkaOrdinaryLoopFifth, 5),
            Show(EnemyFrameId.RinkaOrdinaryLoopFourth, 6),
            Show(EnemyFrameId.RinkaOrdinaryLoopThird, 7),
            Show(EnemyFrameId.RinkaOrdinaryLoopSecond, 8),
        ],
        [P.Special] =
        [
            Cue(new(K.HideAndProcessOffScreen)),
            Show(EnemyFrameId.RinkaOrdinary, 64),
            Cue(new(K.Fire)),
        ],
        [P.SpecialLoop] =
        [
            Show(EnemyFrameId.RinkaOrdinary, 16),
            Show(EnemyFrameId.RinkaOrdinaryLoopSecond, 8),
            Show(EnemyFrameId.RinkaOrdinaryLoopThird, 7),
            Show(EnemyFrameId.RinkaOrdinaryLoopFourth, 6),
            Show(EnemyFrameId.RinkaOrdinaryLoopFifth, 5),
            Show(EnemyFrameId.RinkaOrdinaryLoopFourth, 6),
            Show(EnemyFrameId.RinkaOrdinaryLoopThird, 7),
            Show(EnemyFrameId.RinkaOrdinaryLoopSecond, 8),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<RinkaSegment, Animation<RinkaSegment>> StartingAt { get; } = new Dictionary<RinkaSegment, Animation<RinkaSegment>>
    {
        [P.Ordinary] = new([new(P.Ordinary, 0, 3, 1), new(P.OrdinaryLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(1)),
        [P.OrdinaryLoop] = new([new(P.OrdinaryLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Special] = new([new(P.Special, 0, 3, 1), new(P.SpecialLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SpecialLoop] = new([new(P.SpecialLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<RinkaSegment> StartingWith(RinkaSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, RinkaCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, RinkaCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, RinkaCue> Cue(RinkaCue cue) => new AnimationBeat<EnemyFrameId, RinkaCue>.Event(cue);
}

using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Skree.SkreeMetareeCueKind;
using P = SuperMetroid.Core.Enemies.Skree.SkreeMetareeSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// The Skree's and Metaree's idling, wind-up, diving and stop animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="SkreeMetareeCue"/> with its typed operands.
/// </summary>
internal static class SkreeMetareeAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<SkreeMetareeSegment, EnemyFrameId, SkreeMetareeCue> Segments { get; } = new(
        new Dictionary<SkreeMetareeSegment, AnimationBeat<EnemyFrameId, SkreeMetareeCue>[]>
    {
        [P.MetareeIdling] =
        [
            Show(EnemyFrameId.MetareeIdlingFirst, 10),
            Show(EnemyFrameId.MetareeIdlingSecond, 10),
            Show(EnemyFrameId.MetareeIdlingThird, 10),
            Show(EnemyFrameId.MetareeIdlingFourth, 10),
        ],
        [P.MetareePreparingAttack] =
        [
            Show(EnemyFrameId.MetareeIdlingFirst, 16),
            Show(EnemyFrameId.MetareePreparingAttackSecond, 8),
            Cue(new(K.SetAttackReady)),
        ],
        [P.MetareeDiving] =
        [
            Cue(new(K.ProcessOffScreen)),
        ],
        [P.MetareeDivingLoop] =
        [
            Show(EnemyFrameId.MetareeIdlingSecond, 2),
            Show(EnemyFrameId.MetareeIdlingThird, 2),
            Show(EnemyFrameId.MetareeIdlingFourth, 2),
            Show(EnemyFrameId.MetareeIdlingFirst, 2),
        ],
        [P.MetareeStopAnimating] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Show(EnemyFrameId.MetareeIdlingFirst, 1),
        ],
        [P.SkreeIdling] =
        [
            Show(EnemyFrameId.SkreeIdlingFirst, 10),
            Show(EnemyFrameId.SkreeIdlingSecond, 10),
            Show(EnemyFrameId.SkreeIdlingThird, 10),
            Show(EnemyFrameId.SkreeIdlingFourth, 10),
        ],
        [P.SkreePreparingAttack] =
        [
            Show(EnemyFrameId.SkreeIdlingFirst, 16),
            Show(EnemyFrameId.SkreePreparingAttackSecond, 8),
            Cue(new(K.SetAttackReady)),
        ],
        [P.SkreeDiving] =
        [
            Cue(new(K.ProcessOffScreen)),
        ],
        [P.SkreeDivingLoop] =
        [
            Show(EnemyFrameId.SkreeIdlingSecond, 2),
            Show(EnemyFrameId.SkreeIdlingThird, 2),
            Show(EnemyFrameId.SkreeIdlingFourth, 2),
            Show(EnemyFrameId.SkreeIdlingFirst, 2),
        ],
        [P.SkreeStopAnimating] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Show(EnemyFrameId.SkreeIdlingFirst, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<SkreeMetareeSegment, Animation<SkreeMetareeSegment>> StartingAt { get; } = new Dictionary<SkreeMetareeSegment, Animation<SkreeMetareeSegment>>
    {
        [P.MetareeIdling] = new([new(P.MetareeIdling, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MetareePreparingAttack] = new([new(P.MetareePreparingAttack, 0, 3, 1)], AnimationEnd.Holds),
        [P.MetareeDiving] = new([new(P.MetareeDiving, 0, 1, 1), new(P.MetareeDivingLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.MetareeDivingLoop] = new([new(P.MetareeDivingLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MetareeStopAnimating] = new([new(P.MetareeStopAnimating, 0, 2, 1)], AnimationEnd.Holds),
        [P.SkreeIdling] = new([new(P.SkreeIdling, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SkreePreparingAttack] = new([new(P.SkreePreparingAttack, 0, 3, 1)], AnimationEnd.Holds),
        [P.SkreeDiving] = new([new(P.SkreeDiving, 0, 1, 1), new(P.SkreeDivingLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SkreeDivingLoop] = new([new(P.SkreeDivingLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SkreeStopAnimating] = new([new(P.SkreeStopAnimating, 0, 2, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SkreeMetareeSegment> StartingWith(SkreeMetareeSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, SkreeMetareeCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, SkreeMetareeCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, SkreeMetareeCue> Cue(SkreeMetareeCue cue) => new AnimationBeat<EnemyFrameId, SkreeMetareeCue>.Event(cue);
}

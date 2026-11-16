using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Skree.SkreeMetareeCueKind;
using P = SuperMetroid.Core.Enemies.Skree.SkreeMetareeSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// The Skree's and Metaree's idling, wind-up, diving and stop animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="SkreeMetareeCue"/> naming what it does.
/// </summary>
internal static class SkreeMetareeAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<SkreeMetareeSegment, EnemyFrameId, SkreeMetareeCue> Segments { get; } = new(
        new Dictionary<SkreeMetareeSegment, AnimationBeat<EnemyFrameId, SkreeMetareeCue>[]>
    {
        [P.MetareeIdling] =
        [
            Show(EnemyFrameId.MetareeFacingViewer, 10),
            Show(EnemyFrameId.MetareeTurningOblique, 10),
            Show(EnemyFrameId.MetareeEdgeOn, 10),
            Show(EnemyFrameId.MetareeReturningOblique, 10),
        ],
        [P.MetareePreparingAttack] =
        [
            Show(EnemyFrameId.MetareeFacingViewer, 16),
            Show(EnemyFrameId.MetareeWingsFlared, 8),
            Cue(new(K.SetAttackReady)),
        ],
        [P.MetareeDiving] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(EnemyFrameId.MetareeTurningOblique, 2),
            Show(EnemyFrameId.MetareeEdgeOn, 2),
            Show(EnemyFrameId.MetareeReturningOblique, 2),
            Show(EnemyFrameId.MetareeFacingViewer, 2),
        ],
        [P.SkreeIdling] =
        [
            Show(EnemyFrameId.SkreeFacingViewer, 10),
            Show(EnemyFrameId.SkreeTurningOblique, 10),
            Show(EnemyFrameId.SkreeEdgeOn, 10),
            Show(EnemyFrameId.SkreeReturningOblique, 10),
        ],
        [P.SkreePreparingAttack] =
        [
            Show(EnemyFrameId.SkreeFacingViewer, 16),
            Show(EnemyFrameId.SkreeWingsFlared, 8),
            Cue(new(K.SetAttackReady)),
        ],
        [P.SkreeDiving] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(EnemyFrameId.SkreeTurningOblique, 2),
            Show(EnemyFrameId.SkreeEdgeOn, 2),
            Show(EnemyFrameId.SkreeReturningOblique, 2),
            Show(EnemyFrameId.SkreeFacingViewer, 2),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<SkreeMetareeSegment, Animation<SkreeMetareeSegment>> StartingAt { get; } = new Dictionary<SkreeMetareeSegment, Animation<SkreeMetareeSegment>>
    {
        [P.MetareeIdling] = new([new(P.MetareeIdling, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MetareePreparingAttack] = new([new(P.MetareePreparingAttack, 0, 3, 1)], AnimationEnd.Holds),
        [P.MetareeDiving] = new([new(P.MetareeDiving, 0, 1, 1), new(P.MetareeDiving, 1, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SkreeIdling] = new([new(P.SkreeIdling, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SkreePreparingAttack] = new([new(P.SkreePreparingAttack, 0, 3, 1)], AnimationEnd.Holds),
        [P.SkreeDiving] = new([new(P.SkreeDiving, 0, 1, 1), new(P.SkreeDiving, 1, 4, 1)], AnimationEnd.LoopsFrom(1)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SkreeMetareeSegment> StartingWith(SkreeMetareeSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, SkreeMetareeCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, SkreeMetareeCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, SkreeMetareeCue> Cue(SkreeMetareeCue cue) => new AnimationBeat<EnemyFrameId, SkreeMetareeCue>.Event(cue);
}

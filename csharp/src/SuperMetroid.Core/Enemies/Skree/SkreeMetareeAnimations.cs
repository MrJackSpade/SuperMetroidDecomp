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
        [P.MetareeIdling] = Idling(EnemyFrameId.MetareeFacingViewer, EnemyFrameId.MetareeTurningOblique, EnemyFrameId.MetareeEdgeOn, EnemyFrameId.MetareeReturningOblique),
        [P.MetareePreparingAttack] = PreparingAttack(EnemyFrameId.MetareeFacingViewer, EnemyFrameId.MetareeWingsFlared),
        [P.MetareeDiving] = DiveStart,
        [P.MetareeDivingSpin] = DivingSpin(EnemyFrameId.MetareeTurningOblique, EnemyFrameId.MetareeEdgeOn, EnemyFrameId.MetareeReturningOblique, EnemyFrameId.MetareeFacingViewer),
        [P.SkreeIdling] = Idling(EnemyFrameId.SkreeFacingViewer, EnemyFrameId.SkreeTurningOblique, EnemyFrameId.SkreeEdgeOn, EnemyFrameId.SkreeReturningOblique),
        [P.SkreePreparingAttack] = PreparingAttack(EnemyFrameId.SkreeFacingViewer, EnemyFrameId.SkreeWingsFlared),
        [P.SkreeDiving] = DiveStart,
        [P.SkreeDivingSpin] = DivingSpin(EnemyFrameId.SkreeTurningOblique, EnemyFrameId.SkreeEdgeOn, EnemyFrameId.SkreeReturningOblique, EnemyFrameId.SkreeFacingViewer),
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at, each segment played whole; a dive's
    /// spin loops on from its entry cue (#2276).
    /// </summary>
    private static Dictionary<SkreeMetareeSegment, Animation<SkreeMetareeSegment>> StartingAt { get; } =
        SpeciesStarts(P.MetareeIdling, P.MetareePreparingAttack, P.MetareeDiving, P.MetareeDivingSpin)
            .Concat(SpeciesStarts(P.SkreeIdling, P.SkreePreparingAttack, P.SkreeDiving, P.SkreeDivingSpin)).ToDictionary();

    /// <summary>
    /// One species' starting animations: its idle loops, its wind-up holds, and its dive starts and then spins for good.
    /// </summary>
    private static KeyValuePair<SkreeMetareeSegment, Animation<SkreeMetareeSegment>>[] SpeciesStarts(SkreeMetareeSegment idling,
        SkreeMetareeSegment preparingAttack, SkreeMetareeSegment diving, SkreeMetareeSegment divingSpin) =>
    [
        new(idling, Animation<SkreeMetareeSegment>.Looping([Segments.Whole(idling)])),
        new(preparingAttack, new([Segments.Whole(preparingAttack)], AnimationEnd.Holds)),
        new(diving, Animation<SkreeMetareeSegment>.LeadingInto([Segments.Whole(diving)], [Segments.Whole(divingSpin)])),
    ];

    /// <summary>A dive's start: either species runs off screen from then on.</summary>
    private static AnimationBeat<EnemyFrameId, SkreeMetareeCue>[] DiveStart => [Cue(new(K.ProcessOffScreen))];

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SkreeMetareeSegment> StartingWith(SkreeMetareeSegment segment) => StartingAt[segment];

    /// <summary>A Metaree's or a Skree's hovering over its species' poses, <paramref name="viewer"/>, <paramref name="turningOblique"/>, <paramref name="edgeOn"/>, <paramref name="returningOblique"/>.</summary>
    private static AnimationBeat<EnemyFrameId, SkreeMetareeCue>[] Idling(EnemyFrameId viewer, EnemyFrameId turningOblique, EnemyFrameId edgeOn, EnemyFrameId returningOblique) =>
    [
        Show(viewer, 10),
        Show(turningOblique, 10),
        Show(edgeOn, 10),
        Show(returningOblique, 10),
    ];

    /// <summary>A Metaree's or a Skree's winding up to dive over its species' poses, <paramref name="viewer"/>, <paramref name="wingsFlared"/>.</summary>
    private static AnimationBeat<EnemyFrameId, SkreeMetareeCue>[] PreparingAttack(EnemyFrameId viewer, EnemyFrameId wingsFlared) =>
    [
        Show(viewer, 16),
        Show(wingsFlared, 8),
        Cue(new(K.SetAttackReady)),
    ];

    /// <summary>A Metaree's or a Skree's spinning as it dives over its species' poses, <paramref name="turningOblique"/>, <paramref name="edgeOn"/>, <paramref name="returningOblique"/>, <paramref name="viewer"/>.</summary>
    private static AnimationBeat<EnemyFrameId, SkreeMetareeCue>[] DivingSpin(EnemyFrameId turningOblique, EnemyFrameId edgeOn, EnemyFrameId returningOblique, EnemyFrameId viewer) =>
    [
        Show(turningOblique, 2),
        Show(edgeOn, 2),
        Show(returningOblique, 2),
        Show(viewer, 2),
    ];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, SkreeMetareeCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, SkreeMetareeCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, SkreeMetareeCue> Cue(SkreeMetareeCue cue) => new AnimationBeat<EnemyFrameId, SkreeMetareeCue>.Event(cue);
}

using P = SuperMetroid.Core.Enemies.Gunship.GunshipSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The gunship hull's and entrance pad's typed animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class GunshipAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<GunshipSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<GunshipSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.EntrancePadOpening] =
        [
            Show(EnemyFrameId.GunshipEntrancePadShut, 40),
            Show(EnemyFrameId.GunshipEntrancePadShutRaised1, 8),
            Show(EnemyFrameId.GunshipEntrancePadShutRaised2, 8),
            Show(EnemyFrameId.GunshipEntrancePadShutRaised4, 8),
            Show(EnemyFrameId.GunshipEntrancePadOpening32Wide, 24),
            Show(EnemyFrameId.GunshipEntrancePadOpening36Wide, 8),
            Show(EnemyFrameId.GunshipEntrancePadOpening40Wide, 7),
            Show(EnemyFrameId.GunshipEntrancePadOpening44Wide, 6),
            Show(EnemyFrameId.GunshipEntrancePadOpening48Wide, 5),
            Show(EnemyFrameId.GunshipEntrancePadOpening52Wide, 4),
            Hold(EnemyFrameId.GunshipEntrancePadOpen),
        ],
        [P.EntrancePadClosing] =
        [
            Show(EnemyFrameId.GunshipEntrancePadOpening52Wide, 4),
            Show(EnemyFrameId.GunshipEntrancePadOpening48Wide, 5),
            Show(EnemyFrameId.GunshipEntrancePadOpening44Wide, 6),
            Show(EnemyFrameId.GunshipEntrancePadOpening40Wide, 7),
            Show(EnemyFrameId.GunshipEntrancePadOpening36Wide, 8),
            Show(EnemyFrameId.GunshipEntrancePadOpening32Wide, 24),
            Show(EnemyFrameId.GunshipEntrancePadShutRaised4, 8),
            Show(EnemyFrameId.GunshipEntrancePadShutRaised1, 8),
        ],
        [P.BottomEntrancePad] =
        [
            Hold(EnemyFrameId.GunshipEntrancePadShut),
        ],
        [P.TopHull] =
        [
            Hold(EnemyFrameId.GunshipTopHull),
        ],
        [P.BottomHull] =
        [
            Hold(EnemyFrameId.GunshipBottomHull),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at (#2337): the shut pad and both hulls are
    /// still pictures, and the closing pad ends resting shut, held there (#2353).
    /// </summary>
    internal static IReadOnlyDictionary<GunshipSegment, Animation<GunshipSegment>> StartingAt { get; } = new Dictionary<GunshipSegment, Animation<GunshipSegment>>
    {
        [P.EntrancePadOpening] = new([Whole(P.EntrancePadOpening)], AnimationEnd.Holds),
        [P.EntrancePadClosing] = new([Whole(P.EntrancePadClosing), Whole(P.BottomEntrancePad)], AnimationEnd.Holds),
        [P.BottomEntrancePad] = new([Whole(P.BottomEntrancePad)], AnimationEnd.Holds),
        [P.TopHull] = new([Whole(P.TopHull)], AnimationEnd.Holds),
        [P.BottomHull] = new([Whole(P.BottomHull)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<GunshipSegment> StartingWith(GunshipSegment segment) => StartingAt[segment];

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<GunshipSegment> Whole(GunshipSegment segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good (#2144, #2337): the opened pad, the shut pad and the hulls rest on their picture until their owner plays another.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.HeldFrame(frame);
}

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
            Show(EnemyFrameId.GunshipEntrancePadLifting, 8),
            Show(EnemyFrameId.GunshipEntrancePadRising, 8),
            Show(EnemyFrameId.GunshipEntrancePadRaised, 8),
            .. PadParting,
            Hold(EnemyFrameId.GunshipEntrancePadOpen),
        ],
        [P.EntrancePadClosing] =
        [
            .. PadParting.Reverse(),
            Show(EnemyFrameId.GunshipEntrancePadRaised, 8),
            Show(EnemyFrameId.GunshipEntrancePadLifting, 8),
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
    private static Dictionary<GunshipSegment, Animation<GunshipSegment>> StartingAt { get; } = new Dictionary<GunshipSegment, Animation<GunshipSegment>>
    {
        [P.EntrancePadOpening] = new([Segments.Whole(P.EntrancePadOpening)], AnimationEnd.Holds),
        [P.EntrancePadClosing] = new([Segments.Whole(P.EntrancePadClosing), Segments.Whole(P.BottomEntrancePad)], AnimationEnd.Holds),
        [P.BottomEntrancePad] = new([Segments.Whole(P.BottomEntrancePad)], AnimationEnd.Holds),
        [P.TopHull] = new([Segments.Whole(P.TopHull)], AnimationEnd.Holds),
        [P.BottomHull] = new([Segments.Whole(P.BottomHull)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<GunshipSegment> StartingWith(GunshipSegment segment) => StartingAt[segment];

    /// <summary>
    /// The raised entrance pad parting, from split to nearly open: the pad's opening plays it forward and its closing plays it
    /// back, each picture as long either way.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue>[] PadParting =>
    [
        Show(EnemyFrameId.GunshipEntrancePadSplitting, 24),
        Show(EnemyFrameId.GunshipEntrancePadParting, 8),
        Show(EnemyFrameId.GunshipEntrancePadSeparating, 7),
        Show(EnemyFrameId.GunshipEntrancePadSpreading, 6),
        Show(EnemyFrameId.GunshipEntrancePadWidening, 5),
        Show(EnemyFrameId.GunshipEntrancePadNearlyOpen, 4),
    ];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good (#2144, #2337): the opened pad, the shut pad and the hulls rest on their picture until their owner plays another.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.HeldFrame(frame);
}

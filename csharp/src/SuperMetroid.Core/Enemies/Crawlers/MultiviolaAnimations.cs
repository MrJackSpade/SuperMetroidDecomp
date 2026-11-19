using P = SuperMetroid.Core.Enemies.Crawlers.MultiviolaSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// Multiviola's spinning flight loop as a typed animation (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class MultiviolaAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MultiviolaSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<MultiviolaSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.Flying] =
        [
            Show(EnemyFrameId.MultiviolaPlain, 10),
            Show(EnemyFrameId.MultiviolaFaceFaint, 10),
            Show(EnemyFrameId.MultiviolaFaceForming, 10),
            Show(EnemyFrameId.MultiviolaFaceDim, 10),
            Show(EnemyFrameId.MultiviolaFaceLit, 10),
            Show(EnemyFrameId.MultiviolaFaceBright, 10),
            Show(EnemyFrameId.MultiviolaFaceBrighter, 10),
            Show(EnemyFrameId.MultiviolaFaceBrightest, 10),
            Show(EnemyFrameId.MultiviolaFaceBrighter, 10),
            Show(EnemyFrameId.MultiviolaFaceBright, 10),
            Show(EnemyFrameId.MultiviolaFaceLit, 10),
            Show(EnemyFrameId.MultiviolaFaceDim, 10),
            Show(EnemyFrameId.MultiviolaFaceForming, 10),
            Show(EnemyFrameId.MultiviolaFaceFaint, 10),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    private static IReadOnlyDictionary<MultiviolaSegment, Animation<MultiviolaSegment>> StartingAt { get; } = new Dictionary<MultiviolaSegment, Animation<MultiviolaSegment>>
    {
        [P.Flying] = Animation<MultiviolaSegment>.Looping([Segments.Whole(P.Flying)]),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<MultiviolaSegment> StartingWith(MultiviolaSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}

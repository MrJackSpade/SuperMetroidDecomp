using P = SuperMetroid.Core.Enemies.Polyp.PolypSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>
/// Polyp's stationary pose as a typed animation (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class PolypAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<PolypSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<PolypSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.Stationary] =
        [
            Hold(EnemyFrameId.PolypRock),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    private static Dictionary<PolypSegment, Animation<PolypSegment>> StartingAt { get; } = new Dictionary<PolypSegment, Animation<PolypSegment>>
    {
        [P.Stationary] = new([Segments.Whole(P.Stationary)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<PolypSegment> StartingWith(PolypSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for good (#2276): a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.HeldFrame(frame);
}

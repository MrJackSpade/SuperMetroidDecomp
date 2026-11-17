using P = SuperMetroid.Core.Enemies.Kzan.KzanSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>
/// Kzan's idle pose as a typed animation (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class KzanAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<KzanSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<KzanSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.Idle] =
        [
            Hold(EnemyFrameId.KzanSpikedBlock),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<KzanSegment, Animation<KzanSegment>> StartingAt { get; } = new Dictionary<KzanSegment, Animation<KzanSegment>>
    {
        [P.Idle] = new([Whole(P.Idle)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<KzanSegment> StartingWith(KzanSegment segment) => StartingAt[segment];

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<KzanSegment> Whole(KzanSegment segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>Shows <paramref name="frame"/> for good (#2337): a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.HeldFrame(frame);
}

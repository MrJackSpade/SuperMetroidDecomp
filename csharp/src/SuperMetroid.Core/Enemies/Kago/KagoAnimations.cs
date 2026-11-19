using P = SuperMetroid.Core.Enemies.Kago.KagoSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>
/// Kago's slow and post-hit animation loops (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class KagoAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<KagoSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<KagoSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.Slow] =
        [
            Show(EnemyFrameId.KagoHiveAtRest, 10),
            Show(EnemyFrameId.KagoHiveSwelling, 10),
            Show(EnemyFrameId.KagoHiveSwollen, 10),
            Show(EnemyFrameId.KagoHiveSwelling, 10),
        ],
        [P.Fast] =
        [
            Show(EnemyFrameId.KagoHiveAtRest, 3),
            Show(EnemyFrameId.KagoHiveSwelling, 3),
            Show(EnemyFrameId.KagoHiveSwollen, 3),
            Show(EnemyFrameId.KagoHiveSwelling, 3),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    private static IReadOnlyDictionary<KagoSegment, Animation<KagoSegment>> StartingAt { get; } = new Dictionary<KagoSegment, Animation<KagoSegment>>
    {
        [P.Slow] = Animation<KagoSegment>.Looping([Segments.Whole(P.Slow)]),
        [P.Fast] = Animation<KagoSegment>.Looping([Segments.Whole(P.Fast)]),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<KagoSegment> StartingWith(KagoSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}

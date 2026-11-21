using P = SuperMetroid.Core.Enemies.Kago.KagoSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>
/// Kago's slow and post-hit animation loops (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static partial class KagoAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<KagoSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<KagoSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.Slow] = Pulsing(10),
        [P.Fast] = Pulsing(3),
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static Dictionary<KagoSegment, Animation<KagoSegment>> StartingAt { get; } = new Dictionary<KagoSegment, Animation<KagoSegment>>
    {
        [P.Slow] = Animation<KagoSegment>.Looping([Segments.Whole(P.Slow)]),
        [P.Fast] = Animation<KagoSegment>.Looping([Segments.Whole(P.Fast)]),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<KagoSegment> StartingWith(KagoSegment segment) => StartingAt[segment];

    /// <summary>The hive's four poses at rest, swelling, swollen and swelling again, each shown for <paramref name="updates"/>.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue>[] Pulsing(int updates) =>
    [
        Show(EnemyFrameId.KagoHiveAtRest, updates),
        Show(EnemyFrameId.KagoHiveSwelling, updates),
        Show(EnemyFrameId.KagoHiveSwollen, updates),
        Show(EnemyFrameId.KagoHiveSwelling, updates),
    ];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}

using P = SuperMetroid.Core.Enemies.Kago.KagoProgram;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>
/// Kago's slow and post-hit animation loops as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and its lists use no owner callbacks.
/// </summary>
internal static class KagoAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<KagoProgram, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<KagoProgram, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.Slow] =
        [
            Show(EnemyFrameId.KagoSlowFirst, 10),
            Show(EnemyFrameId.KagoSlowSecond, 10),
            Show(EnemyFrameId.KagoSlowThird, 10),
            Show(EnemyFrameId.KagoSlowSecond, 10),
        ],
        [P.Fast] =
        [
            Show(EnemyFrameId.KagoSlowFirst, 3),
            Show(EnemyFrameId.KagoSlowSecond, 3),
            Show(EnemyFrameId.KagoSlowThird, 3),
            Show(EnemyFrameId.KagoSlowSecond, 3),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<KagoProgram, Animation<KagoProgram>> StartingAt { get; } = new Dictionary<KagoProgram, Animation<KagoProgram>>
    {
        [P.Slow] = new([new(P.Slow, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Fast] = new([new(P.Fast, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<KagoProgram> StartingWith(KagoProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}

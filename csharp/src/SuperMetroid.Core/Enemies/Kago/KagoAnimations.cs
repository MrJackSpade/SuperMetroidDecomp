using P = SuperMetroid.Core.Enemies.Kago.KagoProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>
/// Kago's slow and post-hit animation loops as typed programs (#641). Each
/// frame shows a <see cref="KagoFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class KagoAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<KagoProgram, KagoFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<KagoProgram, AnimationBeat<KagoFrame, NoAnimationCue>[]>
    {
        [P.Slow] =
        [
            Show(0, 10),
            Show(1, 10),
            Show(2, 10),
            Show(1, 10),
        ],
        [P.Fast] =
        [
            Show(0, 3),
            Show(1, 3),
            Show(2, 3),
            Show(1, 3),
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

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<KagoFrame, NoAnimationCue> Show(int pose, int duration) => new AnimationBeat<KagoFrame, NoAnimationCue>.Frame(new(pose), duration);
}

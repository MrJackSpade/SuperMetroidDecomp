using P = SuperMetroid.Core.Enemies.Crawlers.MultiviolaProgram;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// Multiviola's spinning flight loop as a typed program (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and its lists use no owner callbacks.
/// </summary>
internal static class MultiviolaAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MultiviolaProgram, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<MultiviolaProgram, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.Flying] =
        [
            Show(EnemyFrameId.MultiviolaFlyingFirst, 10),
            Show(EnemyFrameId.MultiviolaFlyingSecond, 10),
            Show(EnemyFrameId.MultiviolaFlyingThird, 10),
            Show(EnemyFrameId.MultiviolaFlyingFourth, 10),
            Show(EnemyFrameId.MultiviolaFlyingFifth, 10),
            Show(EnemyFrameId.MultiviolaFlyingSixth, 10),
            Show(EnemyFrameId.MultiviolaFlyingSeventh, 10),
            Show(EnemyFrameId.MultiviolaFlyingEighth, 10),
            Show(EnemyFrameId.MultiviolaFlyingSeventh, 10),
            Show(EnemyFrameId.MultiviolaFlyingSixth, 10),
            Show(EnemyFrameId.MultiviolaFlyingFifth, 10),
            Show(EnemyFrameId.MultiviolaFlyingFourth, 10),
            Show(EnemyFrameId.MultiviolaFlyingThird, 10),
            Show(EnemyFrameId.MultiviolaFlyingSecond, 10),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<MultiviolaProgram, Animation<MultiviolaProgram>> StartingAt { get; } = new Dictionary<MultiviolaProgram, Animation<MultiviolaProgram>>
    {
        [P.Flying] = new([new(P.Flying, 0, 14, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<MultiviolaProgram> StartingWith(MultiviolaProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}

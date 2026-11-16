using P = SuperMetroid.Core.Enemies.Shutters.ShutterSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The growing and vertical shutters' poses and Kamer's platform loop as typed animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class ShutterAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<ShutterSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<ShutterSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.GrowingTenPixels] =
        [
            Show(EnemyFrameId.ShutterTenPixelsLong, 1),
        ],
        [P.GrowingTwentyPixels] =
        [
            Show(EnemyFrameId.ShutterTwentyPixelsLong, 1),
        ],
        [P.GrowingThirtyPixels] =
        [
            Show(EnemyFrameId.ShutterThirtyPixelsLong, 1),
        ],
        [P.FortyPixels] =
        [
            Show(EnemyFrameId.ShutterFortyPixelsLong, 1),
        ],
        [P.KamerPlatform] =
        [
            Show(EnemyFrameId.KamerLightDark, 10),
            Show(EnemyFrameId.KamerLightDim, 10),
            Show(EnemyFrameId.KamerLightLit, 10),
            Show(EnemyFrameId.KamerLightBright, 10),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<ShutterSegment, Animation<ShutterSegment>> StartingAt { get; } = new Dictionary<ShutterSegment, Animation<ShutterSegment>>
    {
        [P.GrowingTenPixels] = new([new(P.GrowingTenPixels, 0, 1, 1)], AnimationEnd.Holds),
        [P.GrowingTwentyPixels] = new([new(P.GrowingTwentyPixels, 0, 1, 1)], AnimationEnd.Holds),
        [P.GrowingThirtyPixels] = new([new(P.GrowingThirtyPixels, 0, 1, 1)], AnimationEnd.Holds),
        [P.FortyPixels] = new([new(P.FortyPixels, 0, 1, 1)], AnimationEnd.Holds),
        [P.KamerPlatform] = new([new(P.KamerPlatform, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<ShutterSegment> StartingWith(ShutterSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}

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
            Hold(EnemyFrameId.ShutterTenPixelsLong),
        ],
        [P.GrowingTwentyPixels] =
        [
            Hold(EnemyFrameId.ShutterTwentyPixelsLong),
        ],
        [P.GrowingThirtyPixels] =
        [
            Hold(EnemyFrameId.ShutterThirtyPixelsLong),
        ],
        [P.FortyPixels] =
        [
            Hold(EnemyFrameId.ShutterFortyPixelsLong),
        ],
        [P.KamerPlatform] =
        [
            Show(EnemyFrameId.PlatformLightDark, 10),
            Show(EnemyFrameId.PlatformLightDim, 10),
            Show(EnemyFrameId.PlatformLightLit, 10),
            Show(EnemyFrameId.PlatformLightBright, 10),
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

    /// <summary>Shows <paramref name="frame"/> for good (#2276): a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.HeldFrame(frame);
}

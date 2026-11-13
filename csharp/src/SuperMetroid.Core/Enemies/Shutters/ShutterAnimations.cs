using P = SuperMetroid.Core.Enemies.Shutters.ShutterProgram;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The growing, vertical and horizontal shutters' poses and Kamer's platform loop as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and its lists use no owner callbacks.
/// </summary>
internal static class ShutterAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<ShutterProgram, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<ShutterProgram, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.GrowingTenPixels] =
        [
            Show(EnemyFrameId.ShutterGrowing10px, 1),
        ],
        [P.GrowingTwentyPixels] =
        [
            Show(EnemyFrameId.ShutterGrowing20px, 1),
        ],
        [P.GrowingThirtyPixels] =
        [
            Show(EnemyFrameId.ShutterGrowing30px, 1),
        ],
        [P.FortyPixels] =
        [
            Show(EnemyFrameId.ShutterVertical40px, 1),
        ],
        [P.Horizontal] =
        [
            Show(EnemyFrameId.ShutterHorizontal, 1),
        ],
        [P.KamerPlatform] =
        [
            Show(EnemyFrameId.KamerPlatform0, 10),
            Show(EnemyFrameId.KamerPlatform1, 10),
            Show(EnemyFrameId.KamerPlatform2, 10),
            Show(EnemyFrameId.KamerPlatform3, 10),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<ShutterProgram, Animation<ShutterProgram>> StartingAt { get; } = new Dictionary<ShutterProgram, Animation<ShutterProgram>>
    {
        [P.GrowingTenPixels] = new([new(P.GrowingTenPixels, 0, 1, 1)], AnimationEnd.Holds),
        [P.GrowingTwentyPixels] = new([new(P.GrowingTwentyPixels, 0, 1, 1)], AnimationEnd.Holds),
        [P.GrowingThirtyPixels] = new([new(P.GrowingThirtyPixels, 0, 1, 1)], AnimationEnd.Holds),
        [P.FortyPixels] = new([new(P.FortyPixels, 0, 1, 1)], AnimationEnd.Holds),
        [P.Horizontal] = new([new(P.Horizontal, 0, 1, 1)], AnimationEnd.Holds),
        [P.KamerPlatform] = new([new(P.KamerPlatform, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<ShutterProgram> StartingWith(ShutterProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}

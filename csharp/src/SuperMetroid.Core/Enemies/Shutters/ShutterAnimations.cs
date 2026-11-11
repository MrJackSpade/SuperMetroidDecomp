using P = SuperMetroid.Core.Enemies.Shutters.ShutterProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The growing, vertical and horizontal shutters' poses and Kamer's platform loop as typed programs (#641). Each
/// frame shows a <see cref="ShutterFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class ShutterAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<ShutterProgram, ShutterFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<ShutterProgram, AnimationBeat<ShutterFrame, NoAnimationCue>[]>
    {
        [P.GrowingTenPixels] =
        [
            Show(0, 1),
        ],
        [P.GrowingTwentyPixels] =
        [
            Show(1, 1),
        ],
        [P.GrowingThirtyPixels] =
        [
            Show(2, 1),
        ],
        [P.FortyPixels] =
        [
            Show(3, 1),
        ],
        [P.Horizontal] =
        [
            Show(4, 1),
        ],
        [P.KamerPlatform] =
        [
            Show(5, 10),
            Show(6, 10),
            Show(7, 10),
            Show(8, 10),
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

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<ShutterFrame, NoAnimationCue> Show(int pose, int duration) => new AnimationBeat<ShutterFrame, NoAnimationCue>.Frame(new(pose), duration);
}

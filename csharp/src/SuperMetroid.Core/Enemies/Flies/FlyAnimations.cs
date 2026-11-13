using P = SuperMetroid.Core.Enemies.Flies.FlyProgram;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Flies;

/// <summary>
/// The shared Mellow, Mella and Memu flight loop as a typed program (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class FlyAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<FlyProgram, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<FlyProgram, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.Flight] =
        [
            Show(EnemyFrameId.FlyFlightFirst, 2),
            Show(EnemyFrameId.FlyFlightSecond, 2),
            Show(EnemyFrameId.FlyFlightThird, 2),
            Show(EnemyFrameId.FlyFlightFourth, 2),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<FlyProgram, Animation<FlyProgram>> StartingAt { get; } = new Dictionary<FlyProgram, Animation<FlyProgram>>
    {
        [P.Flight] = new([new(P.Flight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<FlyProgram> StartingWith(FlyProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}

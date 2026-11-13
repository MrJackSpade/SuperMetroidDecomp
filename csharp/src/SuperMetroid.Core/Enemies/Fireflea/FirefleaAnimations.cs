using P = SuperMetroid.Core.Enemies.Fireflea.FirefleaSegment;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>
/// Fireflea's 52-frame glow loop as a typed animation (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class FirefleaAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<FirefleaSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<FirefleaSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.Loop] =
        [
            Show(EnemyFrameId.FirefleaLoopFirst, 2),
            Show(EnemyFrameId.FirefleaLoopSecond, 1),
            Show(EnemyFrameId.FirefleaLoopThird, 2),
            Show(EnemyFrameId.FirefleaLoopSecond, 1),
            Show(EnemyFrameId.FirefleaLoopFirst, 2),
            Show(EnemyFrameId.FirefleaLoopSecond, 1),
            Show(EnemyFrameId.FirefleaLoopThird, 2),
            Show(EnemyFrameId.FirefleaLoopSecond, 1),
            Show(EnemyFrameId.FirefleaLoopFourth, 2),
            Show(EnemyFrameId.FirefleaLoopFifth, 1),
            Show(EnemyFrameId.FirefleaLoopSixth, 2),
            Show(EnemyFrameId.FirefleaLoopFifth, 1),
            Show(EnemyFrameId.FirefleaLoopSeventh, 2),
            Show(EnemyFrameId.FirefleaLoopEighth, 1),
            Show(EnemyFrameId.FirefleaLoopNinth, 2),
            Show(EnemyFrameId.FirefleaLoopEighth, 1),
            Show(EnemyFrameId.FirefleaLoopTenth, 2),
            Show(EnemyFrameId.FirefleaLoopEleventh, 1),
            Show(EnemyFrameId.FirefleaLoopTwelfth, 2),
            Show(EnemyFrameId.FirefleaLoopEleventh, 1),
            Show(EnemyFrameId.FirefleaLoopThirteenth, 2),
            Show(EnemyFrameId.FirefleaLoopFourteenth, 1),
            Show(EnemyFrameId.FirefleaLoopFifteenth, 2),
            Show(EnemyFrameId.FirefleaLoopFourteenth, 1),
            Show(EnemyFrameId.FirefleaLoopSixteenth, 2),
            Show(EnemyFrameId.FirefleaLoopSeventeenth, 1),
            Show(EnemyFrameId.FirefleaLoopEighteenth, 2),
            Show(EnemyFrameId.FirefleaLoopSeventeenth, 1),
            Show(EnemyFrameId.FirefleaLoopNineteenth, 2),
            Show(EnemyFrameId.FirefleaLoopTwentieth, 1),
            Show(EnemyFrameId.FirefleaLoopTwentyFirst, 2),
            Show(EnemyFrameId.FirefleaLoopTwentieth, 1),
            Show(EnemyFrameId.FirefleaLoopSixteenth, 2),
            Show(EnemyFrameId.FirefleaLoopSeventeenth, 1),
            Show(EnemyFrameId.FirefleaLoopEighteenth, 2),
            Show(EnemyFrameId.FirefleaLoopSeventeenth, 1),
            Show(EnemyFrameId.FirefleaLoopThirteenth, 2),
            Show(EnemyFrameId.FirefleaLoopFourteenth, 1),
            Show(EnemyFrameId.FirefleaLoopFifteenth, 2),
            Show(EnemyFrameId.FirefleaLoopFourteenth, 1),
            Show(EnemyFrameId.FirefleaLoopTenth, 2),
            Show(EnemyFrameId.FirefleaLoopEleventh, 1),
            Show(EnemyFrameId.FirefleaLoopTwelfth, 2),
            Show(EnemyFrameId.FirefleaLoopEleventh, 1),
            Show(EnemyFrameId.FirefleaLoopSeventh, 2),
            Show(EnemyFrameId.FirefleaLoopEighth, 1),
            Show(EnemyFrameId.FirefleaLoopNinth, 2),
            Show(EnemyFrameId.FirefleaLoopEighth, 1),
            Show(EnemyFrameId.FirefleaLoopFourth, 2),
            Show(EnemyFrameId.FirefleaLoopFifth, 1),
            Show(EnemyFrameId.FirefleaLoopSixth, 2),
            Show(EnemyFrameId.FirefleaLoopFifth, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<FirefleaSegment, Animation<FirefleaSegment>> StartingAt { get; } = new Dictionary<FirefleaSegment, Animation<FirefleaSegment>>
    {
        [P.Loop] = new([new(P.Loop, 0, 52, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<FirefleaSegment> StartingWith(FirefleaSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}

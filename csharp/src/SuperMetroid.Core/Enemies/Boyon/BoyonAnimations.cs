using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Boyon.BoyonCueKind;
using P = SuperMetroid.Core.Enemies.Boyon.BoyonSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>
/// Boyon's idle and bouncing loops (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="BoyonCue"/> naming what it does.
/// </summary>
internal static class BoyonAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<BoyonSegment, EnemyFrameId, BoyonCue> Segments { get; } = new(
        new Dictionary<BoyonSegment, AnimationBeat<EnemyFrameId, BoyonCue>[]>
    {
        [P.Idle] =
        [
            Cue(new(K.StopProcessingOffScreen)),
        ],
        [P.IdleRest] =
        [
            Show(EnemyFrameId.BoyonRestingRound, 10),
            Show(EnemyFrameId.BoyonRestingGlinting, 10),
            Show(EnemyFrameId.BoyonRestingSquat, 10),
            Show(EnemyFrameId.BoyonRestingGlinting, 10),
        ],
        [P.Bouncing] =
        [
            Cue(new(K.ProcessOffScreen)),
            Cue(new(K.StartBounce)),
        ],
        [P.BouncingStar] =
        [
            Show(EnemyFrameId.BoyonBouncingBulging, 5),
            Show(EnemyFrameId.BoyonBouncingStarShaped, 5),
            Show(EnemyFrameId.BoyonBouncingStarSpread, 5),
            Show(EnemyFrameId.BoyonBouncingStarSpiked, 5),
            Show(EnemyFrameId.BoyonBouncingStarSpread, 5),
            Show(EnemyFrameId.BoyonBouncingStarShaped, 5),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at (#2275): each raises its own entry cues
    /// once, then rests or bounces for good.
    /// </summary>
    internal static IReadOnlyDictionary<BoyonSegment, Animation<BoyonSegment>> StartingAt { get; } = new Dictionary<BoyonSegment, Animation<BoyonSegment>>
    {
        [P.Idle] = new([Segments.Whole(P.Idle), Segments.Whole(P.IdleRest)], AnimationEnd.LoopsFrom(1)),
        [P.Bouncing] = new([Segments.Whole(P.Bouncing), Segments.Whole(P.BouncingStar)], AnimationEnd.LoopsFrom(1)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<BoyonSegment> StartingWith(BoyonSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, BoyonCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, BoyonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, BoyonCue> Cue(BoyonCue cue) => new AnimationBeat<EnemyFrameId, BoyonCue>.Event(cue);
}

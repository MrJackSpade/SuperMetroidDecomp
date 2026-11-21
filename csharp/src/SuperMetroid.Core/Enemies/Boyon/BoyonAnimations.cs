using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Boyon.BoyonCueKind;
using P = SuperMetroid.Core.Enemies.Boyon.BoyonSegment;
using SuperMetroid.Core.Enemies.Artwork;
using Beats = SuperMetroid.Core.Animation.AnimationBeats<SuperMetroid.Core.Enemies.Artwork.EnemyFrameId, SuperMetroid.Core.Enemies.Boyon.BoyonCue>;

namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>
/// Boyon's idle and bouncing loops (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="BoyonCue"/> naming what it does.
/// </summary>
internal static class BoyonAnimations
{
    /// <summary>The updates each of Boyon's resting poses shows for.</summary>
    private const int RestPoseUpdates = 10;

    /// <summary>The updates each of Boyon's bouncing poses shows for.</summary>
    private const int BouncePoseUpdates = 5;

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
            .. Beats.Steady(RestPoseUpdates,
                EnemyFrameId.BoyonRestingRound, EnemyFrameId.BoyonRestingGlinting, EnemyFrameId.BoyonRestingSquat,
                EnemyFrameId.BoyonRestingGlinting),
        ],
        [P.Bouncing] =
        [
            Cue(new(K.ProcessOffScreen)),
            Cue(new(K.PlayBounceSound)),
        ],
        [P.BouncingStar] =
        [
            .. Beats.Steady(BouncePoseUpdates,
                EnemyFrameId.BoyonBouncingBulging, EnemyFrameId.BoyonBouncingStarShaped,
                EnemyFrameId.BoyonBouncingStarSpread, EnemyFrameId.BoyonBouncingStarSpiked,
                EnemyFrameId.BoyonBouncingStarSpread, EnemyFrameId.BoyonBouncingStarShaped),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at (#2275): each raises its own entry cues
    /// once, then rests or bounces for good.
    /// </summary>
    internal static Dictionary<BoyonSegment, Animation<BoyonSegment>> StartingAt { get; } = new Dictionary<BoyonSegment, Animation<BoyonSegment>>
    {
        [P.Idle] = Animation<BoyonSegment>.LeadingInto([Segments.Whole(P.Idle)], [Segments.Whole(P.IdleRest)]),
        [P.Bouncing] = Animation<BoyonSegment>.LeadingInto([Segments.Whole(P.Bouncing)], [Segments.Whole(P.BouncingStar)]),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<BoyonSegment> StartingWith(BoyonSegment segment) => StartingAt[segment];

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, BoyonCue> Cue(BoyonCue cue) => new AnimationBeat<EnemyFrameId, BoyonCue>.Event(cue);
}

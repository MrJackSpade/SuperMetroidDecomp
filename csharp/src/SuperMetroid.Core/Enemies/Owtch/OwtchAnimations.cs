using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Enemies.Owtch.OwtchSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>
/// Owtch's left and right shell-cycling animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="OwtchCue"/> naming what it does.
/// </summary>
internal static class OwtchAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<OwtchSegment, EnemyFrameId, OwtchCue> Segments { get; } = new(
        new Dictionary<OwtchSegment, AnimationBeat<EnemyFrameId, OwtchCue>[]>
    {
        [P.MovingLeft] =
        [
            Cue(new(HorizontalDirection.Left)),
        ],
        [P.CrawlingLeft] =
        [
            Show(EnemyFrameId.OwtchCrawlingFeetAtRest, 8),
            Show(EnemyFrameId.OwtchCrawlingFeetShuffling, 8),
            Show(EnemyFrameId.OwtchCrawlingFeetStretched, 8),
        ],
        [P.MovingRight] =
        [
            Cue(new(HorizontalDirection.Right)),
        ],
        [P.CrawlingRight] =
        [
            Show(EnemyFrameId.OwtchCrawlingFeetStretched, 8),
            Show(EnemyFrameId.OwtchCrawlingFeetShuffling, 8),
            Show(EnemyFrameId.OwtchCrawlingFeetAtRest, 8),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at: its entry cue once, then its crawl
    /// looping (#2061, #2276).
    /// </summary>
    internal static IReadOnlyDictionary<OwtchSegment, Animation<OwtchSegment>> StartingAt { get; } = new Dictionary<OwtchSegment, Animation<OwtchSegment>>
    {
        [P.MovingLeft] = new([Segments.Whole(P.MovingLeft), Segments.Whole(P.CrawlingLeft)], AnimationEnd.LoopsFrom(1)),
        [P.MovingRight] = new([Segments.Whole(P.MovingRight), Segments.Whole(P.CrawlingRight)], AnimationEnd.LoopsFrom(1)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<OwtchSegment> StartingWith(OwtchSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, OwtchCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, OwtchCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, OwtchCue> Cue(OwtchCue cue) => new AnimationBeat<EnemyFrameId, OwtchCue>.Event(cue);
}

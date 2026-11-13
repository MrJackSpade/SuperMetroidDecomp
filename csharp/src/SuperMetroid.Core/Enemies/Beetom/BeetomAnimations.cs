using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Beetom.BeetomCueKind;
using P = SuperMetroid.Core.Enemies.Beetom.BeetomSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>
/// The Beetom's crawl, hop and drain animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="BeetomCue"/> with its typed operands.
/// </summary>
internal static class BeetomAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<BeetomSegment, EnemyFrameId, BeetomCue> Segments { get; } = new(
        new Dictionary<BeetomSegment, AnimationBeat<EnemyFrameId, BeetomCue>[]>
    {
        [P.CrawlingLeft] =
        [
            Cue(new(K.StopProcessingOffScreen)),
        ],
        [P.CrawlingLeftLoop] =
        [
            Show(EnemyFrameId.BeetomCrawlingLeftLoopFirst, 10),
            Show(EnemyFrameId.BeetomCrawlingLeftLoopSecond, 10),
            Show(EnemyFrameId.BeetomCrawlingLeftLoopThird, 10),
            Show(EnemyFrameId.BeetomCrawlingLeftLoopSecond, 10),
        ],
        [P.HopLeft] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(EnemyFrameId.BeetomHopLeftFirst, 4),
            Show(EnemyFrameId.BeetomHopLeftSecond, 8),
            Show(EnemyFrameId.BeetomHopLeftFirst, 4),
            Show(EnemyFrameId.BeetomCrawlingLeftLoopFirst, 1),
        ],
        [P.DrainingLeft] =
        [
            Show(EnemyFrameId.BeetomDrainingLeftFirst, 5),
            Show(EnemyFrameId.BeetomDrainingLeftSecond, 5),
            Show(EnemyFrameId.BeetomDrainingLeftThird, 5),
            Show(EnemyFrameId.BeetomDrainingLeftSecond, 48),
            Cue(new(K.Nothing)),
        ],
        [P.DrainingLeftLoop] =
        [
            Show(EnemyFrameId.BeetomDrainingLeftLoopFirst, 5),
            Show(EnemyFrameId.BeetomDrainingLeftLoopSecond, 5),
            Show(EnemyFrameId.BeetomDrainingLeftLoopThird, 5),
            Show(EnemyFrameId.BeetomDrainingLeftLoopSecond, 5),
        ],
        [P.CrawlingRight] =
        [
            Cue(new(K.StopProcessingOffScreen)),
        ],
        [P.CrawlingRightLoop] =
        [
            Show(EnemyFrameId.BeetomCrawlingRightLoopFirst, 10),
            Show(EnemyFrameId.BeetomCrawlingRightLoopSecond, 10),
            Show(EnemyFrameId.BeetomCrawlingRightLoopThird, 10),
            Show(EnemyFrameId.BeetomCrawlingRightLoopSecond, 10),
        ],
        [P.HopRight] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(EnemyFrameId.BeetomHopRightFirst, 4),
            Show(EnemyFrameId.BeetomHopRightSecond, 8),
            Show(EnemyFrameId.BeetomHopRightFirst, 4),
            Show(EnemyFrameId.BeetomCrawlingRightLoopFirst, 1),
        ],
        [P.DrainingRight] =
        [
            Show(EnemyFrameId.BeetomDrainingRightFirst, 5),
            Show(EnemyFrameId.BeetomDrainingRightSecond, 5),
            Show(EnemyFrameId.BeetomDrainingRightThird, 5),
            Show(EnemyFrameId.BeetomDrainingRightSecond, 48),
            Cue(new(K.Nothing)),
        ],
        [P.DrainingRightLoop] =
        [
            Show(EnemyFrameId.BeetomDrainingRightLoopFirst, 5),
            Show(EnemyFrameId.BeetomDrainingRightLoopSecond, 5),
            Show(EnemyFrameId.BeetomDrainingRightLoopThird, 5),
            Show(EnemyFrameId.BeetomDrainingRightLoopSecond, 5),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<BeetomSegment, Animation<BeetomSegment>> StartingAt { get; } = new Dictionary<BeetomSegment, Animation<BeetomSegment>>
    {
        [P.CrawlingLeft] = new([new(P.CrawlingLeft, 0, 1, 1), new(P.CrawlingLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.CrawlingLeftLoop] = new([new(P.CrawlingLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HopLeft] = new([new(P.HopLeft, 0, 5, 1)], AnimationEnd.Holds),
        [P.DrainingLeft] = new([new(P.DrainingLeft, 0, 5, 1), new(P.DrainingLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.DrainingLeftLoop] = new([new(P.DrainingLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.CrawlingRight] = new([new(P.CrawlingRight, 0, 1, 1), new(P.CrawlingRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.CrawlingRightLoop] = new([new(P.CrawlingRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HopRight] = new([new(P.HopRight, 0, 5, 1)], AnimationEnd.Holds),
        [P.DrainingRight] = new([new(P.DrainingRight, 0, 5, 1), new(P.DrainingRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.DrainingRightLoop] = new([new(P.DrainingRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<BeetomSegment> StartingWith(BeetomSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, BeetomCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, BeetomCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, BeetomCue> Cue(BeetomCue cue) => new AnimationBeat<EnemyFrameId, BeetomCue>.Event(cue);
}

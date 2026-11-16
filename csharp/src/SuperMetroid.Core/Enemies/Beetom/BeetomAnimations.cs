using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Beetom.BeetomCueKind;
using P = SuperMetroid.Core.Enemies.Beetom.BeetomSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>
/// The Beetom's crawl, hop and drain animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="BeetomCue"/> naming what it does.
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
            Show(EnemyFrameId.BeetomCrawlingLeftLegsTucked, 10),
            Show(EnemyFrameId.BeetomCrawlingLeftLegsMid, 10),
            Show(EnemyFrameId.BeetomCrawlingLeftLegsOut, 10),
            Show(EnemyFrameId.BeetomCrawlingLeftLegsMid, 10),
        ],
        [P.HopLeft] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(EnemyFrameId.BeetomHoppingLeftLegPushing, 4),
            Show(EnemyFrameId.BeetomHoppingLeftLegStretched, 8),
            Show(EnemyFrameId.BeetomHoppingLeftLegPushing, 4),
            Show(EnemyFrameId.BeetomCrawlingLeftLegsTucked, 1),
        ],
        [P.DrainingLeft] =
        [
            Show(EnemyFrameId.BeetomLatchedFacingLeftGemGreenBright, 5),
            Show(EnemyFrameId.BeetomLatchedFacingLeftGemGreen, 5),
            Show(EnemyFrameId.BeetomLatchedFacingLeftGemGreenDim, 5),
            Show(EnemyFrameId.BeetomLatchedFacingLeftGemGreen, 48),
            Show(EnemyFrameId.BeetomLatchedFacingLeftGemRedBright, 5),
            Show(EnemyFrameId.BeetomLatchedFacingLeftGemRed, 5),
            Show(EnemyFrameId.BeetomLatchedFacingLeftGemRedDim, 5),
            Show(EnemyFrameId.BeetomLatchedFacingLeftGemRed, 5),
        ],
        [P.CrawlingRight] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Show(EnemyFrameId.BeetomCrawlingRightLegsTucked, 10),
            Show(EnemyFrameId.BeetomCrawlingRightLegsMid, 10),
            Show(EnemyFrameId.BeetomCrawlingRightLegsOut, 10),
            Show(EnemyFrameId.BeetomCrawlingRightLegsMid, 10),
        ],
        [P.HopRight] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(EnemyFrameId.BeetomHoppingRightLegPushing, 4),
            Show(EnemyFrameId.BeetomHoppingRightLegStretched, 8),
            Show(EnemyFrameId.BeetomHoppingRightLegPushing, 4),
            Show(EnemyFrameId.BeetomCrawlingRightLegsTucked, 1),
        ],
        [P.DrainingRight] =
        [
            Show(EnemyFrameId.BeetomLatchedFacingRightGemGreenBright, 5),
            Show(EnemyFrameId.BeetomLatchedFacingRightGemGreen, 5),
            Show(EnemyFrameId.BeetomLatchedFacingRightGemGreenDim, 5),
            Show(EnemyFrameId.BeetomLatchedFacingRightGemGreen, 48),
            Show(EnemyFrameId.BeetomLatchedFacingRightGemRedBright, 5),
            Show(EnemyFrameId.BeetomLatchedFacingRightGemRed, 5),
            Show(EnemyFrameId.BeetomLatchedFacingRightGemRedDim, 5),
            Show(EnemyFrameId.BeetomLatchedFacingRightGemRed, 5),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<BeetomSegment, Animation<BeetomSegment>> StartingAt { get; } = new Dictionary<BeetomSegment, Animation<BeetomSegment>>
    {
        [P.CrawlingLeft] = new([new(P.CrawlingLeft, 0, 1, 1), new(P.CrawlingLeft, 1, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.HopLeft] = new([new(P.HopLeft, 0, 5, 1)], AnimationEnd.Holds),
        [P.DrainingLeft] = new([new(P.DrainingLeft, 0, 4, 1), new(P.DrainingLeft, 4, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.CrawlingRight] = new([new(P.CrawlingRight, 0, 1, 1), new(P.CrawlingRight, 1, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.HopRight] = new([new(P.HopRight, 0, 5, 1)], AnimationEnd.Holds),
        [P.DrainingRight] = new([new(P.DrainingRight, 0, 4, 1), new(P.DrainingRight, 4, 4, 1)], AnimationEnd.LoopsFrom(1)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<BeetomSegment> StartingWith(BeetomSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, BeetomCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, BeetomCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, BeetomCue> Cue(BeetomCue cue) => new AnimationBeat<EnemyFrameId, BeetomCue>.Event(cue);
}

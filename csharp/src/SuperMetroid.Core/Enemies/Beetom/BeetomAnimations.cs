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
        ],
        [P.CrawlingLeftStride] =
        [
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
            Hold(EnemyFrameId.BeetomCrawlingLeftLegsTucked),
        ],
        [P.DrainingLeft] =
        [
            Show(EnemyFrameId.BeetomLatchedFacingLeftGemGreenBright, 5),
            Show(EnemyFrameId.BeetomLatchedFacingLeftGemGreen, 5),
            Show(EnemyFrameId.BeetomLatchedFacingLeftGemGreenDim, 5),
            Show(EnemyFrameId.BeetomLatchedFacingLeftGemGreen, 48),
        ],
        [P.DrainingLeftRed] =
        [
            Show(EnemyFrameId.BeetomLatchedFacingLeftGemRedBright, 5),
            Show(EnemyFrameId.BeetomLatchedFacingLeftGemRed, 5),
            Show(EnemyFrameId.BeetomLatchedFacingLeftGemRedDim, 5),
            Show(EnemyFrameId.BeetomLatchedFacingLeftGemRed, 5),
        ],
        [P.CrawlingRight] =
        [
            Cue(new(K.StopProcessingOffScreen)),
        ],
        [P.CrawlingRightStride] =
        [
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
            Hold(EnemyFrameId.BeetomCrawlingRightLegsTucked),
        ],
        [P.DrainingRight] =
        [
            Show(EnemyFrameId.BeetomLatchedFacingRightGemGreenBright, 5),
            Show(EnemyFrameId.BeetomLatchedFacingRightGemGreen, 5),
            Show(EnemyFrameId.BeetomLatchedFacingRightGemGreenDim, 5),
            Show(EnemyFrameId.BeetomLatchedFacingRightGemGreen, 48),
        ],
        [P.DrainingRightRed] =
        [
            Show(EnemyFrameId.BeetomLatchedFacingRightGemRedBright, 5),
            Show(EnemyFrameId.BeetomLatchedFacingRightGemRed, 5),
            Show(EnemyFrameId.BeetomLatchedFacingRightGemRedDim, 5),
            Show(EnemyFrameId.BeetomLatchedFacingRightGemRed, 5),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at (#2275): a crawl stops processing off
    /// screen, its own entry cue, and then strides for good; a hop ends on its tucked legs, held (#2432); a drain shows its
    /// green gem once and then pulses red for good.
    /// </summary>
    internal static IReadOnlyDictionary<BeetomSegment, Animation<BeetomSegment>> StartingAt { get; } = new Dictionary<BeetomSegment, Animation<BeetomSegment>>
    {
        [P.CrawlingLeft] = Animation<BeetomSegment>.LeadingInto([Segments.Whole(P.CrawlingLeft)], [Segments.Whole(P.CrawlingLeftStride)]),
        [P.HopLeft] = new([Segments.Whole(P.HopLeft)], AnimationEnd.Holds),
        [P.DrainingLeft] = Animation<BeetomSegment>.LeadingInto([Segments.Whole(P.DrainingLeft)], [Segments.Whole(P.DrainingLeftRed)]),
        [P.CrawlingRight] = Animation<BeetomSegment>.LeadingInto([Segments.Whole(P.CrawlingRight)], [Segments.Whole(P.CrawlingRightStride)]),
        [P.HopRight] = new([Segments.Whole(P.HopRight)], AnimationEnd.Holds),
        [P.DrainingRight] = Animation<BeetomSegment>.LeadingInto([Segments.Whole(P.DrainingRight)], [Segments.Whole(P.DrainingRightRed)]),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<BeetomSegment> StartingWith(BeetomSegment segment) => StartingAt[segment];

    /// <summary>A still picture (#2275): <paramref name="frame"/>, held from the update it shows.</summary>
    private static AnimationBeat<EnemyFrameId, BeetomCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, BeetomCue>.HeldFrame(frame);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, BeetomCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, BeetomCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, BeetomCue> Cue(BeetomCue cue) => new AnimationBeat<EnemyFrameId, BeetomCue>.Event(cue);
}

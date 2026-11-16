using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Enemies.Crawlers.CrawlerSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// The Sciser, Zero, Viola, orange Zoomer and shared Zoomer/Zeela/Sova surface animations (#641): each family's crawl on
/// each of its four surfaces, and the Viola's one crawl on every surface (#2061). Each frame shows a
/// <see cref="EnemyFrameId"/> and no beat raises a cue: the surface a crawler shows sets its crawl.
/// </summary>
internal static class CrawlerAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<CrawlerSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<CrawlerSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.SciserUpsideRight] =
        [
            Show(EnemyFrameId.SciserUpsideRightLegsTucked, 8),
            Show(EnemyFrameId.SciserUpsideRightLegsMid, 8),
            Show(EnemyFrameId.SciserUpsideRightLegsOut, 8),
            Show(EnemyFrameId.SciserUpsideRightLegsMid, 8),
        ],
        [P.SciserUpsideLeft] =
        [
            Show(EnemyFrameId.SciserUpsideLeftLegsTucked, 8),
            Show(EnemyFrameId.SciserUpsideLeftLegsMid, 8),
            Show(EnemyFrameId.SciserUpsideLeftLegsOut, 8),
            Show(EnemyFrameId.SciserUpsideLeftLegsMid, 8),
        ],
        [P.SciserUpsideDown] =
        [
            Show(EnemyFrameId.SciserUpsideDownLegsTucked, 8),
            Show(EnemyFrameId.SciserUpsideDownLegsMid, 8),
            Show(EnemyFrameId.SciserUpsideDownLegsOut, 8),
            Show(EnemyFrameId.SciserUpsideDownLegsMid, 8),
        ],
        [P.SciserUpsideUp] =
        [
            Show(EnemyFrameId.SciserUpsideUpLegsTucked, 8),
            Show(EnemyFrameId.SciserUpsideUpLegsMid, 8),
            Show(EnemyFrameId.SciserUpsideUpLegsOut, 8),
            Show(EnemyFrameId.SciserUpsideUpLegsMid, 8),
        ],
        [P.ZeroUpsideRight] =
        [
            Show(EnemyFrameId.ZeroUpsideRightContracted, 4),
            Show(EnemyFrameId.ZeroUpsideRightStretching, 4),
            Show(EnemyFrameId.ZeroUpsideRightStretched, 4),
            Show(EnemyFrameId.ZeroUpsideRightFullyStretched, 4),
            Show(EnemyFrameId.ZeroUpsideRightStretched, 4),
            Show(EnemyFrameId.ZeroUpsideRightStretching, 4),
        ],
        [P.ZeroUpsideLeft] =
        [
            Show(EnemyFrameId.ZeroUpsideLeftContracted, 4),
            Show(EnemyFrameId.ZeroUpsideLeftStretching, 4),
            Show(EnemyFrameId.ZeroUpsideLeftStretched, 4),
            Show(EnemyFrameId.ZeroUpsideLeftFullyStretched, 4),
            Show(EnemyFrameId.ZeroUpsideLeftStretched, 4),
            Show(EnemyFrameId.ZeroUpsideLeftStretching, 4),
        ],
        [P.ZeroUpsideDown] =
        [
            Show(EnemyFrameId.ZeroUpsideDownContracted, 4),
            Show(EnemyFrameId.ZeroUpsideDownStretching, 4),
            Show(EnemyFrameId.ZeroUpsideDownStretched, 4),
            Show(EnemyFrameId.ZeroUpsideDownFullyStretched, 4),
            Show(EnemyFrameId.ZeroUpsideDownStretched, 4),
            Show(EnemyFrameId.ZeroUpsideDownStretching, 4),
        ],
        [P.ZeroUpsideUp] =
        [
            Show(EnemyFrameId.ZeroUpsideUpContracted, 4),
            Show(EnemyFrameId.ZeroUpsideUpStretching, 4),
            Show(EnemyFrameId.ZeroUpsideUpStretched, 4),
            Show(EnemyFrameId.ZeroUpsideUpFullyStretched, 4),
            Show(EnemyFrameId.ZeroUpsideUpStretched, 4),
            Show(EnemyFrameId.ZeroUpsideUpStretching, 4),
        ],
        [P.ViolaNormal] =
        [
            Show(EnemyFrameId.ViolaPlain, 10),
            Show(EnemyFrameId.ViolaFaceFaint, 10),
            Show(EnemyFrameId.ViolaFaceForming, 10),
            Show(EnemyFrameId.ViolaFaceDim, 10),
            Show(EnemyFrameId.ViolaFaceLit, 10),
            Show(EnemyFrameId.ViolaFaceBright, 10),
            Show(EnemyFrameId.ViolaFaceBrighter, 10),
            Show(EnemyFrameId.ViolaFaceBrightest, 10),
            Show(EnemyFrameId.ViolaFaceBrighter, 10),
            Show(EnemyFrameId.ViolaFaceBright, 10),
            Show(EnemyFrameId.ViolaFaceLit, 10),
            Show(EnemyFrameId.ViolaFaceDim, 10),
            Show(EnemyFrameId.ViolaFaceForming, 10),
            Show(EnemyFrameId.ViolaFaceFaint, 10),
        ],
        [P.OrangeZoomerUpsideRight] =
        [
            Show(EnemyFrameId.OrangeZoomerUpsideRightSpinesTurned0, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideRightSpinesTurned12, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideRightSpinesTurned24, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideRightSpinesTurned36, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideRightSpinesTurned48, 3),
        ],
        [P.OrangeZoomerUpsideLeft] =
        [
            Show(EnemyFrameId.OrangeZoomerUpsideLeftSpinesTurned0, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideLeftSpinesTurned12, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideLeftSpinesTurned24, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideLeftSpinesTurned36, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideLeftSpinesTurned48, 3),
        ],
        [P.OrangeZoomerUpsideDown] =
        [
            Show(EnemyFrameId.OrangeZoomerUpsideDownSpinesTurned0, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideDownSpinesTurned12, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideDownSpinesTurned24, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideDownSpinesTurned36, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideDownSpinesTurned48, 3),
        ],
        [P.OrangeZoomerUpsideUp] =
        [
            Show(EnemyFrameId.OrangeZoomerUpsideUpSpinesTurned0, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideUpSpinesTurned12, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideUpSpinesTurned24, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideUpSpinesTurned36, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideUpSpinesTurned48, 3),
        ],
        [P.SharedUpsideRight] =
        [
            Show(EnemyFrameId.OrangeZoomerUpsideRightSpinesTurned0, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideRightSpinesTurned12, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideRightSpinesTurned24, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideRightSpinesTurned36, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideRightSpinesTurned48, 3),
        ],
        [P.SharedUpsideLeft] =
        [
            Show(EnemyFrameId.OrangeZoomerUpsideLeftSpinesTurned0, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideLeftSpinesTurned12, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideLeftSpinesTurned24, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideLeftSpinesTurned36, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideLeftSpinesTurned48, 3),
        ],
        [P.SharedUpsideDown] =
        [
            Show(EnemyFrameId.OrangeZoomerUpsideDownSpinesTurned0, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideDownSpinesTurned12, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideDownSpinesTurned24, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideDownSpinesTurned36, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideDownSpinesTurned48, 3),
        ],
        [P.SharedUpsideUp] =
        [
            Show(EnemyFrameId.OrangeZoomerUpsideUpSpinesTurned0, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideUpSpinesTurned12, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideUpSpinesTurned24, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideUpSpinesTurned36, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideUpSpinesTurned48, 3),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<CrawlerSegment, Animation<CrawlerSegment>> StartingAt { get; } = new Dictionary<CrawlerSegment, Animation<CrawlerSegment>>
    {
        [P.SciserUpsideRight] = new([new(P.SciserUpsideRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SciserUpsideLeft] = new([new(P.SciserUpsideLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SciserUpsideDown] = new([new(P.SciserUpsideDown, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SciserUpsideUp] = new([new(P.SciserUpsideUp, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZeroUpsideRight] = new([new(P.ZeroUpsideRight, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZeroUpsideLeft] = new([new(P.ZeroUpsideLeft, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZeroUpsideDown] = new([new(P.ZeroUpsideDown, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZeroUpsideUp] = new([new(P.ZeroUpsideUp, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ViolaNormal] = new([new(P.ViolaNormal, 0, 14, 1)], AnimationEnd.LoopsFrom(0)),
        [P.OrangeZoomerUpsideRight] = new([new(P.OrangeZoomerUpsideRight, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.OrangeZoomerUpsideLeft] = new([new(P.OrangeZoomerUpsideLeft, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.OrangeZoomerUpsideDown] = new([new(P.OrangeZoomerUpsideDown, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.OrangeZoomerUpsideUp] = new([new(P.OrangeZoomerUpsideUp, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SharedUpsideRight] = new([new(P.SharedUpsideRight, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SharedUpsideLeft] = new([new(P.SharedUpsideLeft, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SharedUpsideDown] = new([new(P.SharedUpsideDown, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SharedUpsideUp] = new([new(P.SharedUpsideUp, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<CrawlerSegment> StartingWith(CrawlerSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}

using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Enemies.Crawlers.CrawlerSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// The Sciser, Zero, Viola, orange Zoomer and shared Zoomer/Zeela/Sova surface animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue: the surface a crawler shows sets its crawl.
/// </summary>
internal static class CrawlerAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<CrawlerSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<CrawlerSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.SciserUpsideRight] =
        [
        ],
        [P.SciserUpsideRightLoop] =
        [
            Show(EnemyFrameId.SciserUpsideRightLegsTucked, 8),
            Show(EnemyFrameId.SciserUpsideRightLegsMid, 8),
            Show(EnemyFrameId.SciserUpsideRightLegsOut, 8),
            Show(EnemyFrameId.SciserUpsideRightLegsMid, 8),
        ],
        [P.SciserUpsideLeft] =
        [
        ],
        [P.SciserUpsideLeftLoop] =
        [
            Show(EnemyFrameId.SciserUpsideLeftLegsTucked, 8),
            Show(EnemyFrameId.SciserUpsideLeftLegsMid, 8),
            Show(EnemyFrameId.SciserUpsideLeftLegsOut, 8),
            Show(EnemyFrameId.SciserUpsideLeftLegsMid, 8),
        ],
        [P.SciserUpsideDown] =
        [
        ],
        [P.SciserUpsideDownLoop] =
        [
            Show(EnemyFrameId.SciserUpsideDownLegsTucked, 8),
            Show(EnemyFrameId.SciserUpsideDownLegsMid, 8),
            Show(EnemyFrameId.SciserUpsideDownLegsOut, 8),
            Show(EnemyFrameId.SciserUpsideDownLegsMid, 8),
        ],
        [P.SciserUpsideUp] =
        [
        ],
        [P.SciserUpsideUpLoop] =
        [
            Show(EnemyFrameId.SciserUpsideUpLegsTucked, 8),
            Show(EnemyFrameId.SciserUpsideUpLegsMid, 8),
            Show(EnemyFrameId.SciserUpsideUpLegsOut, 8),
            Show(EnemyFrameId.SciserUpsideUpLegsMid, 8),
        ],
        [P.ZeroUpsideRight] =
        [
        ],
        [P.ZeroUpsideRightLoop] =
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
        ],
        [P.ZeroUpsideLeftLoop] =
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
        ],
        [P.ZeroUpsideDownLoop] =
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
        ],
        [P.ZeroUpsideUpLoop] =
        [
            Show(EnemyFrameId.ZeroUpsideUpContracted, 4),
            Show(EnemyFrameId.ZeroUpsideUpStretching, 4),
            Show(EnemyFrameId.ZeroUpsideUpStretched, 4),
            Show(EnemyFrameId.ZeroUpsideUpFullyStretched, 4),
            Show(EnemyFrameId.ZeroUpsideUpStretched, 4),
            Show(EnemyFrameId.ZeroUpsideUpStretching, 4),
        ],
        [P.ViolaUpsideDown] =
        [
        ],
        [P.ViolaUpsideUp] =
        [
        ],
        [P.ViolaUpsideRight] =
        [
        ],
        [P.ViolaUpsideLeft] =
        [
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
        ],
        [P.OrangeZoomerUpsideRightLoop] =
        [
            Show(EnemyFrameId.OrangeZoomerUpsideRightSpinesTurned0, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideRightSpinesTurned12, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideRightSpinesTurned24, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideRightSpinesTurned36, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideRightSpinesTurned48, 3),
        ],
        [P.OrangeZoomerUpsideLeft] =
        [
        ],
        [P.OrangeZoomerUpsideLeftLoop] =
        [
            Show(EnemyFrameId.OrangeZoomerUpsideLeftSpinesTurned0, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideLeftSpinesTurned12, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideLeftSpinesTurned24, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideLeftSpinesTurned36, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideLeftSpinesTurned48, 3),
        ],
        [P.OrangeZoomerUpsideDown] =
        [
        ],
        [P.OrangeZoomerUpsideDownLoop] =
        [
            Show(EnemyFrameId.OrangeZoomerUpsideDownSpinesTurned0, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideDownSpinesTurned12, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideDownSpinesTurned24, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideDownSpinesTurned36, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideDownSpinesTurned48, 3),
        ],
        [P.OrangeZoomerUpsideUp] =
        [
        ],
        [P.OrangeZoomerUpsideUpLoop] =
        [
            Show(EnemyFrameId.OrangeZoomerUpsideUpSpinesTurned0, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideUpSpinesTurned12, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideUpSpinesTurned24, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideUpSpinesTurned36, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideUpSpinesTurned48, 3),
        ],
        [P.SharedUpsideRight] =
        [
        ],
        [P.SharedUpsideRightLoop] =
        [
            Show(EnemyFrameId.OrangeZoomerUpsideRightSpinesTurned0, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideRightSpinesTurned12, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideRightSpinesTurned24, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideRightSpinesTurned36, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideRightSpinesTurned48, 3),
        ],
        [P.SharedUpsideLeft] =
        [
        ],
        [P.SharedUpsideLeftLoop] =
        [
            Show(EnemyFrameId.OrangeZoomerUpsideLeftSpinesTurned0, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideLeftSpinesTurned12, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideLeftSpinesTurned24, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideLeftSpinesTurned36, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideLeftSpinesTurned48, 3),
        ],
        [P.SharedUpsideDown] =
        [
        ],
        [P.SharedUpsideDownLoop] =
        [
            Show(EnemyFrameId.OrangeZoomerUpsideDownSpinesTurned0, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideDownSpinesTurned12, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideDownSpinesTurned24, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideDownSpinesTurned36, 3),
            Show(EnemyFrameId.OrangeZoomerUpsideDownSpinesTurned48, 3),
        ],
        [P.SharedUpsideUp] =
        [
        ],
        [P.SharedUpsideUpLoop] =
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
        [P.SciserUpsideRight] = new([new(P.SciserUpsideRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SciserUpsideRightLoop] = new([new(P.SciserUpsideRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SciserUpsideLeft] = new([new(P.SciserUpsideLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SciserUpsideLeftLoop] = new([new(P.SciserUpsideLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SciserUpsideDown] = new([new(P.SciserUpsideDownLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SciserUpsideDownLoop] = new([new(P.SciserUpsideDownLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SciserUpsideUp] = new([new(P.SciserUpsideUpLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SciserUpsideUpLoop] = new([new(P.SciserUpsideUpLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZeroUpsideRight] = new([new(P.ZeroUpsideRightLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZeroUpsideRightLoop] = new([new(P.ZeroUpsideRightLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZeroUpsideLeft] = new([new(P.ZeroUpsideLeftLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZeroUpsideLeftLoop] = new([new(P.ZeroUpsideLeftLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZeroUpsideDown] = new([new(P.ZeroUpsideDownLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZeroUpsideDownLoop] = new([new(P.ZeroUpsideDownLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZeroUpsideUp] = new([new(P.ZeroUpsideUpLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZeroUpsideUpLoop] = new([new(P.ZeroUpsideUpLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ViolaUpsideDown] = new([new(P.ViolaNormal, 0, 14, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ViolaUpsideUp] = new([new(P.ViolaNormal, 0, 14, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ViolaUpsideRight] = new([new(P.ViolaNormal, 0, 14, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ViolaUpsideLeft] = new([new(P.ViolaNormal, 0, 14, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ViolaNormal] = new([new(P.ViolaNormal, 0, 14, 1)], AnimationEnd.LoopsFrom(0)),
        [P.OrangeZoomerUpsideRight] = new([new(P.OrangeZoomerUpsideRightLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.OrangeZoomerUpsideRightLoop] = new([new(P.OrangeZoomerUpsideRightLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.OrangeZoomerUpsideLeft] = new([new(P.OrangeZoomerUpsideLeftLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.OrangeZoomerUpsideLeftLoop] = new([new(P.OrangeZoomerUpsideLeftLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.OrangeZoomerUpsideDown] = new([new(P.OrangeZoomerUpsideDownLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.OrangeZoomerUpsideDownLoop] = new([new(P.OrangeZoomerUpsideDownLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.OrangeZoomerUpsideUp] = new([new(P.OrangeZoomerUpsideUpLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.OrangeZoomerUpsideUpLoop] = new([new(P.OrangeZoomerUpsideUpLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SharedUpsideRight] = new([new(P.SharedUpsideRightLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SharedUpsideRightLoop] = new([new(P.SharedUpsideRightLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SharedUpsideLeft] = new([new(P.SharedUpsideLeftLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SharedUpsideLeftLoop] = new([new(P.SharedUpsideLeftLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SharedUpsideDown] = new([new(P.SharedUpsideDownLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SharedUpsideDownLoop] = new([new(P.SharedUpsideDownLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SharedUpsideUp] = new([new(P.SharedUpsideUpLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SharedUpsideUpLoop] = new([new(P.SharedUpsideUpLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<CrawlerSegment> StartingWith(CrawlerSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}

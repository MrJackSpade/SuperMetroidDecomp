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
        [P.SciserOnLeftWall] =
        [
            Show(EnemyFrameId.SciserOnLeftWallLegsTucked, 8),
            Show(EnemyFrameId.SciserOnLeftWallLegsMid, 8),
            Show(EnemyFrameId.SciserOnLeftWallLegsOut, 8),
            Show(EnemyFrameId.SciserOnLeftWallLegsMid, 8),
        ],
        [P.SciserOnRightWall] =
        [
            Show(EnemyFrameId.SciserOnRightWallLegsTucked, 8),
            Show(EnemyFrameId.SciserOnRightWallLegsMid, 8),
            Show(EnemyFrameId.SciserOnRightWallLegsOut, 8),
            Show(EnemyFrameId.SciserOnRightWallLegsMid, 8),
        ],
        [P.SciserOnCeiling] =
        [
            Show(EnemyFrameId.SciserOnCeilingLegsTucked, 8),
            Show(EnemyFrameId.SciserOnCeilingLegsMid, 8),
            Show(EnemyFrameId.SciserOnCeilingLegsOut, 8),
            Show(EnemyFrameId.SciserOnCeilingLegsMid, 8),
        ],
        [P.SciserOnFloor] =
        [
            Show(EnemyFrameId.SciserOnFloorLegsTucked, 8),
            Show(EnemyFrameId.SciserOnFloorLegsMid, 8),
            Show(EnemyFrameId.SciserOnFloorLegsOut, 8),
            Show(EnemyFrameId.SciserOnFloorLegsMid, 8),
        ],
        [P.ZeroOnLeftWall] =
        [
            Show(EnemyFrameId.ZeroOnLeftWallContracted, 4),
            Show(EnemyFrameId.ZeroOnLeftWallStretching, 4),
            Show(EnemyFrameId.ZeroOnLeftWallStretched, 4),
            Show(EnemyFrameId.ZeroOnLeftWallFullyStretched, 4),
            Show(EnemyFrameId.ZeroOnLeftWallStretched, 4),
            Show(EnemyFrameId.ZeroOnLeftWallStretching, 4),
        ],
        [P.ZeroOnRightWall] =
        [
            Show(EnemyFrameId.ZeroOnRightWallContracted, 4),
            Show(EnemyFrameId.ZeroOnRightWallStretching, 4),
            Show(EnemyFrameId.ZeroOnRightWallStretched, 4),
            Show(EnemyFrameId.ZeroOnRightWallFullyStretched, 4),
            Show(EnemyFrameId.ZeroOnRightWallStretched, 4),
            Show(EnemyFrameId.ZeroOnRightWallStretching, 4),
        ],
        [P.ZeroOnCeiling] =
        [
            Show(EnemyFrameId.ZeroOnCeilingContracted, 4),
            Show(EnemyFrameId.ZeroOnCeilingStretching, 4),
            Show(EnemyFrameId.ZeroOnCeilingStretched, 4),
            Show(EnemyFrameId.ZeroOnCeilingFullyStretched, 4),
            Show(EnemyFrameId.ZeroOnCeilingStretched, 4),
            Show(EnemyFrameId.ZeroOnCeilingStretching, 4),
        ],
        [P.ZeroOnFloor] =
        [
            Show(EnemyFrameId.ZeroOnFloorContracted, 4),
            Show(EnemyFrameId.ZeroOnFloorStretching, 4),
            Show(EnemyFrameId.ZeroOnFloorStretched, 4),
            Show(EnemyFrameId.ZeroOnFloorFullyStretched, 4),
            Show(EnemyFrameId.ZeroOnFloorStretched, 4),
            Show(EnemyFrameId.ZeroOnFloorStretching, 4),
        ],
        [P.ViolaIdle] =
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
        [P.SpinyCrawlerOnLeftWall] =
        [
            Show(EnemyFrameId.SpinyCrawlerOnLeftWallSpinesTurned0, 3),
            Show(EnemyFrameId.SpinyCrawlerOnLeftWallSpinesTurned12, 3),
            Show(EnemyFrameId.SpinyCrawlerOnLeftWallSpinesTurned24, 3),
            Show(EnemyFrameId.SpinyCrawlerOnLeftWallSpinesTurned36, 3),
            Show(EnemyFrameId.SpinyCrawlerOnLeftWallSpinesTurned48, 3),
        ],
        [P.SpinyCrawlerOnRightWall] =
        [
            Show(EnemyFrameId.SpinyCrawlerOnRightWallSpinesTurned0, 3),
            Show(EnemyFrameId.SpinyCrawlerOnRightWallSpinesTurned12, 3),
            Show(EnemyFrameId.SpinyCrawlerOnRightWallSpinesTurned24, 3),
            Show(EnemyFrameId.SpinyCrawlerOnRightWallSpinesTurned36, 3),
            Show(EnemyFrameId.SpinyCrawlerOnRightWallSpinesTurned48, 3),
        ],
        [P.SpinyCrawlerOnCeiling] =
        [
            Show(EnemyFrameId.SpinyCrawlerOnCeilingSpinesTurned0, 3),
            Show(EnemyFrameId.SpinyCrawlerOnCeilingSpinesTurned12, 3),
            Show(EnemyFrameId.SpinyCrawlerOnCeilingSpinesTurned24, 3),
            Show(EnemyFrameId.SpinyCrawlerOnCeilingSpinesTurned36, 3),
            Show(EnemyFrameId.SpinyCrawlerOnCeilingSpinesTurned48, 3),
        ],
        [P.SpinyCrawlerOnFloor] =
        [
            Show(EnemyFrameId.SpinyCrawlerOnFloorSpinesTurned0, 3),
            Show(EnemyFrameId.SpinyCrawlerOnFloorSpinesTurned12, 3),
            Show(EnemyFrameId.SpinyCrawlerOnFloorSpinesTurned24, 3),
            Show(EnemyFrameId.SpinyCrawlerOnFloorSpinesTurned36, 3),
            Show(EnemyFrameId.SpinyCrawlerOnFloorSpinesTurned48, 3),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    private static Dictionary<CrawlerSegment, Animation<CrawlerSegment>> StartingAt { get; } = Segments.EachLooping(
        P.SciserOnLeftWall, P.SciserOnRightWall, P.SciserOnCeiling, P.SciserOnFloor, P.ZeroOnLeftWall,
        P.ZeroOnRightWall, P.ZeroOnCeiling, P.ZeroOnFloor, P.ViolaIdle, P.SpinyCrawlerOnLeftWall,
        P.SpinyCrawlerOnRightWall, P.SpinyCrawlerOnCeiling, P.SpinyCrawlerOnFloor);

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<CrawlerSegment> StartingWith(CrawlerSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}

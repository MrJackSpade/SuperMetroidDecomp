using SuperMetroid.Core.Enemies.Crawlers;

namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Fixed direction and airborne-animation definitions for Yard.</summary>
internal static class YardDirectionDefinitions
{
    /// <summary>
    /// Each of the eight surface orientations' crawling and hiding animations, airborne
    /// facing, opposite direction and movement behaviour, by direction: right-side wall moving up and
    /// down, left-side wall moving up and down, ceiling moving left and right, floor moving left and right. Direction
    /// pairs reverse motion on the same surface. Each names the side of its block a landing crawl takes its signs from.
    /// </summary>
    internal static readonly Dictionary<YardDirection, YardDirectionDefinition> Directions = new()
    {
        [YardDirection.UpsideRightMovingUp] = new(YardSegment.CrawlingUpsideRightMovingUp, CrawlerSurfaceOrientation.UpsideDown,
            YardSegment.HidingUpsideRightMovingUp, HorizontalDirection.Left, YardDirection.UpsideRightMovingDown,
            YardMovement.CrawlingUpsideDownMovingLeft),
        [YardDirection.UpsideRightMovingDown] = new(YardSegment.CrawlingUpsideRightMovingDown, CrawlerSurfaceOrientation.UpsideUp,
            YardSegment.HidingUpsideRightMovingDown, HorizontalDirection.Right, YardDirection.UpsideRightMovingUp,
            YardMovement.CrawlingUpsideRightMovingDown),
        [YardDirection.UpsideLeftMovingUp] = new(YardSegment.CrawlingUpsideLeftMovingUp, CrawlerSurfaceOrientation.UpsideDown,
            YardSegment.HidingUpsideLeftMovingUp, HorizontalDirection.Right, YardDirection.UpsideLeftMovingDown,
            YardMovement.CrawlingUpsideLeftMovingUp),
        [YardDirection.UpsideLeftMovingDown] = new(YardSegment.CrawlingUpsideLeftMovingDown, CrawlerSurfaceOrientation.UpsideUp,
            YardSegment.HidingUpsideLeftMovingDown, HorizontalDirection.Left, YardDirection.UpsideLeftMovingUp,
            YardMovement.CrawlingUpsideLeftMovingDown),
        [YardDirection.UpsideDownMovingLeft] = new(YardSegment.CrawlingUpsideDownMovingLeft, CrawlerSurfaceOrientation.UpsideRight,
            YardSegment.HidingUpsideDownMovingLeft, HorizontalDirection.Right, YardDirection.UpsideDownMovingRight,
            YardMovement.CrawlingUpsideRightMovingUp),
        [YardDirection.UpsideDownMovingRight] = new(YardSegment.CrawlingUpsideDownMovingRight, CrawlerSurfaceOrientation.UpsideLeft,
            YardSegment.HidingUpsideDownMovingRight, HorizontalDirection.Left, YardDirection.UpsideDownMovingLeft,
            YardMovement.CrawlingUpsideDownMovingRight),
        [YardDirection.UpsideUpMovingLeft] = new(YardSegment.CrawlingUpsideUpMovingLeft, CrawlerSurfaceOrientation.UpsideRight,
            YardSegment.HidingUpsideUpMovingLeft, HorizontalDirection.Left, YardDirection.UpsideUpMovingRight,
            YardMovement.CrawlingUpsideUpMovingLeft),
        [YardDirection.UpsideUpMovingRight] = new(YardSegment.CrawlingUpsideUpMovingRight, CrawlerSurfaceOrientation.UpsideLeft,
            YardSegment.HidingUpsideUpMovingRight, HorizontalDirection.Right, YardDirection.UpsideUpMovingLeft,
            YardMovement.CrawlingUpsideUpMovingRight),
    };

    /// <summary>
    /// The visible and hidden airborne animations by facing. Detach, contact-kick and shot-launch repeat these
    /// pairs.
    /// </summary>
    internal static readonly Dictionary<HorizontalDirection, YardAirborneAnimationDefinition> AirborneFacings = new()
    {
        [HorizontalDirection.Left] = new(YardSegment.AirborneFacingLeft, YardSegment.HiddenUpsideUpMovingLeft),
        [HorizontalDirection.Right] = new(YardSegment.AirborneFacingRight, YardSegment.HiddenUpsideUpMovingRight),
    };
}

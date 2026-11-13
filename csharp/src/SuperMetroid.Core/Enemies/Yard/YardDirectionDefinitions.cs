using SuperMetroid.Core.Enemies.Crawlers;

namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Fixed direction and airborne-animation definitions for Yard.</summary>
internal static class YardDirectionDefinitions
{
    /// <summary>
    /// Each of the eight surface orientations' crawling and hiding animations, airborne
    /// facing, opposite direction and movement callback, by direction: right-side wall moving up and
    /// down, left-side wall moving up and down, ceiling moving left and right, floor moving left and right. Direction
    /// pairs reverse motion on the same surface. Each names the side of its block a landing crawl takes its signs from.
    /// </summary>
    internal static readonly Dictionary<YardDirection, YardDirectionDefinition> Directions = new()
    {
        [YardDirection.UpsideRightMovingUp] = new(YardSegment.CrawlingUpsideRightMovingUp, CrawlerSurfaceOrientation.UpsideDown,
            YardSegment.HidingUpsideRightMovingUp, YardAirborneFacing.Left, YardDirection.UpsideRightMovingDown,
            YardMovementFunction.CrawlingUpsideDownMovingLeft),
        [YardDirection.UpsideRightMovingDown] = new(YardSegment.CrawlingUpsideRightMovingDown, CrawlerSurfaceOrientation.UpsideUp,
            YardSegment.HidingUpsideRightMovingDown, YardAirborneFacing.Right, YardDirection.UpsideRightMovingUp,
            YardMovementFunction.CrawlingUpsideRightMovingDown),
        [YardDirection.UpsideLeftMovingUp] = new(YardSegment.CrawlingUpsideLeftMovingUp, CrawlerSurfaceOrientation.UpsideDown,
            YardSegment.HidingUpsideLeftMovingUp, YardAirborneFacing.Right, YardDirection.UpsideLeftMovingDown,
            YardMovementFunction.CrawlingUpsideLeftMovingUp),
        [YardDirection.UpsideLeftMovingDown] = new(YardSegment.CrawlingUpsideLeftMovingDown, CrawlerSurfaceOrientation.UpsideUp,
            YardSegment.HidingUpsideLeftMovingDown, YardAirborneFacing.Left, YardDirection.UpsideLeftMovingUp,
            YardMovementFunction.CrawlingUpsideLeftMovingDown),
        [YardDirection.UpsideDownMovingLeft] = new(YardSegment.CrawlingUpsideDownMovingLeft, CrawlerSurfaceOrientation.UpsideRight,
            YardSegment.HidingUpsideDownMovingLeft, YardAirborneFacing.Right, YardDirection.UpsideDownMovingRight,
            YardMovementFunction.CrawlingUpsideRightMovingUp),
        [YardDirection.UpsideDownMovingRight] = new(YardSegment.CrawlingUpsideDownMovingRight, CrawlerSurfaceOrientation.UpsideLeft,
            YardSegment.HidingUpsideDownMovingRight, YardAirborneFacing.Left, YardDirection.UpsideDownMovingLeft,
            YardMovementFunction.CrawlingUpsideDownMovingRight),
        [YardDirection.UpsideUpMovingLeft] = new(YardSegment.CrawlingUpsideUpMovingLeft, CrawlerSurfaceOrientation.UpsideRight,
            YardSegment.HidingUpsideUpMovingLeft, YardAirborneFacing.Left, YardDirection.UpsideUpMovingRight,
            YardMovementFunction.CrawlingUpsideUpMovingLeft),
        [YardDirection.UpsideUpMovingRight] = new(YardSegment.CrawlingUpsideUpMovingRight, CrawlerSurfaceOrientation.UpsideLeft,
            YardSegment.HidingUpsideUpMovingRight, YardAirborneFacing.Right, YardDirection.UpsideUpMovingLeft,
            YardMovementFunction.CrawlingUpsideUpMovingRight),
    };

    /// <summary>
    /// The visible and hidden airborne animations by facing. Detach, contact-kick and shot-launch repeat these
    /// pairs.
    /// </summary>
    internal static readonly Dictionary<YardAirborneFacing, YardAirborneAnimationDefinition> AirborneFacings = new()
    {
        [YardAirborneFacing.Left] = new(YardSegment.AirborneFacingLeft, YardSegment.HiddenUpsideUpMovingLeft),
        [YardAirborneFacing.Right] = new(YardSegment.AirborneFacingRight, YardSegment.HiddenUpsideUpMovingRight),
    };
}

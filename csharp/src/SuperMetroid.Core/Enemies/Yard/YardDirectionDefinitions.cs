using SuperMetroid.Core.Enemies.Crawlers;

namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Fixed direction and airborne-animation definitions for Yard.</summary>
internal static class YardDirectionDefinitions
{
    /// <summary>
    /// Each of the eight surface orientations' crawling and hiding animations, airborne
    /// facing, opposite direction and the crawl turning around into it begins, by direction: on a wall to its left moving
    /// up and down, on a wall to its right moving up and down, ceiling moving left and right, floor moving left and right. Direction
    /// pairs reverse motion on the same surface. Each names the side of its block a landing crawl takes its signs from.
    /// </summary>
    internal static readonly IdentityTable<YardDirection, YardDirectionDefinition> Directions = new(new Dictionary<YardDirection, YardDirectionDefinition>()
    {
        [YardDirection.OnLeftWallMovingUp] = new(YardSegment.CrawlingOnLeftWallMovingUp, CrawlerSurfaceOrientation.OnCeiling,
            YardSegment.HidingOnLeftWallMovingUp, HorizontalDirection.Left, YardDirection.OnLeftWallMovingDown,
            YardDirection.OnCeilingMovingLeft),
        [YardDirection.OnLeftWallMovingDown] = new(YardSegment.CrawlingOnLeftWallMovingDown, CrawlerSurfaceOrientation.OnFloor,
            YardSegment.HidingOnLeftWallMovingDown, HorizontalDirection.Right, YardDirection.OnLeftWallMovingUp,
            YardDirection.OnLeftWallMovingDown),
        [YardDirection.OnRightWallMovingUp] = new(YardSegment.CrawlingOnRightWallMovingUp, CrawlerSurfaceOrientation.OnCeiling,
            YardSegment.HidingOnRightWallMovingUp, HorizontalDirection.Right, YardDirection.OnRightWallMovingDown,
            YardDirection.OnRightWallMovingUp),
        [YardDirection.OnRightWallMovingDown] = new(YardSegment.CrawlingOnRightWallMovingDown, CrawlerSurfaceOrientation.OnFloor,
            YardSegment.HidingOnRightWallMovingDown, HorizontalDirection.Left, YardDirection.OnRightWallMovingUp,
            YardDirection.OnRightWallMovingDown),
        [YardDirection.OnCeilingMovingLeft] = new(YardSegment.CrawlingOnCeilingMovingLeft, CrawlerSurfaceOrientation.OnLeftWall,
            YardSegment.HidingOnCeilingMovingLeft, HorizontalDirection.Right, YardDirection.OnCeilingMovingRight,
            YardDirection.OnLeftWallMovingUp),
        [YardDirection.OnCeilingMovingRight] = new(YardSegment.CrawlingOnCeilingMovingRight, CrawlerSurfaceOrientation.OnRightWall,
            YardSegment.HidingOnCeilingMovingRight, HorizontalDirection.Left, YardDirection.OnCeilingMovingLeft,
            YardDirection.OnCeilingMovingRight),
        [YardDirection.OnFloorMovingLeft] = new(YardSegment.CrawlingOnFloorMovingLeft, CrawlerSurfaceOrientation.OnLeftWall,
            YardSegment.HidingOnFloorMovingLeft, HorizontalDirection.Left, YardDirection.OnFloorMovingRight,
            YardDirection.OnFloorMovingLeft),
        [YardDirection.OnFloorMovingRight] = new(YardSegment.CrawlingOnFloorMovingRight, CrawlerSurfaceOrientation.OnRightWall,
            YardSegment.HidingOnFloorMovingRight, HorizontalDirection.Right, YardDirection.OnFloorMovingLeft,
            YardDirection.OnFloorMovingRight),
    });

    /// <summary>
    /// The visible and hidden airborne animations by facing. Detach, contact-kick and shot-launch repeat these
    /// pairs.
    /// </summary>
    internal static readonly IdentityTable<HorizontalDirection, YardAirborneAnimationDefinition> AirborneFacings = new(new Dictionary<HorizontalDirection, YardAirborneAnimationDefinition>
    {
        [HorizontalDirection.Left] = new(YardSegment.AirborneFacingLeft, YardSegment.HiddenOnFloorMovingLeft),
        [HorizontalDirection.Right] = new(YardSegment.AirborneFacingRight, YardSegment.HiddenOnFloorMovingRight),
    });
}

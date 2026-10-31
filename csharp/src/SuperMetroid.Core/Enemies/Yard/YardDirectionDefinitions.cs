using SuperMetroid.Core.Enemies.Crawlers;

namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Fixed direction and airborne-animation definitions for Yard.</summary>
internal static class YardDirectionDefinitions
{
    /// <summary>
    /// $A3:CD42, $CDC2 and $CDD2: each of the eight surface orientations' crawling and hiding animations, airborne
    /// facing, opposite direction and native movement callback, by direction: right-side wall moving up and
    /// down, left-side wall moving up and down, ceiling moving left and right, floor moving left and right. Direction
    /// pairs reverse motion on the same surface. Each names the side of its block a landing crawl takes its signs from.
    /// </summary>
    internal static readonly Dictionary<YardDirection, YardDirectionDefinition> Directions = new()
    {
        [YardDirection.UpsideRightMovingUp] = new(YardProgram.CrawlingUpsideRightMovingUp, CrawlerSurfaceOrientation.UpsideDown,
            YardProgram.HidingUpsideRightMovingUp, YardAirborneFacing.Left, YardDirection.UpsideRightMovingDown,
            YardMovementFunction.CrawlingUpsideDownMovingLeft),
        [YardDirection.UpsideRightMovingDown] = new(YardProgram.CrawlingUpsideRightMovingDown, CrawlerSurfaceOrientation.UpsideUp,
            YardProgram.HidingUpsideRightMovingDown, YardAirborneFacing.Right, YardDirection.UpsideRightMovingUp,
            YardMovementFunction.CrawlingUpsideRightMovingDown),
        [YardDirection.UpsideLeftMovingUp] = new(YardProgram.CrawlingUpsideLeftMovingUp, CrawlerSurfaceOrientation.UpsideDown,
            YardProgram.HidingUpsideLeftMovingUp, YardAirborneFacing.Right, YardDirection.UpsideLeftMovingDown,
            YardMovementFunction.CrawlingUpsideLeftMovingUp),
        [YardDirection.UpsideLeftMovingDown] = new(YardProgram.CrawlingUpsideLeftMovingDown, CrawlerSurfaceOrientation.UpsideUp,
            YardProgram.HidingUpsideLeftMovingDown, YardAirborneFacing.Left, YardDirection.UpsideLeftMovingUp,
            YardMovementFunction.CrawlingUpsideLeftMovingDown),
        [YardDirection.UpsideDownMovingLeft] = new(YardProgram.CrawlingUpsideDownMovingLeft, CrawlerSurfaceOrientation.UpsideRight,
            YardProgram.HidingUpsideDownMovingLeft, YardAirborneFacing.Right, YardDirection.UpsideDownMovingRight,
            YardMovementFunction.CrawlingUpsideRightMovingUp),
        [YardDirection.UpsideDownMovingRight] = new(YardProgram.CrawlingUpsideDownMovingRight, CrawlerSurfaceOrientation.UpsideLeft,
            YardProgram.HidingUpsideDownMovingRight, YardAirborneFacing.Left, YardDirection.UpsideDownMovingLeft,
            YardMovementFunction.CrawlingUpsideDownMovingRight),
        [YardDirection.UpsideUpMovingLeft] = new(YardProgram.CrawlingUpsideUpMovingLeft, CrawlerSurfaceOrientation.UpsideRight,
            YardProgram.HidingUpsideUpMovingLeft, YardAirborneFacing.Left, YardDirection.UpsideUpMovingRight,
            YardMovementFunction.CrawlingUpsideUpMovingLeft),
        [YardDirection.UpsideUpMovingRight] = new(YardProgram.CrawlingUpsideUpMovingRight, CrawlerSurfaceOrientation.UpsideLeft,
            YardProgram.HidingUpsideUpMovingRight, YardAirborneFacing.Right, YardDirection.UpsideUpMovingLeft,
            YardMovementFunction.CrawlingUpsideUpMovingRight),
    };

    /// <summary>
    /// The visible and hidden airborne animations by facing. Native detach, contact-kick and shot-launch repeat these
    /// pairs at $A3:D1AB, $D50F and $D5A4.
    /// </summary>
    internal static readonly Dictionary<YardAirborneFacing, YardAirborneInstructionDefinition> AirborneFacings = new()
    {
        [YardAirborneFacing.Left] = new(YardProgram.AirborneFacingLeft, YardProgram.HiddenUpsideUpMovingLeft),
        [YardAirborneFacing.Right] = new(YardProgram.AirborneFacingRight, YardProgram.HiddenUpsideUpMovingRight),
    };
}

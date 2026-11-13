using SuperMetroid.Core.Enemies.Crawlers;

namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// One of the eight authored surface directions used by a Yard (Maridia snail).
/// </summary>
internal readonly record struct YardDirectionDefinition(
    YardSegment CrawlingAnimation,
    CrawlerSurfaceOrientation BlockSide,
    YardSegment HidingAnimation,
    YardAirborneFacing AirborneFacingDirection,
    YardDirection OppositeDirection,
    YardMovementFunction MovementFunction);

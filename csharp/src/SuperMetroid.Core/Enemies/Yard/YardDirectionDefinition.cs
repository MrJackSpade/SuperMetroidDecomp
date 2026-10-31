using SuperMetroid.Core.Enemies.Crawlers;

namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// One of the eight cartridge-authored surface directions used by a Yard (Maridia snail).
/// </summary>
internal readonly record struct YardDirectionDefinition(
    YardProgram CrawlingProgram,
    CrawlerSurfaceOrientation BlockSide,
    YardProgram HidingProgram,
    YardAirborneFacing AirborneFacingDirection,
    YardDirection OppositeDirection,
    YardMovementFunction MovementFunction);

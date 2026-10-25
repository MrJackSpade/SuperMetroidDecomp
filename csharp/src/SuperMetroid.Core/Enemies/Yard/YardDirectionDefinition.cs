namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// One of the eight cartridge-authored surface directions used by a Yard (Maridia snail).
/// </summary>
internal readonly record struct YardDirectionDefinition(
    YardProgram CrawlingProgram,
    int PropertyBits,
    YardProgram HidingProgram,
    YardAirborneFacing AirborneFacingDirection,
    int OppositeDirection,
    YardMovementFunction MovementFunction);

namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// One of the eight cartridge-authored surface directions used by a Yard (Maridia snail).
/// </summary>
internal readonly record struct YardDirectionDefinition(
    YardProgram CrawlingProgram,
    byte PropertyBits,
    YardProgram HidingProgram,
    YardAirborneFacing AirborneFacingDirection,
    ushort OppositeDirection,
    YardMovementFunction MovementFunction);

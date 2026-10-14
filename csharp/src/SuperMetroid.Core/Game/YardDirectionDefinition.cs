namespace SuperMetroid.Core.Game;

/// <summary>
/// One of the eight cartridge-authored surface directions used by a Yard (Maridia snail).
/// </summary>
internal readonly record struct YardDirectionDefinition(
    ushort CrawlingInstructionList,
    ushort PropertyBits,
    ushort HidingInstructionList,
    ushort AirborneFacingDirection,
    ushort OppositeDirection,
    YardMovementFunction MovementFunction);

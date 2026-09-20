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

/// <summary>Facing-specific animation lists installed while a Yard is airborne.</summary>
internal readonly record struct YardAirborneInstructionDefinition(
    ushort VisibleInstructionList,
    ushort HidingInstructionList);

/// <summary>Fixed direction and airborne-animation definitions for Yard.</summary>
internal static class YardDirectionDefinitions
{
    /// <summary>
    /// The eight direction records at <c>$A3:CD42-$A3:CD81</c>, joined with their
    /// corresponding opposite-direction words at <c>$A3:CDC2-$A3:CDD1</c> and movement
    /// functions at <c>$A3:CDD2-$A3:CDE1</c>.
    /// </summary>
    private static readonly YardDirectionDefinition[] Directions =
    [
        new(YardInstructionProgramDefinitions.CrawlingUpsideRightMovingUp, 0x0002,
            YardInstructionProgramDefinitions.HidingUpsideRightMovingUp, 0, 1,
            YardMovementFunction.CrawlingUpsideDownMovingLeft),
        new(YardInstructionProgramDefinitions.CrawlingUpsideRightMovingDown, 0x0003,
            YardInstructionProgramDefinitions.HidingUpsideRightMovingDown, 1, 0,
            YardMovementFunction.CrawlingUpsideRightMovingDown),
        new(YardInstructionProgramDefinitions.CrawlingUpsideLeftMovingUp, 0x0002,
            YardInstructionProgramDefinitions.HidingUpsideLeftMovingUp, 1, 3,
            YardMovementFunction.CrawlingUpsideLeftMovingUp),
        new(YardInstructionProgramDefinitions.CrawlingUpsideLeftMovingDown, 0x0003,
            YardInstructionProgramDefinitions.HidingUpsideLeftMovingDown, 0, 2,
            YardMovementFunction.CrawlingUpsideLeftMovingDown),
        new(YardInstructionProgramDefinitions.CrawlingUpsideDownMovingLeft, 0x0000,
            YardInstructionProgramDefinitions.HidingUpsideDownMovingLeft, 1, 5,
            YardMovementFunction.CrawlingUpsideRightMovingUp),
        new(YardInstructionProgramDefinitions.CrawlingUpsideDownMovingRight, 0x0001,
            YardInstructionProgramDefinitions.HidingUpsideDownMovingRight, 0, 4,
            YardMovementFunction.CrawlingUpsideDownMovingRight),
        new(YardInstructionProgramDefinitions.CrawlingUpsideUpMovingLeft, 0x0000,
            YardInstructionProgramDefinitions.HidingUpsideUpMovingLeft, 0, 7,
            YardMovementFunction.CrawlingUpsideUpMovingLeft),
        new(YardInstructionProgramDefinitions.CrawlingUpsideUpMovingRight, 0x0001,
            YardInstructionProgramDefinitions.HidingUpsideUpMovingRight, 1, 6,
            YardMovementFunction.CrawlingUpsideUpMovingRight),
    ];

    /// <summary>
    /// The facing-left and facing-right pairs duplicated at <c>$A3:D1AB-$A3:D1B2</c>,
    /// <c>$A3:D50F-$A3:D516</c>, and <c>$A3:D5A4-$A3:D5AB</c> by the native detach,
    /// contact-kick, and shot-launch routines.
    /// </summary>
    private static readonly YardAirborneInstructionDefinition[] AirborneInstructions =
    [
        new(YardInstructionProgramDefinitions.AirborneFacingLeft,
            YardInstructionProgramDefinitions.HiddenUpsideUpMovingLeft),
        new(YardInstructionProgramDefinitions.AirborneFacingRight,
            YardInstructionProgramDefinitions.HiddenUpsideUpMovingRight),
    ];

    /// <summary>Returns one of the eight physical surface-direction definitions.</summary>
    internal static YardDirectionDefinition ForDirection(ushort direction) =>
        direction < Directions.Length
            ? Directions[direction]
            : throw new InvalidDataException(
                $"Yard direction ${direction:X4} exceeds its eight cartridge definitions.");

    /// <summary>Returns the animation pair for native airborne facing zero or one.</summary>
    internal static YardAirborneInstructionDefinition ForAirborneFacing(ushort facing) =>
        facing < AirborneInstructions.Length
            ? AirborneInstructions[facing]
            : throw new InvalidDataException(
                $"Yard airborne facing ${facing:X4} exceeds its two cartridge definitions.");
}

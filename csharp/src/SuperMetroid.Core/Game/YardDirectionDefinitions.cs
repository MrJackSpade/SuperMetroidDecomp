namespace SuperMetroid.Core.Game;

/// <summary>
/// One of the eight cartridge-authored surface directions used by a Yard (Maridia snail).
/// </summary>
/// <param name="CrawlingInstructionList">Instruction list used while the Yard crawls along this surface orientation.</param>
/// <param name="PropertyBits">Native movement-property bits derived from the direction's motion axis and sign.</param>
/// <param name="HidingInstructionList">Instruction list used while the Yard hides in this orientation.</param>
/// <param name="AirborneFacingDirection">Facing selector used when this direction transitions to airborne behavior.</param>
/// <param name="OppositeDirection">Direction-table index for the paired orientation that reverses travel on the same surface.</param>
/// <param name="MovementFunction">Movement callback selected for this surface orientation.</param>
internal readonly record struct YardDirectionDefinition(
    ushort CrawlingInstructionList,
    ushort PropertyBits,
    ushort HidingInstructionList,
    ushort AirborneFacingDirection,
    ushort OppositeDirection,
    YardMovementFunction MovementFunction);

/// <summary>Facing-specific animation lists installed while a Yard is airborne.</summary>
/// <param name="VisibleInstructionList">Airborne animation played for the selected facing.</param>
/// <param name="HidingInstructionList">Hidden airborne animation used for the selected facing during detach or launch behavior.</param>
internal readonly record struct YardAirborneInstructionDefinition(
    ushort VisibleInstructionList,
    ushort HidingInstructionList);

/// <summary>Fixed direction and airborne-animation definitions for Yard.</summary>
internal static class YardDirectionDefinitions
{
    /// <summary>The eight mutually exclusive surface orientations at $A3:CD42.</summary>
    private enum SurfaceDirection : ushort
    {
        /// <summary>Ceiling-right wall surface with upward travel.</summary>
        UpsideRightMovingUp,

        /// <summary>Ceiling-right wall surface with downward travel.</summary>
        UpsideRightMovingDown,

        /// <summary>Ceiling-left wall surface with upward travel.</summary>
        UpsideLeftMovingUp,

        /// <summary>Ceiling-left wall surface with downward travel.</summary>
        UpsideLeftMovingDown,

        /// <summary>Bottom surface with leftward travel.</summary>
        UpsideDownMovingLeft,

        /// <summary>Bottom surface with rightward travel.</summary>
        UpsideDownMovingRight,

        /// <summary>Top surface with leftward travel.</summary>
        UpsideUpMovingLeft,

        /// <summary>Top surface with rightward travel.</summary>
        UpsideUpMovingRight,
    }

    /// <summary>
    /// Dispatches the physical orientation to its crawling/hiding animations and native
    /// movement callback ($A3:CD42, $CDC2 and $CDD2). Direction pairs reverse motion on
    /// the same surface. Property bit zero selects positive motion and bit one vertical motion.
    /// </summary>
    internal static YardDirectionDefinition ForDirection(ushort direction)
    {
        var selected = (SurfaceDirection)direction switch
        {
            SurfaceDirection.UpsideRightMovingUp => new YardDirectionDefinition(
                YardInstructionProgramDefinitions.CrawlingUpsideRightMovingUp, 0,
                YardInstructionProgramDefinitions.HidingUpsideRightMovingUp, 0, 0,
                YardMovementFunction.CrawlingUpsideDownMovingLeft),
            SurfaceDirection.UpsideRightMovingDown => new YardDirectionDefinition(
                YardInstructionProgramDefinitions.CrawlingUpsideRightMovingDown, 0,
                YardInstructionProgramDefinitions.HidingUpsideRightMovingDown, 1, 0,
                YardMovementFunction.CrawlingUpsideRightMovingDown),
            SurfaceDirection.UpsideLeftMovingUp => new YardDirectionDefinition(
                YardInstructionProgramDefinitions.CrawlingUpsideLeftMovingUp, 0,
                YardInstructionProgramDefinitions.HidingUpsideLeftMovingUp, 1, 0,
                YardMovementFunction.CrawlingUpsideLeftMovingUp),
            SurfaceDirection.UpsideLeftMovingDown => new YardDirectionDefinition(
                YardInstructionProgramDefinitions.CrawlingUpsideLeftMovingDown, 0,
                YardInstructionProgramDefinitions.HidingUpsideLeftMovingDown, 0, 0,
                YardMovementFunction.CrawlingUpsideLeftMovingDown),
            SurfaceDirection.UpsideDownMovingLeft => new YardDirectionDefinition(
                YardInstructionProgramDefinitions.CrawlingUpsideDownMovingLeft, 0,
                YardInstructionProgramDefinitions.HidingUpsideDownMovingLeft, 1, 0,
                YardMovementFunction.CrawlingUpsideRightMovingUp),
            SurfaceDirection.UpsideDownMovingRight => new YardDirectionDefinition(
                YardInstructionProgramDefinitions.CrawlingUpsideDownMovingRight, 0,
                YardInstructionProgramDefinitions.HidingUpsideDownMovingRight, 0, 0,
                YardMovementFunction.CrawlingUpsideDownMovingRight),
            SurfaceDirection.UpsideUpMovingLeft => new YardDirectionDefinition(
                YardInstructionProgramDefinitions.CrawlingUpsideUpMovingLeft, 0,
                YardInstructionProgramDefinitions.HidingUpsideUpMovingLeft, 0, 0,
                YardMovementFunction.CrawlingUpsideUpMovingLeft),
            SurfaceDirection.UpsideUpMovingRight => new YardDirectionDefinition(
                YardInstructionProgramDefinitions.CrawlingUpsideUpMovingRight, 0,
                YardInstructionProgramDefinitions.HidingUpsideUpMovingRight, 1, 0,
                YardMovementFunction.CrawlingUpsideUpMovingRight),
            _ => throw new InvalidDataException(
                $"Yard direction ${direction:X4} exceeds its eight cartridge definitions."),
        };
        return selected with
        {
            PropertyBits = (ushort)((direction & 1) | (direction < 4 ? 2 : 0)),
            OppositeDirection = (ushort)(direction ^ 1),
        };
    }

    /// <summary>
    /// Chooses visible and hidden airborne animations by facing. Native detach,
    /// contact-kick and shot-launch repeat these pairs at $A3:D1AB, $D50F and $D5A4.
    /// </summary>
    internal static YardAirborneInstructionDefinition ForAirborneFacing(ushort facing) => facing switch
    {
        0 => new(YardInstructionProgramDefinitions.AirborneFacingLeft,
            YardInstructionProgramDefinitions.HiddenUpsideUpMovingLeft),
        1 => new(YardInstructionProgramDefinitions.AirborneFacingRight,
            YardInstructionProgramDefinitions.HiddenUpsideUpMovingRight),
        _ => throw new InvalidDataException(
            $"Yard airborne facing ${facing:X4} exceeds its two cartridge definitions."),
    };
}

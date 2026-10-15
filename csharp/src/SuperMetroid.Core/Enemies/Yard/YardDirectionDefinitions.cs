namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Fixed direction and airborne-animation definitions for Yard.</summary>
internal static class YardDirectionDefinitions
{
    /// <summary>The eight mutually exclusive surface orientations at $A3:CD42.</summary>
    private enum SurfaceDirection : ushort
    {
        /// <summary>Right-side wall orientation with upward crawl.</summary>
        UpsideRightMovingUp,
        /// <summary>Right-side wall orientation with downward crawl.</summary>
        UpsideRightMovingDown,
        /// <summary>Left-side wall orientation with upward crawl.</summary>
        UpsideLeftMovingUp,
        /// <summary>Left-side wall orientation with downward crawl.</summary>
        UpsideLeftMovingDown,
        /// <summary>Ceiling orientation with leftward crawl.</summary>
        UpsideDownMovingLeft,
        /// <summary>Ceiling orientation with rightward crawl.</summary>
        UpsideDownMovingRight,
        /// <summary>Floor orientation with leftward crawl.</summary>
        UpsideUpMovingLeft,
        /// <summary>Floor orientation with rightward crawl.</summary>
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
                YardProgram.CrawlingUpsideRightMovingUp, 0,
                YardProgram.HidingUpsideRightMovingUp, 0, 0,
                YardMovementFunction.CrawlingUpsideDownMovingLeft),
            SurfaceDirection.UpsideRightMovingDown => new YardDirectionDefinition(
                YardProgram.CrawlingUpsideRightMovingDown, 0,
                YardProgram.HidingUpsideRightMovingDown, 1, 0,
                YardMovementFunction.CrawlingUpsideRightMovingDown),
            SurfaceDirection.UpsideLeftMovingUp => new YardDirectionDefinition(
                YardProgram.CrawlingUpsideLeftMovingUp, 0,
                YardProgram.HidingUpsideLeftMovingUp, 1, 0,
                YardMovementFunction.CrawlingUpsideLeftMovingUp),
            SurfaceDirection.UpsideLeftMovingDown => new YardDirectionDefinition(
                YardProgram.CrawlingUpsideLeftMovingDown, 0,
                YardProgram.HidingUpsideLeftMovingDown, 0, 0,
                YardMovementFunction.CrawlingUpsideLeftMovingDown),
            SurfaceDirection.UpsideDownMovingLeft => new YardDirectionDefinition(
                YardProgram.CrawlingUpsideDownMovingLeft, 0,
                YardProgram.HidingUpsideDownMovingLeft, 1, 0,
                YardMovementFunction.CrawlingUpsideRightMovingUp),
            SurfaceDirection.UpsideDownMovingRight => new YardDirectionDefinition(
                YardProgram.CrawlingUpsideDownMovingRight, 0,
                YardProgram.HidingUpsideDownMovingRight, 0, 0,
                YardMovementFunction.CrawlingUpsideDownMovingRight),
            SurfaceDirection.UpsideUpMovingLeft => new YardDirectionDefinition(
                YardProgram.CrawlingUpsideUpMovingLeft, 0,
                YardProgram.HidingUpsideUpMovingLeft, 0, 0,
                YardMovementFunction.CrawlingUpsideUpMovingLeft),
            SurfaceDirection.UpsideUpMovingRight => new YardDirectionDefinition(
                YardProgram.CrawlingUpsideUpMovingRight, 0,
                YardProgram.HidingUpsideUpMovingRight, 1, 0,
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
        0 => new(YardProgram.AirborneFacingLeft,
            YardProgram.HiddenUpsideUpMovingLeft),
        1 => new(YardProgram.AirborneFacingRight,
            YardProgram.HiddenUpsideUpMovingRight),
        _ => throw new InvalidDataException(
            $"Yard airborne facing ${facing:X4} exceeds its two cartridge definitions."),
    };
}

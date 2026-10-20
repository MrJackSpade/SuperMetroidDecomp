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
    internal static YardDirectionDefinition ForDirection(int direction)
    {
        var selected = (SurfaceDirection)direction switch
        {
            SurfaceDirection.UpsideRightMovingUp => new YardDirectionDefinition(
                YardProgram.CrawlingUpsideRightMovingUp, 0,
                YardProgram.HidingUpsideRightMovingUp, YardAirborneFacing.Left, 0,
                YardMovementFunction.CrawlingUpsideDownMovingLeft),
            SurfaceDirection.UpsideRightMovingDown => new YardDirectionDefinition(
                YardProgram.CrawlingUpsideRightMovingDown, 0,
                YardProgram.HidingUpsideRightMovingDown, YardAirborneFacing.Right, 0,
                YardMovementFunction.CrawlingUpsideRightMovingDown),
            SurfaceDirection.UpsideLeftMovingUp => new YardDirectionDefinition(
                YardProgram.CrawlingUpsideLeftMovingUp, 0,
                YardProgram.HidingUpsideLeftMovingUp, YardAirborneFacing.Right, 0,
                YardMovementFunction.CrawlingUpsideLeftMovingUp),
            SurfaceDirection.UpsideLeftMovingDown => new YardDirectionDefinition(
                YardProgram.CrawlingUpsideLeftMovingDown, 0,
                YardProgram.HidingUpsideLeftMovingDown, YardAirborneFacing.Left, 0,
                YardMovementFunction.CrawlingUpsideLeftMovingDown),
            SurfaceDirection.UpsideDownMovingLeft => new YardDirectionDefinition(
                YardProgram.CrawlingUpsideDownMovingLeft, 0,
                YardProgram.HidingUpsideDownMovingLeft, YardAirborneFacing.Right, 0,
                YardMovementFunction.CrawlingUpsideRightMovingUp),
            SurfaceDirection.UpsideDownMovingRight => new YardDirectionDefinition(
                YardProgram.CrawlingUpsideDownMovingRight, 0,
                YardProgram.HidingUpsideDownMovingRight, YardAirborneFacing.Left, 0,
                YardMovementFunction.CrawlingUpsideDownMovingRight),
            SurfaceDirection.UpsideUpMovingLeft => new YardDirectionDefinition(
                YardProgram.CrawlingUpsideUpMovingLeft, 0,
                YardProgram.HidingUpsideUpMovingLeft, YardAirborneFacing.Left, 0,
                YardMovementFunction.CrawlingUpsideUpMovingLeft),
            SurfaceDirection.UpsideUpMovingRight => new YardDirectionDefinition(
                YardProgram.CrawlingUpsideUpMovingRight, 0,
                YardProgram.HidingUpsideUpMovingRight, YardAirborneFacing.Right, 0,
                YardMovementFunction.CrawlingUpsideUpMovingRight),
            _ => throw new InvalidDataException(
                $"Yard direction ${direction:X4} exceeds its eight cartridge definitions."),
        };
        return selected with
        {
            PropertyBits = (byte)((direction & 1) | (direction < 4 ? 2 : 0)),
            OppositeDirection = (ushort)(direction ^ 1),
        };
    }

    /// <summary>
    /// Chooses visible and hidden airborne animations by facing. Native detach,
    /// contact-kick and shot-launch repeat these pairs at $A3:D1AB, $D50F and $D5A4.
    /// </summary>
    internal static YardAirborneInstructionDefinition ForAirborneFacing(YardAirborneFacing facing) => facing switch
    {
        YardAirborneFacing.Left => new(YardProgram.AirborneFacingLeft,
            YardProgram.HiddenUpsideUpMovingLeft),
        YardAirborneFacing.Right => new(YardProgram.AirborneFacingRight,
            YardProgram.HiddenUpsideUpMovingRight),
        _ => throw new InvalidDataException(
            $"Yard airborne facing {facing} exceeds its two cartridge definitions."),
    };
}

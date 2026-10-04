namespace SuperMetroid.Core.Game;

/// <summary>One physical lookahead and callback-selector record for a Yard surface turn.</summary>
internal readonly record struct YardTurnDefinition(
    short LookaheadX,
    short LookaheadY,
    ushort OutsideTurnInstructionList,
    ushort InsideTurnInstructionList);

/// <summary>Compiled turn geometry and callback metadata for the Maridia Yard.</summary>
internal static class YardTurnDefinitions
{
    /// <summary>
    /// Selects the record used by the native movement dispatcher at
    /// <c>$A3:CFA6-$A3:CFFF</c>. Only the four horizontal crawling states have a
    /// transition-disabled alternative. Ordinary turns look seven pixels along motion;
    /// disabled turns use zero lookahead and the corresponding vertical crawl callbacks.
    /// This directly dispatches the native $CCE2-$CD41 turn records by movement state.
    /// </summary>
    internal static YardTurnDefinition ForMovement(
        YardMovementFunction movement,
        bool transitionDisabled) => movement switch
    {
        YardMovementFunction.CrawlingUpsideUpMovingLeft =>
            transitionDisabled ? new(0, 0, YardInstructionProgramDefinitions.CrawlingUpsideLeftMovingDown,
            YardInstructionProgramDefinitions.CrawlingUpsideRightMovingUp) : new(-7, 0, YardInstructionProgramDefinitions.OutsideTurnUpsideUpMovingLeft,
            YardInstructionProgramDefinitions.InsideTurnUpsideUpMovingLeft),
        YardMovementFunction.CrawlingUpsideLeftMovingDown => new(0, 7, YardInstructionProgramDefinitions.OutsideTurnUpsideLeftMovingDown,
            YardInstructionProgramDefinitions.InsideTurnUpsideLeftMovingDown),
        YardMovementFunction.CrawlingUpsideDownMovingRight =>
            transitionDisabled ? new(0, 0, YardInstructionProgramDefinitions.CrawlingUpsideRightMovingUp,
            YardInstructionProgramDefinitions.CrawlingUpsideLeftMovingDown) : new(7, 0, YardInstructionProgramDefinitions.OutsideTurnUpsideDownMovingRight,
            YardInstructionProgramDefinitions.InsideTurnUpsideDownMovingRight),
        YardMovementFunction.CrawlingUpsideRightMovingUp => new(0, -7, YardInstructionProgramDefinitions.OutsideTurnUpsideRightMovingUp,
            YardInstructionProgramDefinitions.InsideTurnUpsideRightMovingUp),
        YardMovementFunction.CrawlingUpsideUpMovingRight =>
            transitionDisabled ? new(0, 0, YardInstructionProgramDefinitions.CrawlingUpsideRightMovingDown,
            YardInstructionProgramDefinitions.CrawlingUpsideLeftMovingUp) : new(7, 0, YardInstructionProgramDefinitions.OutsideTurnUpsideUpMovingRight,
            YardInstructionProgramDefinitions.InsideTurnUpsideUpMovingRight),
        YardMovementFunction.CrawlingUpsideRightMovingDown => new(0, 7, YardInstructionProgramDefinitions.OutsideTurnUpsideRightMovingDown,
            YardInstructionProgramDefinitions.InsideTurnUpsideRightMovingDown),
        YardMovementFunction.CrawlingUpsideDownMovingLeft =>
            transitionDisabled ? new(0, 0, YardInstructionProgramDefinitions.CrawlingUpsideLeftMovingUp,
            YardInstructionProgramDefinitions.CrawlingUpsideRightMovingDown) : new(-7, 0, YardInstructionProgramDefinitions.OutsideTurnUpsideDownMovingLeft,
            YardInstructionProgramDefinitions.InsideTurnUpsideDownMovingLeft),
        YardMovementFunction.CrawlingUpsideLeftMovingUp => new(0, -7, YardInstructionProgramDefinitions.OutsideTurnUpsideLeftMovingUp,
            YardInstructionProgramDefinitions.InsideTurnUpsideLeftMovingUp),
        _ => throw new InvalidDataException(
            $"Yard crawl function $A3:{(ushort)movement:X4} has no turn definition."),
    };
}

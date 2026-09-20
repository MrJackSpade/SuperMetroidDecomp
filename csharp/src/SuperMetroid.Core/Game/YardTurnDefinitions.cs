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
    /// The eight ordinary turn records at <c>$A3:CCE2-$A3:CD21</c>, ordered by the
    /// corresponding crawling movement functions beginning at <c>$A3:CFA6</c>.
    /// </summary>
    private static readonly YardTurnDefinition[] OrdinaryTurns =
    [
        new(-7, 0, YardInstructionProgramDefinitions.OutsideTurnUpsideUpMovingLeft,
            YardInstructionProgramDefinitions.InsideTurnUpsideUpMovingLeft),
        new(0, 7, YardInstructionProgramDefinitions.OutsideTurnUpsideLeftMovingDown,
            YardInstructionProgramDefinitions.InsideTurnUpsideLeftMovingDown),
        new(7, 0, YardInstructionProgramDefinitions.OutsideTurnUpsideDownMovingRight,
            YardInstructionProgramDefinitions.InsideTurnUpsideDownMovingRight),
        new(0, -7, YardInstructionProgramDefinitions.OutsideTurnUpsideRightMovingUp,
            YardInstructionProgramDefinitions.InsideTurnUpsideRightMovingUp),
        new(7, 0, YardInstructionProgramDefinitions.OutsideTurnUpsideUpMovingRight,
            YardInstructionProgramDefinitions.InsideTurnUpsideUpMovingRight),
        new(0, 7, YardInstructionProgramDefinitions.OutsideTurnUpsideRightMovingDown,
            YardInstructionProgramDefinitions.InsideTurnUpsideRightMovingDown),
        new(-7, 0, YardInstructionProgramDefinitions.OutsideTurnUpsideDownMovingLeft,
            YardInstructionProgramDefinitions.InsideTurnUpsideDownMovingLeft),
        new(0, -7, YardInstructionProgramDefinitions.OutsideTurnUpsideLeftMovingUp,
            YardInstructionProgramDefinitions.InsideTurnUpsideLeftMovingUp),
    ];

    /// <summary>
    /// The four zero-lookahead records at <c>$A3:CD22-$A3:CD41</c>, selected while
    /// slope alignment temporarily disables turn transitions. Their list pointers are
    /// still observable fallback callbacks even though the lookahead is suppressed.
    /// </summary>
    private static readonly YardTurnDefinition[] SuppressedTurns =
    [
        new(0, 0, YardInstructionProgramDefinitions.CrawlingUpsideLeftMovingDown,
            YardInstructionProgramDefinitions.CrawlingUpsideRightMovingUp),
        new(0, 0, YardInstructionProgramDefinitions.CrawlingUpsideRightMovingUp,
            YardInstructionProgramDefinitions.CrawlingUpsideLeftMovingDown),
        new(0, 0, YardInstructionProgramDefinitions.CrawlingUpsideRightMovingDown,
            YardInstructionProgramDefinitions.CrawlingUpsideLeftMovingUp),
        new(0, 0, YardInstructionProgramDefinitions.CrawlingUpsideLeftMovingUp,
            YardInstructionProgramDefinitions.CrawlingUpsideRightMovingDown),
    ];

    /// <summary>
    /// Selects the record used by the native movement dispatcher at
    /// <c>$A3:CFA6-$A3:CFFF</c>. Only the four horizontal crawling states have a
    /// transition-disabled alternative.
    /// </summary>
    internal static YardTurnDefinition ForMovement(
        YardMovementFunction movement,
        bool transitionDisabled) => movement switch
    {
        YardMovementFunction.CrawlingUpsideUpMovingLeft =>
            transitionDisabled ? SuppressedTurns[0] : OrdinaryTurns[0],
        YardMovementFunction.CrawlingUpsideLeftMovingDown => OrdinaryTurns[1],
        YardMovementFunction.CrawlingUpsideDownMovingRight =>
            transitionDisabled ? SuppressedTurns[1] : OrdinaryTurns[2],
        YardMovementFunction.CrawlingUpsideRightMovingUp => OrdinaryTurns[3],
        YardMovementFunction.CrawlingUpsideUpMovingRight =>
            transitionDisabled ? SuppressedTurns[2] : OrdinaryTurns[4],
        YardMovementFunction.CrawlingUpsideRightMovingDown => OrdinaryTurns[5],
        YardMovementFunction.CrawlingUpsideDownMovingLeft =>
            transitionDisabled ? SuppressedTurns[3] : OrdinaryTurns[6],
        YardMovementFunction.CrawlingUpsideLeftMovingUp => OrdinaryTurns[7],
        _ => throw new InvalidDataException(
            $"Yard crawl function $A3:{(ushort)movement:X4} has no turn definition."),
    };
}

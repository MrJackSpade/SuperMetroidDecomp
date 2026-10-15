namespace SuperMetroid.Core.Enemies.Yard;

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
            transitionDisabled ? new(new(0, 0), YardProgram.CrawlingUpsideLeftMovingDown,
            YardProgram.CrawlingUpsideRightMovingUp) : new(new(-7, 0), YardProgram.OutsideTurnUpsideUpMovingLeft,
            YardProgram.InsideTurnUpsideUpMovingLeft),
        YardMovementFunction.CrawlingUpsideLeftMovingDown => new(new(0, 7), YardProgram.OutsideTurnUpsideLeftMovingDown,
            YardProgram.InsideTurnUpsideLeftMovingDown),
        YardMovementFunction.CrawlingUpsideDownMovingRight =>
            transitionDisabled ? new(new(0, 0), YardProgram.CrawlingUpsideRightMovingUp,
            YardProgram.CrawlingUpsideLeftMovingDown) : new(new(7, 0), YardProgram.OutsideTurnUpsideDownMovingRight,
            YardProgram.InsideTurnUpsideDownMovingRight),
        YardMovementFunction.CrawlingUpsideRightMovingUp => new(new(0, -7), YardProgram.OutsideTurnUpsideRightMovingUp,
            YardProgram.InsideTurnUpsideRightMovingUp),
        YardMovementFunction.CrawlingUpsideUpMovingRight =>
            transitionDisabled ? new(new(0, 0), YardProgram.CrawlingUpsideRightMovingDown,
            YardProgram.CrawlingUpsideLeftMovingUp) : new(new(7, 0), YardProgram.OutsideTurnUpsideUpMovingRight,
            YardProgram.InsideTurnUpsideUpMovingRight),
        YardMovementFunction.CrawlingUpsideRightMovingDown => new(new(0, 7), YardProgram.OutsideTurnUpsideRightMovingDown,
            YardProgram.InsideTurnUpsideRightMovingDown),
        YardMovementFunction.CrawlingUpsideDownMovingLeft =>
            transitionDisabled ? new(new(0, 0), YardProgram.CrawlingUpsideLeftMovingUp,
            YardProgram.CrawlingUpsideRightMovingDown) : new(new(-7, 0), YardProgram.OutsideTurnUpsideDownMovingLeft,
            YardProgram.InsideTurnUpsideDownMovingLeft),
        YardMovementFunction.CrawlingUpsideLeftMovingUp => new(new(0, -7), YardProgram.OutsideTurnUpsideLeftMovingUp,
            YardProgram.InsideTurnUpsideLeftMovingUp),
        _ => throw new InvalidDataException(
            $"Yard crawl function $A3:{(ushort)movement:X4} has no turn definition."),
    };
}

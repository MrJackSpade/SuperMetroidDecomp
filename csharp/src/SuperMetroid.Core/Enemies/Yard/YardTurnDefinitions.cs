namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Compiled turn geometry and callback metadata for the Maridia Yard.</summary>
internal static class YardTurnDefinitions
{
    /// <summary>
    /// The native $CCE2-$CD41 turn records the movement dispatcher at <c>$A3:CFA6-$A3:CFFF</c> uses for each crawling
    /// state. Ordinary turns look seven pixels along motion.
    /// </summary>
    internal static readonly Dictionary<YardMovementFunction, YardTurnDefinition> Turns = new()
    {
        [YardMovementFunction.CrawlingUpsideUpMovingLeft] = new(new(-7, 0), YardProgram.OutsideTurnUpsideUpMovingLeft, YardProgram.InsideTurnUpsideUpMovingLeft),
        [YardMovementFunction.CrawlingUpsideLeftMovingDown] = new(new(0, 7), YardProgram.OutsideTurnUpsideLeftMovingDown, YardProgram.InsideTurnUpsideLeftMovingDown),
        [YardMovementFunction.CrawlingUpsideDownMovingRight] = new(new(7, 0), YardProgram.OutsideTurnUpsideDownMovingRight, YardProgram.InsideTurnUpsideDownMovingRight),
        [YardMovementFunction.CrawlingUpsideRightMovingUp] = new(new(0, -7), YardProgram.OutsideTurnUpsideRightMovingUp, YardProgram.InsideTurnUpsideRightMovingUp),
        [YardMovementFunction.CrawlingUpsideUpMovingRight] = new(new(7, 0), YardProgram.OutsideTurnUpsideUpMovingRight, YardProgram.InsideTurnUpsideUpMovingRight),
        [YardMovementFunction.CrawlingUpsideRightMovingDown] = new(new(0, 7), YardProgram.OutsideTurnUpsideRightMovingDown, YardProgram.InsideTurnUpsideRightMovingDown),
        [YardMovementFunction.CrawlingUpsideDownMovingLeft] = new(new(-7, 0), YardProgram.OutsideTurnUpsideDownMovingLeft, YardProgram.InsideTurnUpsideDownMovingLeft),
        [YardMovementFunction.CrawlingUpsideLeftMovingUp] = new(new(0, -7), YardProgram.OutsideTurnUpsideLeftMovingUp, YardProgram.InsideTurnUpsideLeftMovingUp),
    };

    /// <summary>
    /// The transition-disabled alternatives, which only the four horizontal crawling states have: zero lookahead and
    /// the corresponding vertical crawl callbacks.
    /// </summary>
    internal static readonly Dictionary<YardMovementFunction, YardTurnDefinition> DisabledTurns = new()
    {
        [YardMovementFunction.CrawlingUpsideUpMovingLeft] = new(new(0, 0), YardProgram.CrawlingUpsideLeftMovingDown, YardProgram.CrawlingUpsideRightMovingUp),
        [YardMovementFunction.CrawlingUpsideDownMovingRight] = new(new(0, 0), YardProgram.CrawlingUpsideRightMovingUp, YardProgram.CrawlingUpsideLeftMovingDown),
        [YardMovementFunction.CrawlingUpsideUpMovingRight] = new(new(0, 0), YardProgram.CrawlingUpsideRightMovingDown, YardProgram.CrawlingUpsideLeftMovingUp),
        [YardMovementFunction.CrawlingUpsideDownMovingLeft] = new(new(0, 0), YardProgram.CrawlingUpsideLeftMovingUp, YardProgram.CrawlingUpsideRightMovingDown),
    };
}

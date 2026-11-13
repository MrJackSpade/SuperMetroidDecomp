namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Compiled turn geometry and callback metadata for the Maridia Yard.</summary>
internal static class YardTurnDefinitions
{
    /// <summary>
    /// The turn records the movement dispatcher uses for each crawling
    /// state. Ordinary turns look seven pixels along motion.
    /// </summary>
    internal static readonly Dictionary<YardMovementFunction, YardTurnDefinition> Turns = new()
    {
        [YardMovementFunction.CrawlingUpsideUpMovingLeft] = new(new(-7, 0), YardSegment.OutsideTurnUpsideUpMovingLeft, YardSegment.InsideTurnUpsideUpMovingLeft),
        [YardMovementFunction.CrawlingUpsideLeftMovingDown] = new(new(0, 7), YardSegment.OutsideTurnUpsideLeftMovingDown, YardSegment.InsideTurnUpsideLeftMovingDown),
        [YardMovementFunction.CrawlingUpsideDownMovingRight] = new(new(7, 0), YardSegment.OutsideTurnUpsideDownMovingRight, YardSegment.InsideTurnUpsideDownMovingRight),
        [YardMovementFunction.CrawlingUpsideRightMovingUp] = new(new(0, -7), YardSegment.OutsideTurnUpsideRightMovingUp, YardSegment.InsideTurnUpsideRightMovingUp),
        [YardMovementFunction.CrawlingUpsideUpMovingRight] = new(new(7, 0), YardSegment.OutsideTurnUpsideUpMovingRight, YardSegment.InsideTurnUpsideUpMovingRight),
        [YardMovementFunction.CrawlingUpsideRightMovingDown] = new(new(0, 7), YardSegment.OutsideTurnUpsideRightMovingDown, YardSegment.InsideTurnUpsideRightMovingDown),
        [YardMovementFunction.CrawlingUpsideDownMovingLeft] = new(new(-7, 0), YardSegment.OutsideTurnUpsideDownMovingLeft, YardSegment.InsideTurnUpsideDownMovingLeft),
        [YardMovementFunction.CrawlingUpsideLeftMovingUp] = new(new(0, -7), YardSegment.OutsideTurnUpsideLeftMovingUp, YardSegment.InsideTurnUpsideLeftMovingUp),
    };

    /// <summary>
    /// The transition-disabled alternatives, which only the four horizontal crawling states have: zero lookahead and
    /// the corresponding vertical crawl callbacks.
    /// </summary>
    internal static readonly Dictionary<YardMovementFunction, YardTurnDefinition> DisabledTurns = new()
    {
        [YardMovementFunction.CrawlingUpsideUpMovingLeft] = new(new(0, 0), YardSegment.CrawlingUpsideLeftMovingDown, YardSegment.CrawlingUpsideRightMovingUp),
        [YardMovementFunction.CrawlingUpsideDownMovingRight] = new(new(0, 0), YardSegment.CrawlingUpsideRightMovingUp, YardSegment.CrawlingUpsideLeftMovingDown),
        [YardMovementFunction.CrawlingUpsideUpMovingRight] = new(new(0, 0), YardSegment.CrawlingUpsideRightMovingDown, YardSegment.CrawlingUpsideLeftMovingUp),
        [YardMovementFunction.CrawlingUpsideDownMovingLeft] = new(new(0, 0), YardSegment.CrawlingUpsideLeftMovingUp, YardSegment.CrawlingUpsideRightMovingDown),
    };
}

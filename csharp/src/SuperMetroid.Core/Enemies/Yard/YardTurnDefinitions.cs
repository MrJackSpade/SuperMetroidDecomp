namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Compiled turn geometry and behaviour metadata for the Maridia Yard.</summary>
internal static class YardTurnDefinitions
{
    /// <summary>
    /// The turn records the movement dispatcher uses for each crawling
    /// state. Ordinary turns look seven pixels along motion.
    /// </summary>
    internal static readonly Dictionary<YardMovement, YardTurnDefinition> Turns = new()
    {
        [YardMovement.CrawlingUpsideUpMovingLeft] = new(new(-7, 0), YardSegment.OutsideTurnUpsideUpMovingLeft, YardSegment.InsideTurnUpsideUpMovingLeft),
        [YardMovement.CrawlingUpsideLeftMovingDown] = new(new(0, 7), YardSegment.OutsideTurnUpsideLeftMovingDown, YardSegment.InsideTurnUpsideLeftMovingDown),
        [YardMovement.CrawlingUpsideDownMovingRight] = new(new(7, 0), YardSegment.OutsideTurnUpsideDownMovingRight, YardSegment.InsideTurnUpsideDownMovingRight),
        [YardMovement.CrawlingUpsideRightMovingUp] = new(new(0, -7), YardSegment.OutsideTurnUpsideRightMovingUp, YardSegment.InsideTurnUpsideRightMovingUp),
        [YardMovement.CrawlingUpsideUpMovingRight] = new(new(7, 0), YardSegment.OutsideTurnUpsideUpMovingRight, YardSegment.InsideTurnUpsideUpMovingRight),
        [YardMovement.CrawlingUpsideRightMovingDown] = new(new(0, 7), YardSegment.OutsideTurnUpsideRightMovingDown, YardSegment.InsideTurnUpsideRightMovingDown),
        [YardMovement.CrawlingUpsideDownMovingLeft] = new(new(-7, 0), YardSegment.OutsideTurnUpsideDownMovingLeft, YardSegment.InsideTurnUpsideDownMovingLeft),
        [YardMovement.CrawlingUpsideLeftMovingUp] = new(new(0, -7), YardSegment.OutsideTurnUpsideLeftMovingUp, YardSegment.InsideTurnUpsideLeftMovingUp),
    };

    /// <summary>
    /// The transition-disabled alternatives, which only the four horizontal crawling states have: zero lookahead and
    /// the corresponding vertical crawl behaviours.
    /// </summary>
    internal static readonly Dictionary<YardMovement, YardTurnDefinition> DisabledTurns = new()
    {
        [YardMovement.CrawlingUpsideUpMovingLeft] = new(new(0, 0), YardSegment.CrawlingUpsideLeftMovingDown, YardSegment.CrawlingUpsideRightMovingUp),
        [YardMovement.CrawlingUpsideDownMovingRight] = new(new(0, 0), YardSegment.CrawlingUpsideRightMovingUp, YardSegment.CrawlingUpsideLeftMovingDown),
        [YardMovement.CrawlingUpsideUpMovingRight] = new(new(0, 0), YardSegment.CrawlingUpsideRightMovingDown, YardSegment.CrawlingUpsideLeftMovingUp),
        [YardMovement.CrawlingUpsideDownMovingLeft] = new(new(0, 0), YardSegment.CrawlingUpsideLeftMovingUp, YardSegment.CrawlingUpsideRightMovingDown),
    };
}

namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>The Maridia Yard's turn geometry and behaviour.</summary>
internal static class YardTurnDefinitions
{
    /// <summary>
    /// The turn records the movement step uses for each way the Yard crawls. Ordinary turns look seven pixels along motion.
    /// </summary>
    internal static readonly Dictionary<YardDirection, YardTurnDefinition> Turns = new()
    {
        [YardDirection.UpsideUpMovingLeft] = new(new(-7, 0), YardSegment.OutsideTurnUpsideUpMovingLeft, YardSegment.InsideTurnUpsideUpMovingLeft),
        [YardDirection.UpsideLeftMovingDown] = new(new(0, 7), YardSegment.OutsideTurnUpsideLeftMovingDown, YardSegment.InsideTurnUpsideLeftMovingDown),
        [YardDirection.UpsideDownMovingRight] = new(new(7, 0), YardSegment.OutsideTurnUpsideDownMovingRight, YardSegment.InsideTurnUpsideDownMovingRight),
        [YardDirection.UpsideRightMovingUp] = new(new(0, -7), YardSegment.OutsideTurnUpsideRightMovingUp, YardSegment.InsideTurnUpsideRightMovingUp),
        [YardDirection.UpsideUpMovingRight] = new(new(7, 0), YardSegment.OutsideTurnUpsideUpMovingRight, YardSegment.InsideTurnUpsideUpMovingRight),
        [YardDirection.UpsideRightMovingDown] = new(new(0, 7), YardSegment.OutsideTurnUpsideRightMovingDown, YardSegment.InsideTurnUpsideRightMovingDown),
        [YardDirection.UpsideDownMovingLeft] = new(new(-7, 0), YardSegment.OutsideTurnUpsideDownMovingLeft, YardSegment.InsideTurnUpsideDownMovingLeft),
        [YardDirection.UpsideLeftMovingUp] = new(new(0, -7), YardSegment.OutsideTurnUpsideLeftMovingUp, YardSegment.InsideTurnUpsideLeftMovingUp),
    };

    /// <summary>
    /// How the four crawls along a floor or ceiling continue while the Yard's turns are suppressed, which the crawls along
    /// a wall do not have (#2572): with no lookahead, at the crawl along a wall each continues at when its surface ends or
    /// when it meets one.
    /// </summary>
    internal static readonly Dictionary<YardDirection, YardSuppressedCrawl> SuppressedCrawls = new()
    {
        [YardDirection.UpsideUpMovingLeft] = new(YardSegment.CrawlingUpsideLeftMovingDown, YardSegment.CrawlingUpsideRightMovingUp),
        [YardDirection.UpsideDownMovingRight] = new(YardSegment.CrawlingUpsideRightMovingUp, YardSegment.CrawlingUpsideLeftMovingDown),
        [YardDirection.UpsideUpMovingRight] = new(YardSegment.CrawlingUpsideRightMovingDown, YardSegment.CrawlingUpsideLeftMovingUp),
        [YardDirection.UpsideDownMovingLeft] = new(YardSegment.CrawlingUpsideLeftMovingUp, YardSegment.CrawlingUpsideRightMovingDown),
    };
}

namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>The Maridia Yard's turn geometry and behaviour.</summary>
internal static class YardTurnDefinitions
{
    /// <summary>
    /// The corner lookahead and turn animations for each way the Yard crawls. Ordinary turns look seven pixels along
    /// motion.
    /// </summary>
    internal static readonly IdentityTable<YardDirection, YardTurnDefinition> Turns = new(new Dictionary<YardDirection, YardTurnDefinition>
    {
        [YardDirection.OnFloorMovingLeft] = new(new(-7, 0), YardSegment.OutsideTurnOnFloorMovingLeft, YardSegment.InsideTurnOnFloorMovingLeft),
        [YardDirection.OnRightWallMovingDown] = new(new(0, 7), YardSegment.OutsideTurnOnRightWallMovingDown, YardSegment.InsideTurnOnRightWallMovingDown),
        [YardDirection.OnCeilingMovingRight] = new(new(7, 0), YardSegment.OutsideTurnOnCeilingMovingRight, YardSegment.InsideTurnOnCeilingMovingRight),
        [YardDirection.OnLeftWallMovingUp] = new(new(0, -7), YardSegment.OutsideTurnOnLeftWallMovingUp, YardSegment.InsideTurnOnLeftWallMovingUp),
        [YardDirection.OnFloorMovingRight] = new(new(7, 0), YardSegment.OutsideTurnOnFloorMovingRight, YardSegment.InsideTurnOnFloorMovingRight),
        [YardDirection.OnLeftWallMovingDown] = new(new(0, 7), YardSegment.OutsideTurnOnLeftWallMovingDown, YardSegment.InsideTurnOnLeftWallMovingDown),
        [YardDirection.OnCeilingMovingLeft] = new(new(-7, 0), YardSegment.OutsideTurnOnCeilingMovingLeft, YardSegment.InsideTurnOnCeilingMovingLeft),
        [YardDirection.OnRightWallMovingUp] = new(new(0, -7), YardSegment.OutsideTurnOnRightWallMovingUp, YardSegment.InsideTurnOnRightWallMovingUp),
    });

    /// <summary>
    /// How the four crawls along a floor or ceiling continue while the Yard's turns are suppressed, which the crawls along
    /// a wall do not have (#2572): with no lookahead, at the crawl along a wall each continues at when its surface ends or
    /// when it meets one.
    /// </summary>
    internal static readonly Dictionary<YardDirection, YardSuppressedCrawl> SuppressedCrawls = new()
    {
        [YardDirection.OnFloorMovingLeft] = new(YardSegment.CrawlingOnRightWallMovingDown, YardSegment.CrawlingOnLeftWallMovingUp),
        [YardDirection.OnCeilingMovingRight] = new(YardSegment.CrawlingOnLeftWallMovingUp, YardSegment.CrawlingOnRightWallMovingDown),
        [YardDirection.OnFloorMovingRight] = new(YardSegment.CrawlingOnLeftWallMovingDown, YardSegment.CrawlingOnRightWallMovingUp),
        [YardDirection.OnCeilingMovingLeft] = new(YardSegment.CrawlingOnRightWallMovingUp, YardSegment.CrawlingOnLeftWallMovingDown),
    };
}

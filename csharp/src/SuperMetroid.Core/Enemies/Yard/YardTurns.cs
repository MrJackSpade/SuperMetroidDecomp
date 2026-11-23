namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Chooses how a crawling Yard's step goes on at the end of its surface or against another.</summary>
internal static partial class YardTurns
{
    /// <summary>
    /// The step of a crawl <paramref name="crawl"/>: its turn, or, while the Yard's turns are suppressed
    /// (<paramref name="turnsSuppressed"/>), its suppressed crawl when it has one.
    /// </summary>
    /// <exception cref="InvalidDataException"><paramref name="crawl"/> is not one of the eight ways a Yard crawls.</exception>
    internal static YardCrawlStep ForCrawl(
        YardDirection crawl,
        bool turnsSuppressed)
    {
        if (turnsSuppressed && YardTurnDefinitions.SuppressedCrawls.TryGetValue(crawl, out YardSuppressedCrawl suppressed))
            return new(Offset16<RoomPixels>.Zero, suppressed.WhenSurfaceEnds, suppressed.WhenSurfaceMet);
        YardTurnDefinition turn = YardTurnDefinitions.Turns[crawl];
        return new(turn.Lookahead, turn.OutsideTurnAnimation, turn.InsideTurnAnimation);
    }
}

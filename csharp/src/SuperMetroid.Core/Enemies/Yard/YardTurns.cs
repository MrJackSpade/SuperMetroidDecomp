namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Chooses the turn a crawling Yard uses.</summary>
internal static class YardTurns
{
    /// <summary>
    /// The turn of a crawl <paramref name="crawl"/>, or, while the Yard's turns are suppressed
    /// (<paramref name="turnsSuppressed"/>), its suppressed turn when it has one.
    /// </summary>
    /// <exception cref="InvalidDataException"><paramref name="crawl"/> is not one of the eight ways a Yard crawls.</exception>
    internal static YardTurnDefinition ForCrawl(
        YardDirection crawl,
        bool turnsSuppressed)
    {
        if (turnsSuppressed && YardTurnDefinitions.SuppressedTurns.TryGetValue(crawl, out YardTurnDefinition suppressed))
            return suppressed;
        return YardTurnDefinitions.Turns.TryGetValue(crawl, out YardTurnDefinition turn)
            ? turn
            : throw new InvalidDataException(
                $"Yard crawl {crawl} has no turn definition.");
    }
}

using SuperMetroid.Core.Enemies.Crawlers;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Zero crawler OAM identities shown by its four surface-orientation loops.
/// Orientation changes, velocity, timers and collision stay in the crawler code.
/// </summary>
internal static class ZeroVisualDefinitions
{
    /// <summary>Zero instruction and OAM bank $A3.</summary>
    internal const byte Bank = 0xa3;

    /// <summary>Sixteen selected Spritemap_Zero records at $A3:995B..9B6A.</summary>
    internal const int FrameCount = 16;

    /// <summary>
    /// Exports the distinct OAM pointers of the four orientation loops in surface-table order. Repeated intermediate
    /// poses stay in the programs' timing rather than duplicating editable artwork entries.
    /// </summary>
    internal static EnemySpritemapDefinition[] Frames()
    {
        ushort[] pointers = [.. CrawlerAnimations.Spritemaps(CrawlerProgram.ZeroUpsideRightLoop, CrawlerProgram.ZeroUpsideLeftLoop,
            CrawlerProgram.ZeroUpsideDownLoop, CrawlerProgram.ZeroUpsideUpLoop).Distinct()];
        if (pointers.Length != FrameCount)
            throw new InvalidDataException($"Zero programs show {pointers.Length} distinct OAM frames, expected {FrameCount}.");
        return [.. pointers.Select((pointer, index) => new EnemySpritemapDefinition(Bank, pointer, $"zero_crawler_{index:D2}"))];
    }
}

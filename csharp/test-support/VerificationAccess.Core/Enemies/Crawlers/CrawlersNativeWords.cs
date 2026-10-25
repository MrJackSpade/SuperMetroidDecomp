using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// Verification's native view of the Crawlers identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class CrawlersNativeWords
{
    /// <summary>The routine word naming each <see cref="CrawlerEnemyFunction"/>.</summary>
    internal static readonly NativeWordIdentities<CrawlerEnemyFunction> CrawlerEnemyFunctions = new(new Dictionary<CrawlerEnemyFunction, ushort>
    {
        [CrawlerEnemyFunction.HZoomerInstructionPending] = 0xe08a,
        [CrawlerEnemyFunction.HZoomerCrawlingVertically] = 0xe091,
        [CrawlerEnemyFunction.HZoomerCrawlingHorizontally] = 0xe168,
        [CrawlerEnemyFunction.InstructionPending] = 0xe6c1,
        [CrawlerEnemyFunction.CrawlingVertically] = 0xe6c8,
        [CrawlerEnemyFunction.Falling] = 0xe785,
        [CrawlerEnemyFunction.CrawlingHorizontally] = 0xe7f2,
    });
}

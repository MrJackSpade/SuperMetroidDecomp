using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// Verification's native view of the Crawlers identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class CrawlersNativeWords
{
    /// <summary>The routine word naming each <see cref="CrawlerAction"/>.</summary>
    internal static readonly NativeWordIdentities<CrawlerAction> CrawlerActions = new(new Dictionary<CrawlerAction, ushort>
    {
        [CrawlerAction.HZoomerAwaitingAnimationCue] = 0xe08a,
        [CrawlerAction.HZoomerCrawlingVertically] = 0xe091,
        [CrawlerAction.HZoomerCrawlingHorizontally] = 0xe168,
        [CrawlerAction.AwaitingAnimationCue] = 0xe6c1,
        [CrawlerAction.CrawlingVertically] = 0xe6c8,
        [CrawlerAction.Falling] = 0xe785,
        [CrawlerAction.CrawlingHorizontally] = 0xe7f2,
    });
}

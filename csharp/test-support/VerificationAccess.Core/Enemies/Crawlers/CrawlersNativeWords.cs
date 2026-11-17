using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// Verification's native view of the Crawlers identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class CrawlersNativeWords
{
    /// <summary>The routine word naming each <see cref="CrawlerAction"/> of a crawler that crawls by the shared crawl.</summary>
    internal static readonly NativeWordIdentities<CrawlerAction> SurfaceCrawlerActions = new(new Dictionary<CrawlerAction, ushort>
    {
        [CrawlerAction.Settling] = 0xe6c1,
        [CrawlerAction.CrawlingVertically] = 0xe6c8,
        [CrawlerAction.Falling] = 0xe785,
        [CrawlerAction.CrawlingHorizontally] = 0xe7f2,
    });

    /// <summary>
    /// The routine word naming each <see cref="CrawlerAction"/> of an orange Zoomer (#2283): its own routines settle it
    /// and crawl it after Samus, and it falls by the shared fall.
    /// </summary>
    internal static readonly NativeWordIdentities<CrawlerAction> OrangeZoomerActions = new(new Dictionary<CrawlerAction, ushort>
    {
        [CrawlerAction.Settling] = 0xe08a,
        [CrawlerAction.CrawlingVertically] = 0xe091,
        [CrawlerAction.CrawlingHorizontally] = 0xe168,
        [CrawlerAction.Falling] = 0xe785,
    });
}

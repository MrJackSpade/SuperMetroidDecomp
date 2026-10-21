using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.World.Artwork;

namespace SuperMetroid.Verification.Enemies.Crawlers;

/// <summary>Confirms four crawler species share the same compiled animation
/// selectors and installed HZoomer spritemap artwork.</summary>
internal static class SharedCrawlerArtworkVerification
{
    /// <summary>Confirms every pose the shared crawler loops show has installed artwork.</summary>
    internal static void VerifyInstalledSharedCrawlerFrames(EnemyTileArtworkCatalog stock)
    {
        // Zeela, Sova, Zoomer and Stone Zoomer share the orange Zoomer's installed compositions.
        foreach (ushort pose in CrawlerAnimationsConstants.Spritemaps(CrawlerProgram.SharedUpsideRightLoop, CrawlerProgram.SharedUpsideLeftLoop,
                     CrawlerProgram.SharedUpsideDownLoop, CrawlerProgram.SharedUpsideUpLoop))
            AssertTrue((stock.Spritemaps ?? throw new InvalidOperationException("Verification requires stock.Spritemaps.")).TryGet(
                    EnemySpritemapDefinitions.HZoomerBank, pose, out _),
                $"Shared crawler pose ${pose:X4} uses installed HZoomer artwork");
    }
}

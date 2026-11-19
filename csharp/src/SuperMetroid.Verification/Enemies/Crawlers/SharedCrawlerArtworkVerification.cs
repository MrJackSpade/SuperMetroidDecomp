using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.Verification.Enemies.Crawlers;

/// <summary>Confirms four crawler species share the same compiled animation
/// selectors and installed orange Zoomer spritemap artwork.</summary>
internal static class SharedCrawlerArtworkVerification
{
    /// <summary>Confirms every pose the shared crawler loops show has installed artwork.</summary>
    internal static void VerifyInstalledSharedCrawlerFrames(EnemyTileSourceArtwork stock)
    {
        // Zeela, Sova, Zoomer and Stone Zoomer share the orange Zoomer's installed compositions.
        foreach (ushort pose in CrawlerAnimationsConstants.Spritemaps(CrawlerSegment.SpinyCrawlerOnLeftWall, CrawlerSegment.SpinyCrawlerOnRightWall,
                     CrawlerSegment.SpinyCrawlerOnCeiling, CrawlerSegment.SpinyCrawlerOnFloor))
            AssertTrue((stock.Spritemaps ?? throw new InvalidOperationException("Verification requires stock.Spritemaps.")).TryGet(
                    EnemySpritemapDefinitions.OrangeZoomerBank, pose, out _),
                $"Shared crawler pose ${pose:X4} uses installed OrangeZoomer artwork");
    }
}

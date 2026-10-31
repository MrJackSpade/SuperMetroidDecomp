using SuperMetroid.Core;
using SuperMetroid.Core.Enemies.Crawlers;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The cartridge's counting of crawler surfaces, for checks that read its two-bit orientation indices.</summary>
internal static class CrawlerOrientationOrder
{
    /// <summary>The four surface orientations in the order the two-bit orientation index counts them.</summary>
    internal static readonly IdentitySequence<CrawlerSurfaceOrientation> Orientations = new(
        CrawlerSurfaceOrientation.UpsideRight, CrawlerSurfaceOrientation.UpsideLeft,
        CrawlerSurfaceOrientation.UpsideDown, CrawlerSurfaceOrientation.UpsideUp);
}

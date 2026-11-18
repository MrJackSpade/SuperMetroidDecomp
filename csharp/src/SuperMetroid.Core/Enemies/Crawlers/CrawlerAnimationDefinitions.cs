namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>The surface animations of each crawler animation family, which crawlers start on and switch between at corners.</summary>
internal static class CrawlerAnimationDefinitions
{
    /// <summary>Each <see cref="CrawlerAnimationFamily"/>'s animations for the four surfaces.</summary>
    internal static readonly Dictionary<CrawlerAnimationFamily, CrawlerAnimationDefinition> Families = new()
    {
        [CrawlerAnimationFamily.Shared] = new(
            CrawlerSegment.SpinyCrawlerUpsideRight,
            CrawlerSegment.SpinyCrawlerUpsideLeft,
            CrawlerSegment.SpinyCrawlerUpsideDown,
            CrawlerSegment.SpinyCrawlerUpsideUp),
        [CrawlerAnimationFamily.Viola] = new(
            CrawlerSegment.ViolaIdle,
            CrawlerSegment.ViolaIdle,
            CrawlerSegment.ViolaIdle,
            CrawlerSegment.ViolaIdle),
        [CrawlerAnimationFamily.Sciser] = new(
            CrawlerSegment.SciserUpsideRight,
            CrawlerSegment.SciserUpsideLeft,
            CrawlerSegment.SciserUpsideDown,
            CrawlerSegment.SciserUpsideUp),
        [CrawlerAnimationFamily.Zero] = new(
            CrawlerSegment.ZeroUpsideRight,
            CrawlerSegment.ZeroUpsideLeft,
            CrawlerSegment.ZeroUpsideDown,
            CrawlerSegment.ZeroUpsideUp),
    };
}

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>The surface animations of each crawler animation family, which crawlers start on and switch between at corners.</summary>
internal static class CrawlerAnimationDefinitions
{
    /// <summary>Each <see cref="CrawlerAnimationFamily"/>'s animations for the four surfaces.</summary>
    internal static readonly Dictionary<CrawlerAnimationFamily, CrawlerAnimationDefinition> Families = new()
    {
        [CrawlerAnimationFamily.Shared] = new(
            CrawlerSegment.SharedUpsideRight,
            CrawlerSegment.SharedUpsideLeft,
            CrawlerSegment.SharedUpsideDown,
            CrawlerSegment.SharedUpsideUp),
        [CrawlerAnimationFamily.Viola] = new(
            CrawlerSegment.ViolaUpsideRight,
            CrawlerSegment.ViolaUpsideLeft,
            CrawlerSegment.ViolaUpsideDown,
            CrawlerSegment.ViolaUpsideUp),
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
        [CrawlerAnimationFamily.HZoomer] = new(
            CrawlerSegment.HZoomerUpsideRight,
            CrawlerSegment.HZoomerUpsideLeft,
            CrawlerSegment.HZoomerUpsideDown,
            CrawlerSegment.HZoomerUpsideUp),
    };
}

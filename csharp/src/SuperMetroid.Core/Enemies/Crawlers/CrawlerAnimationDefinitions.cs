namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>The surface animations of each crawler animation family, which crawlers start on and switch between at corners.</summary>
internal static class CrawlerAnimationDefinitions
{
    /// <summary>Each <see cref="CrawlerAnimationFamily"/>'s animations for the four surfaces.</summary>
    internal static readonly Dictionary<CrawlerAnimationFamily, CrawlerAnimationDefinition> Families = new()
    {
        [CrawlerAnimationFamily.Shared] = new(
            CrawlerProgram.SharedUpsideRight,
            CrawlerProgram.SharedUpsideLeft,
            CrawlerProgram.SharedUpsideDown,
            CrawlerProgram.SharedUpsideUp),
        [CrawlerAnimationFamily.Viola] = new(
            CrawlerProgram.ViolaUpsideRight,
            CrawlerProgram.ViolaUpsideLeft,
            CrawlerProgram.ViolaUpsideDown,
            CrawlerProgram.ViolaUpsideUp),
        [CrawlerAnimationFamily.Sciser] = new(
            CrawlerProgram.SciserUpsideRight,
            CrawlerProgram.SciserUpsideLeft,
            CrawlerProgram.SciserUpsideDown,
            CrawlerProgram.SciserUpsideUp),
        [CrawlerAnimationFamily.Zero] = new(
            CrawlerProgram.ZeroUpsideRight,
            CrawlerProgram.ZeroUpsideLeft,
            CrawlerProgram.ZeroUpsideDown,
            CrawlerProgram.ZeroUpsideUp),
        [CrawlerAnimationFamily.HZoomer] = new(
            CrawlerProgram.HZoomerUpsideRight,
            CrawlerProgram.HZoomerUpsideLeft,
            CrawlerProgram.HZoomerUpsideDown,
            CrawlerProgram.HZoomerUpsideUp),
    };
}

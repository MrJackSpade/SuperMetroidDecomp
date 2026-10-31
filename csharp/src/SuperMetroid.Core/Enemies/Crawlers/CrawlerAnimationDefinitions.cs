namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Initial and shared surface animation selectors for crawler enemies.</summary>
internal static class CrawlerAnimationDefinitions
{
    /// <summary>
    /// The four-word initial animation tables, one per
    /// <see cref="CrawlerAnimationFamily"/>.
    /// </summary>
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

    /// <summary>
    /// The family each even species instruction-table offset selects; the first three species share the
    /// common animation, the remaining species select Viola, Sciser and Zero respectively.
    /// </summary>
    internal static readonly Dictionary<int, CrawlerAnimationFamily> SurfaceFamilies = new()
    {
        [0] = CrawlerAnimationFamily.Shared,
        [2] = CrawlerAnimationFamily.Shared,
        [4] = CrawlerAnimationFamily.Shared,
        [6] = CrawlerAnimationFamily.Viola,
        [8] = CrawlerAnimationFamily.Sciser,
        [10] = CrawlerAnimationFamily.Zero,
    };
}

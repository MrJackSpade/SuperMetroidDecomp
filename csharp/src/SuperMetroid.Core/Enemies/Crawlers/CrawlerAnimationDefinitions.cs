namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>The surface animations of each crawler animation family, which crawlers start on and switch between at corners.</summary>
internal static partial class CrawlerAnimationDefinitions
{
    /// <summary>Each <see cref="CrawlerAnimationFamily"/>'s animations for the four surfaces.</summary>
    internal static readonly IdentityTable<CrawlerAnimationFamily, CrawlerAnimationDefinition> Families = new(new Dictionary<CrawlerAnimationFamily, CrawlerAnimationDefinition>
    {
        [CrawlerAnimationFamily.Shared] = new(
            CrawlerSegment.SpinyCrawlerOnLeftWall,
            CrawlerSegment.SpinyCrawlerOnRightWall,
            CrawlerSegment.SpinyCrawlerOnCeiling,
            CrawlerSegment.SpinyCrawlerOnFloor),
        [CrawlerAnimationFamily.Viola] = new(
            CrawlerSegment.ViolaIdle,
            CrawlerSegment.ViolaIdle,
            CrawlerSegment.ViolaIdle,
            CrawlerSegment.ViolaIdle),
        [CrawlerAnimationFamily.Sciser] = new(
            CrawlerSegment.SciserOnLeftWall,
            CrawlerSegment.SciserOnRightWall,
            CrawlerSegment.SciserOnCeiling,
            CrawlerSegment.SciserOnFloor),
        [CrawlerAnimationFamily.Zero] = new(
            CrawlerSegment.ZeroOnLeftWall,
            CrawlerSegment.ZeroOnRightWall,
            CrawlerSegment.ZeroOnCeiling,
            CrawlerSegment.ZeroOnFloor),
    });
}

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>The definitions the Crawlers species installs and the part each one is.</summary>
internal static class CrawlersPartDefinitions
{
    /// <summary>Each Crawlers definition and its part.</summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, CrawlersPart> Parts { get; } = new Dictionary<EnemyDefinitionId, CrawlersPart>
    {
        [EnemyDefinitionId.Sciser] = CrawlersPart.Sciser,
        [EnemyDefinitionId.Zero] = CrawlersPart.Zero,
        [EnemyDefinitionId.Viola] = CrawlersPart.Viola,
        [EnemyDefinitionId.Zeela] = CrawlersPart.Zeela,
        [EnemyDefinitionId.Sova] = CrawlersPart.Sova,
        [EnemyDefinitionId.Zoomer] = CrawlersPart.Zoomer,
        [EnemyDefinitionId.MZoomer] = CrawlersPart.MZoomer,
        [EnemyDefinitionId.HZoomer] = CrawlersPart.HZoomer,
    };
}

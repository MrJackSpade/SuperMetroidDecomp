namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// A Sciser, Zero, Viola, Zeela, Sova, Zoomer or armoured Zoomer (#2146): set up in its animation family on its
/// starting surface, then crawling it by the shared crawl.
/// </summary>
/// <param name="species">The crawler species owner.</param>
/// <param name="place">The place the crawler occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="family">The animation family the crawler's setup selects its surface animations from.</param>
/// <param name="settings">Where the crawler starts and how fast it crawls, as its population entry tells it.</param>
internal sealed class SurfaceCrawlerEntity(CrawlersBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, CrawlerAnimationFamily family, CrawlerSpawnSettings settings)
    : CrawlerEntity(place, common)
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal override CrawlerEnemyState State { get; } = CrawlersBehavior.InitializeCrawler(place, family, settings);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => species.RunCrawlerMainAi(Place, State, frame);
}

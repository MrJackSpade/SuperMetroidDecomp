namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// A Sciser, Zero, Viola, Zeela, Sova, Zoomer or armoured Zoomer (#2146): set up in its animation family on its
/// starting surface, then crawling it by the shared crawl.
/// </summary>
/// <param name="species">The crawler species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the crawler occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="family">The animation family the crawler's setup selects its surface animations from.</param>
internal sealed class SurfaceCrawlerEntity(CrawlersBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, CrawlerAnimationFamily family)
    : CrawlerEntity(occupant, place, common)
{
    /// <inheritdoc/>
    public override void Initialize() => CrawlersBehavior.InitializeCrawler(Occupant, Place, family);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => species.RunCrawlerMainAi(Place, Place.RequireSpeciesState<CrawlerEnemyState>(), frame);
}

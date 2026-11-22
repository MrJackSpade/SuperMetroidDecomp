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
/// <param name="settings">Where and how fast the crawler's population entry starts it crawling.</param>
internal sealed partial class SurfaceCrawlerEntity(CrawlersBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, CrawlerAnimationFamily family, CrawlerSpawnSettings settings)
    : CrawlerEntity(occupant, place, common, CrawlersBehavior.InitializeCrawler(settings, place, family))
{
    /// <summary>The <c>family</c> the object was made with, which it keeps so it can be made again from what it holds (#2602).</summary>
    private readonly CrawlerAnimationFamily family = family;

    /// <summary>The <c>settings</c> the object was made with, which it keeps so it can be made again from what it holds (#2602).</summary>
    private readonly CrawlerSpawnSettings settings = settings;

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => species.RunCrawlerMainAi(Place, State, frame);
}

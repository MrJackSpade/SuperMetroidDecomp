namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// A Sciser, Zero, Viola, Zeela, Sova, Zoomer or armoured Zoomer (#2146): set up in its animation family on its
/// starting surface, then crawling it by the shared crawl.
/// </summary>
/// <param name="species">The crawler species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the crawler occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="state">What the enemy's setup built, which its base holds (#2554).</param>
internal sealed partial class SurfaceCrawlerEntity(CrawlersBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, CrawlerEnemyState state)
    : CrawlerEntity(occupant, place, common, state)
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The crawler species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the crawler occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="family">The animation family the crawler's setup selects its surface animations from.</param>
    /// <param name="settings">Where and how fast the crawler's population entry starts it crawling.</param>
    internal SurfaceCrawlerEntity(CrawlersBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        CrawlerAnimationFamily family, CrawlerSpawnSettings settings)
        : this(species, occupant, place, common, CrawlersBehavior.InitializeCrawler(settings, place, family))
    {
    }

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => species.RunCrawlerMainAi(Place, State, frame);
}

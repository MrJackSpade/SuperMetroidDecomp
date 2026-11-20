namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>An orange Zoomer (#2146): set up with its placement's speed, then crawling its surface after Samus.</summary>
/// <param name="species">The crawler species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the orange Zoomer occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class OrangeZoomerEntity(CrawlersBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CrawlerEntity(occupant, place, common)
{
    /// <inheritdoc/>
    public override void Initialize() => CrawlersBehavior.InitializeOrangeZoomer(Occupant, Occupant.SpawnSettings<CrawlerSpawnSettings>(), Place);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => species.RunOrangeZoomerMainAi(Place, Place.RequireSpeciesState<CrawlerEnemyState>(), frame);
}

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>An orange Zoomer (#2146): set up with its placement's speed, then crawling its surface after Samus.</summary>
/// <param name="species">The crawler species owner.</param>
/// <param name="place">The place the orange Zoomer occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class OrangeZoomerEntity(CrawlersBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, OrangeZoomerSpawnSettings settings)
    : CrawlerEntity(occupant, place, common)
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal override CrawlerEnemyState State { get; } = CrawlersBehavior.InitializeOrangeZoomer(place, settings);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => species.RunOrangeZoomerMainAi(Place, State, frame);
}

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>An orange Zoomer (#2146): set up with its placement's speed, then crawling its surface after Samus.</summary>
/// <param name="species">The crawler species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the orange Zoomer occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="state">What the enemy's setup built, which its base holds (#2554).</param>
internal sealed partial class OrangeZoomerEntity(CrawlersBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, CrawlerEnemyState state)
    : CrawlerEntity(occupant, place, common, state)
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The crawler species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the orange Zoomer occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="settings">What the orange Zoomer's population entry tells its species.</param>
    internal OrangeZoomerEntity(CrawlersBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        OrangeZoomerSpawnSettings settings)
        : this(species, occupant, place, common, CrawlersBehavior.InitializeOrangeZoomer(settings, place))
    {
    }

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => species.RunOrangeZoomerMainAi(Place, State, frame);
}

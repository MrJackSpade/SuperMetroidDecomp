namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>An orange Zoomer (#2146): set up with its placement's speed, then crawling its surface after Samus.</summary>
/// <param name="species">The crawler species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the orange Zoomer occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the orange Zoomer's population entry tells its species.</param>
internal sealed partial class OrangeZoomerEntity(CrawlersBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, OrangeZoomerSpawnSettings settings)
    : CrawlerEntity(occupant, place, common, CrawlersBehavior.InitializeOrangeZoomer(settings, place))
{
    /// <summary>The constructor's <c>settings</c>, kept so a debugger state rebuilds the object through it (#2602).</summary>
    private readonly OrangeZoomerSpawnSettings settings = settings;

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => species.RunOrangeZoomerMainAi(Place, State, frame);
}

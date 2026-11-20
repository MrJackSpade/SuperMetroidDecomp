namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// One crawler occupant: a crawler that walks its surface by the shared crawl (<see cref="SurfaceCrawlerEntity"/>) or an
/// orange Zoomer that pursues Samus (<see cref="OrangeZoomerEntity"/>), each with the surface animations and the shared
/// reactions.
/// </summary>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the crawler occupies.</param>
/// <param name="common">The shared participation.</param>
internal abstract class CrawlerEntity(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public abstract void Initialize();

    /// <inheritdoc/>
    public abstract void RunMainAi(IEnemyAiFrame frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => CrawlersBehavior.AdvanceCrawlerTypedAnimation(Place);
}

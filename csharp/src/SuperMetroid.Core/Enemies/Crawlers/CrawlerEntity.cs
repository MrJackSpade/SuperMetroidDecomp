namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// One crawler occupant: a crawler that walks its surface by the shared crawl (<see cref="SurfaceCrawlerEntity"/>) or an
/// orange Zoomer that pursues Samus (<see cref="OrangeZoomerEntity"/>), each with the surface animations and the shared
/// reactions.
/// </summary>
/// <param name="place">The place the crawler occupies.</param>
/// <param name="common">The shared participation.</param>
internal abstract class CrawlerEntity(RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public abstract void Initialize();

    /// <inheritdoc/>
    public abstract void RunMainAi(IEnemyAiFrame frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => CrawlersBehavior.AdvanceCrawlerTypedAnimation(Place);
}

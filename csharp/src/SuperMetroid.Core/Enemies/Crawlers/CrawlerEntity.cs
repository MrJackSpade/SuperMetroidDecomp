namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// One crawler occupant: a crawler that walks its surface by the shared crawl (<see cref="SurfaceCrawlerEntity"/>) or an
/// orange Zoomer that pursues Samus (<see cref="OrangeZoomerEntity"/>), each with the surface animations and the shared
/// reactions.
/// </summary>
/// <param name="place">The place the crawler occupies.</param>
/// <param name="common">The shared participation.</param>
internal abstract class CrawlerEntity(RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The crawler's state, which its setup builds as it is built (#2554).</summary>
    internal abstract CrawlerEnemyState State { get; }

    /// <inheritdoc/>
    public abstract void RunMainAi(IEnemyAiFrame frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => CrawlersBehavior.AdvanceCrawlerTypedAnimation(Place, context);
}

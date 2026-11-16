namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>One crawler occupant: its family's initializer, the shared or orange-Zoomer main AI and the typed animation, with the shared reactions.</summary>
/// <param name="species">The crawler species owner.</param>
/// <param name="place">The place the crawler occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class CrawlerEntity(CrawlersBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize()
    {
        switch (CrawlersPartDefinitions.Parts[Place.DefinitionId])
        {
            case CrawlersPart.Sciser:
                CrawlersBehavior.InitializeSciserAi(Place);
                break;
            case CrawlersPart.Zero:
                CrawlersBehavior.InitializeZeroAi(Place);
                break;
            case CrawlersPart.Viola:
                CrawlersBehavior.InitializeViolaAi(Place);
                break;
            case CrawlersPart.Zeela or CrawlersPart.Sova or CrawlersPart.Zoomer or CrawlersPart.ArmoredZoomer:
                CrawlersBehavior.InitializeSharedCrawlerAi(Place);
                break;
            case CrawlersPart.OrangeZoomer:
                CrawlersBehavior.InitializeOrangeZoomer(Place);
                break;
            default:
                throw new InvalidOperationException($"Crawler entity bound to {Place.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame)
    {
        switch (CrawlersPartDefinitions.Parts[Place.DefinitionId])
        {
            case CrawlersPart.Sciser or CrawlersPart.Zero or CrawlersPart.Viola or CrawlersPart.Zeela or CrawlersPart.Sova or CrawlersPart.Zoomer or CrawlersPart.ArmoredZoomer:
                species.RunCrawlerMainAi(Place, frame);
                break;
            case CrawlersPart.OrangeZoomer:
                species.RunOrangeZoomerMainAi(Place, frame);
                break;
            default:
                throw new InvalidOperationException($"Crawler entity bound to {Place.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => CrawlersBehavior.AdvanceCrawlerTypedAnimation(Place, context);
}

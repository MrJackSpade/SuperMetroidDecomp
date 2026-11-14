namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>One crawler occupant: its family's initializer, the shared or orange-Zoomer main AI and the typed animation, with the shared reactions.</summary>
/// <param name="species">The crawler species owner.</param>
/// <param name="slot">The slot the crawler occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class CrawlerEntity(CrawlersBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize()
    {
        switch (CrawlersPartDefinitions.Parts[Slot.DefinitionId])
        {
            case CrawlersPart.Sciser:
                CrawlersBehavior.InitializeSciserAi(Slot);
                break;
            case CrawlersPart.Zero:
                CrawlersBehavior.InitializeZeroAi(Slot);
                break;
            case CrawlersPart.Viola:
                CrawlersBehavior.InitializeViolaAi(Slot);
                break;
            case CrawlersPart.Zeela or CrawlersPart.Sova or CrawlersPart.Zoomer or CrawlersPart.MZoomer:
                CrawlersBehavior.InitializeSharedCrawlerAi(Slot);
                break;
            case CrawlersPart.HZoomer:
                CrawlersBehavior.InitializeHZoomer(Slot);
                break;
            default:
                throw new InvalidOperationException($"Crawler entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame)
    {
        switch (CrawlersPartDefinitions.Parts[Slot.DefinitionId])
        {
            case CrawlersPart.Sciser or CrawlersPart.Zero or CrawlersPart.Viola or CrawlersPart.Zeela or CrawlersPart.Sova or CrawlersPart.Zoomer or CrawlersPart.MZoomer:
                species.RunCrawlerMainAi(Slot, frame);
                break;
            case CrawlersPart.HZoomer:
                species.RunHZoomerMainAi(Slot, frame);
                break;
            default:
                throw new InvalidOperationException($"Crawler entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => CrawlersBehavior.AdvanceCrawlerTypedAnimation(Slot, context);
}

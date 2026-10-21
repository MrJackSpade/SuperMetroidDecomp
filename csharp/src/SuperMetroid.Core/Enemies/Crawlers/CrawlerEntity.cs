namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>One crawler occupant: its family's initializer, the shared or orange-Zoomer main AI and the typed animation, with the shared reactions.</summary>
/// <param name="species">The crawler species owner.</param>
/// <param name="slot">The slot the crawler occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class CrawlerEntity(CrawlersBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => CrawlersPartDefinitions.Parts[Slot.DefinitionId] switch
    {
        CrawlersPart.Sciser => CrawlersBehavior.InitializeSciserAi,
        CrawlersPart.Zero => CrawlersBehavior.InitializeZeroAi,
        CrawlersPart.Viola => CrawlersBehavior.InitializeViolaAi,
        CrawlersPart.Zeela or CrawlersPart.Sova or CrawlersPart.Zoomer or CrawlersPart.MZoomer =>
            CrawlersBehavior.InitializeSharedCrawlerAi,
        CrawlersPart.HZoomer => CrawlersBehavior.InitializeHZoomer,
        _ => throw new InvalidOperationException($"Crawler entity bound to {Slot.DefinitionId}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => CrawlersPartDefinitions.Parts[Slot.DefinitionId] switch
    {
        CrawlersPart.Sciser or CrawlersPart.Zero or CrawlersPart.Viola or CrawlersPart.Zeela or
            CrawlersPart.Sova or CrawlersPart.Zoomer or CrawlersPart.MZoomer => species.RunCrawlerMainAi,
        CrawlersPart.HZoomer => species.RunHZoomerMainAi,
        _ => throw new InvalidOperationException($"Crawler entity bound to {Slot.DefinitionId}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => CrawlersBehavior.AdvanceCrawlerTypedAnimation;
}

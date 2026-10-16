namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>One crawler occupant: its family's initializer, the shared or orange-Zoomer main AI and the typed animation, with the shared reactions.</summary>
/// <param name="species">The crawler species owner.</param>
/// <param name="slot">The slot the crawler occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class CrawlerEntity(CrawlersBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's CrawlerEnemyState, held by its physical slot.</summary>
    internal CrawlerEnemyState? State
    {
        get => Slot.SpeciesState<CrawlerEnemyState>();
        set => Slot.SetSpeciesState(value);
    }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.Sciser => CrawlersBehavior.InitializeSciserAi,
        EnemyDefinitionId.Zero => CrawlersBehavior.InitializeZeroAi,
        EnemyDefinitionId.Viola => CrawlersBehavior.InitializeViolaAi,
        EnemyDefinitionId.Zeela or EnemyDefinitionId.Sova or EnemyDefinitionId.Zoomer or EnemyDefinitionId.MZoomer =>
            CrawlersBehavior.InitializeSharedCrawlerAi,
        EnemyDefinitionId.HZoomer => CrawlersBehavior.InitializeHZoomerAi,
        _ => throw new InvalidOperationException($"Crawler entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.Sciser or EnemyDefinitionId.Zero or EnemyDefinitionId.Viola or EnemyDefinitionId.Zeela or
            EnemyDefinitionId.Sova or EnemyDefinitionId.Zoomer or EnemyDefinitionId.MZoomer => species.RunCrawlerMainAi,
        EnemyDefinitionId.HZoomer => species.RunHZoomerMainAi,
        _ => throw new InvalidOperationException($"Crawler entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => CrawlersBehavior.AdvanceCrawlerTypedAnimation;
}

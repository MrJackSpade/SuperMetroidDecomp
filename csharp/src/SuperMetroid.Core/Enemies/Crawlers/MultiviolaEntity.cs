namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>One Multiviola occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Multiviola species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MultiviolaEntity(MultiviolaBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's MultiviolaEnemyState, held by its physical slot.</summary>
    internal MultiviolaEnemyState? State
    {
        get => Slot.SpeciesState<MultiviolaEnemyState>();
        set => Slot.SetSpeciesState(value);
    }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => MultiviolaBehavior.RunMultiviolaInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunMultiviolaMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => MultiviolaBehavior.AdvanceMultiviolaTypedAnimation;
}

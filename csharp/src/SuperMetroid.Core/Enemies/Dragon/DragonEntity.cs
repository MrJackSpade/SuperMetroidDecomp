namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>One Dragon occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Dragon species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class DragonEntity(DragonBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's DragonEnemyState, held by its physical slot.</summary>
    internal DragonEnemyState? State
    {
        get => Slot.SpeciesState<DragonEnemyState>();
        set => Slot.SetSpeciesState(value);
    }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.RunDragonInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunDragonMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => DragonBehavior.AdvanceDragonTypedAnimation;
}

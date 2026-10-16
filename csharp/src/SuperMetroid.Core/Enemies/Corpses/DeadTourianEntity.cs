namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One DeadTourian occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The DeadTourian species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class DeadTourianEntity(DeadTourianBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's DeadTourianCorpseEnemyState, held by its physical slot.</summary>
    internal DeadTourianCorpseEnemyState? State
    {
        get => Slot.SpeciesState<DeadTourianCorpseEnemyState>();
        set => Slot.SetSpeciesState(value);
    }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.RunDeadTourianInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunDeadTourianMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceDeadTourianTypedAnimation;
}

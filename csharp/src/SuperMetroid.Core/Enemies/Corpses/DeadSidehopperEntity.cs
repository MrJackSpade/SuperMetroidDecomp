namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One DeadSidehopper occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The DeadSidehopper species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class DeadSidehopperEntity(DeadSidehopperBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's DeadSidehopperEnemyState, held by its physical slot.</summary>
    internal DeadSidehopperEnemyState? State
    {
        get => Slot.SpeciesState<DeadSidehopperEnemyState>();
        set => Slot.SetSpeciesState(value);
    }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.RunDeadSidehopperInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunDeadSidehopperMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceDeadSidehopperTypedAnimation;
}

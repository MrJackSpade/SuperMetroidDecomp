namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>One Hoppers occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Hoppers species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class HoppersEntity(HoppersBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's HopperEnemyState, held by its physical slot.</summary>
    internal HopperEnemyState? State
    {
        get => Slot.SpeciesState<HopperEnemyState>();
        set => Slot.SetSpeciesState(value);
    }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.RunHoppersInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunHoppersMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceHoppersTypedAnimation;
}

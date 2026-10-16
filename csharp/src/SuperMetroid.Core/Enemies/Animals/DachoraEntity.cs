namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>One Dachora occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Dachora species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class DachoraEntity(DachoraBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's DachoraEnemyState, held by its physical slot.</summary>
    internal DachoraEnemyState? State
    {
        get => Slot.SpeciesState<DachoraEnemyState>();
        set => Slot.SetSpeciesState(value);
    }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => DachoraBehavior.RunDachoraInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunDachoraMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => DachoraBehavior.AdvanceDachoraTypedAnimation;
}

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>One escape Dachora occupant: its initialization, main AI and typed animation, with the shared reactions.</summary>
/// <param name="species">The escape-animals species owner.</param>
/// <param name="slot">The slot the animal occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class EscapeDachoraEntity(EscapeAnimalsBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's EscapeDachoraEnemyState, held by its physical slot.</summary>
    internal EscapeDachoraEnemyState? State
    {
        get => Slot.SpeciesState<EscapeDachoraEnemyState>();
        set => Slot.SetSpeciesState(value);
    }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.RunEscapeDachoraInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => EscapeAnimalsBehavior.RunEscapeDachoraMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceEscapeAnimalTypedAnimation;
}

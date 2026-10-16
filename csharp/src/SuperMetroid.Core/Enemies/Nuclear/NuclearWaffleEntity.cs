namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>One NuclearWaffle occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The NuclearWaffle species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class NuclearWaffleEntity(NuclearWaffleBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's NuclearWaffleEnemyState, held by its physical slot.</summary>
    internal NuclearWaffleEnemyState? State
    {
        get => Slot.SpeciesState<NuclearWaffleEnemyState>();
        set => Slot.SetSpeciesState(value);
    }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.RunNuclearWaffleInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunNuclearWaffleMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => NuclearWaffleBehavior.AdvanceNuclearWaffleTypedAnimation;
}

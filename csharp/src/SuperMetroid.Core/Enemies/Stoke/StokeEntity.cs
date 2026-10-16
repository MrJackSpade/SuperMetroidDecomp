namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>One Stoke occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Stoke species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class StokeEntity(StokeBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's StokeEnemyState, held by its physical slot.</summary>
    internal StokeEnemyState? State
    {
        get => Slot.SpeciesState<StokeEnemyState>();
        set => Slot.SetSpeciesState(value);
    }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => StokeBehavior.RunStokeInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunStokeMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceStokeTypedAnimation;
}

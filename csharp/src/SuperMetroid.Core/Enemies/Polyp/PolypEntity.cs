namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>One Polyp occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Polyp species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class PolypEntity(PolypBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's PolypEnemyState, held by its physical slot.</summary>
    internal PolypEnemyState? State
    {
        get => Slot.SpeciesState<PolypEnemyState>();
        set => Slot.SetSpeciesState(value);
    }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.RunPolypInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunPolypMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => PolypBehavior.AdvancePolypTypedAnimation;
}

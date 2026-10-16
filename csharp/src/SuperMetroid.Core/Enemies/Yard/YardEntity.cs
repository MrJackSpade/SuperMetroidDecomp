namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>One Yard occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Yard species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class YardEntity(YardEnemyBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's YardEnemyState, held by its physical slot.</summary>
    internal YardEnemyState? State
    {
        get => Slot.SpeciesState<YardEnemyState>();
        set => Slot.SetSpeciesState(value);
    }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => YardEnemyBehavior.RunYardInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunYardMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceYardTypedAnimation;
}

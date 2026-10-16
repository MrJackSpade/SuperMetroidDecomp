namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>One Hibashi occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Hibashi species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class HibashiEntity(HibashiBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's HibashiEnemyState, held by its physical slot.</summary>
    internal HibashiEnemyState? State
    {
        get => Slot.SpeciesState<HibashiEnemyState>();
        set => Slot.SetSpeciesState(value);
    }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => HibashiBehavior.RunHibashiInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunHibashiMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceHibashiTypedAnimation;
}

namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>One Zoa occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class ZoaEntity(RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's ZoaEnemyState, held by its physical slot.</summary>
    internal ZoaEnemyState? State
    {
        get => Slot.SpeciesState<ZoaEnemyState>();
        set => Slot.SetSpeciesState(value);
    }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => ZoaBehavior.RunZoaInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => ZoaBehavior.RunZoaMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => ZoaBehavior.AdvanceZoaTypedAnimation;
}

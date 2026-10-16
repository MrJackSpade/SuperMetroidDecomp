namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>One Mochtroid occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Mochtroid species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MochtroidEntity(MochtroidBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's MochtroidEnemyState, held by its physical slot.</summary>
    internal MochtroidEnemyState? State
    {
        get => Slot.SpeciesState<MochtroidEnemyState>();
        set => Slot.SetSpeciesState(value);
    }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => MochtroidBehavior.RunMochtroidInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunMochtroidMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => MochtroidBehavior.AdvanceMochtroidTypedAnimation;
}

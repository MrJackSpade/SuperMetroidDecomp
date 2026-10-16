namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>One Puyo occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Puyo species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class PuyoEntity(PuyoBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's PuyoEnemyState, held by its physical slot.</summary>
    internal PuyoEnemyState? State
    {
        get => Slot.SpeciesState<PuyoEnemyState>();
        set => Slot.SetSpeciesState(value);
    }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => PuyoBehavior.RunPuyoInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunPuyoMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => PuyoBehavior.AdvancePuyoTypedAnimation;
}

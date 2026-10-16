namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One DeadTorizo occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The DeadTorizo species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class DeadTorizoEntity(DeadTorizoBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.RunDeadTorizoInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunDeadTorizoMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceDeadTorizoTypedAnimation;
}

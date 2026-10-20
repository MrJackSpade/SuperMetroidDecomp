namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>One Boulder occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Boulder species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class BoulderEntity(BoulderBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => BoulderBehavior.InitializeBoulder;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunBoulderMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => BoulderBehavior.AdvanceBoulderTypedAnimation;
}

namespace SuperMetroid.Core.Enemies.Flies;

/// <summary>One Flies occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Flies species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class FliesEntity(FliesBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's state, set by its initializer.</summary>
    internal FlyEnemyState? State { get; set; }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => FliesBehavior.RunFliesInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunFliesMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => FliesBehavior.AdvanceFliesTypedAnimation;
}

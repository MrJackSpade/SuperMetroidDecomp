namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>One Rio occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Rio species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class RioEntity(RioBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's state, set by its initializer.</summary>
    internal RioEnemyState? State { get; set; }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => RioBehavior.RunRioInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunRioMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => RioBehavior.AdvanceRioTypedAnimation;
}

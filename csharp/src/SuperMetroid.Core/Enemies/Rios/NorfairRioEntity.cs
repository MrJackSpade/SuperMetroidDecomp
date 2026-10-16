namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>One NorfairRio occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The NorfairRio species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class NorfairRioEntity(NorfairRioBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's state, set by its initializer.</summary>
    internal NorfairRioEnemyState? State { get; set; }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.RunNorfairRioInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunNorfairRioMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => NorfairRioBehavior.AdvanceNorfairRioTypedAnimation;
}

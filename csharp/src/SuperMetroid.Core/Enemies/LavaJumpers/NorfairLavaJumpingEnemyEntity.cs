namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>One NorfairLavaJumpingEnemy occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The NorfairLavaJumpingEnemy species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class NorfairLavaJumpingEnemyEntity(NorfairLavaJumpingEnemyBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's state, set by its initializer.</summary>
    internal NorfairLavaJumpingEnemyState? State { get; set; }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.RunNorfairLavaJumpingEnemyInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunNorfairLavaJumpingEnemyMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => NorfairLavaJumpingEnemyBehavior.AdvanceNorfairLavaJumpingEnemyTypedAnimation;
}

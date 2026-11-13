namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>One Norfair lava-jumping enemy occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Norfair lava-jumping enemy species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class NorfairLavaJumpingEnemyEntity(NorfairLavaJumpingEnemyBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.InitializeNorfairLavaJumpingEnemy;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunNorfairLavaJumpingEnemyMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => NorfairLavaJumpingEnemyBehavior.AdvanceNorfairLavaJumpingEnemyTypedAnimation;
}

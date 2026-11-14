namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>One Norfair lava-jumping enemy occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Norfair lava-jumping enemy species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class NorfairLavaJumpingEnemyEntity(NorfairLavaJumpingEnemyBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeNorfairLavaJumpingEnemy(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunNorfairLavaJumpingEnemyMainAi(Slot, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => NorfairLavaJumpingEnemyBehavior.AdvanceNorfairLavaJumpingEnemyTypedAnimation(Slot, context);
}

namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>One Norfair lava-jumping enemy occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Norfair lava-jumping enemy species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class NorfairLavaJumpingEnemyEntity(NorfairLavaJumpingEnemyBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeNorfairLavaJumpingEnemy(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunNorfairLavaJumpingEnemyMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => NorfairLavaJumpingEnemyBehavior.AdvanceNorfairLavaJumpingEnemyTypedAnimation(Place, context);
}

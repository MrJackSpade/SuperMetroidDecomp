namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>
/// A jumping Squeept (#2164): it leaps out of the lava and falls back, the cosmetic follower its room lists right after it
/// (<see cref="NorfairLavaJumperFollowerEntity"/>) showing its second half while it rises.
/// </summary>
/// <param name="species">The Norfair lava-jumping enemy species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class NorfairLavaJumpingEnemyEntity(NorfairLavaJumpingEnemyBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => NorfairLavaJumpingEnemyBehavior.InitializeNorfairLavaJumpingEnemy(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunNorfairLavaJumpingEnemyMainAi(Place);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => NorfairLavaJumpingEnemyBehavior.AdvanceNorfairLavaJumpingEnemyTypedAnimation(Place);
}

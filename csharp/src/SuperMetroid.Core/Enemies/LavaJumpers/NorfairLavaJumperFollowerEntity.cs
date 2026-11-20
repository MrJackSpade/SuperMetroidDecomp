namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>
/// A Squeept's cosmetic follower (#2164), which shares the Squeept's definition: its placement names it the second half
/// of the jumping parent its room lists right before it, shown only while that parent rises.
/// </summary>
/// <param name="species">The Squeept species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the follower occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class NorfairLavaJumperFollowerEntity(SqueeptBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal NorfairLavaJumperFollowerState State { get; } = species.InitializeNorfairLavaJumperFollower(place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunNorfairLavaJumperFollowerMain(Place, State);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => SqueeptBehavior.AdvanceNorfairLavaJumperFollowerTypedAnimation(Place, State);
}

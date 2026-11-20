namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// A Holtz's flame follower (#2164), which shares the Rio's definition: its placement names it the follower
/// of the parent its room lists right before it, whose freeze, visibility and position it copies.
/// </summary>
/// <param name="species">The Holtz species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the follower occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class HoltzFollowerEntity(HoltzBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal HoltzFollowerState State { get; } = species.InitializeHoltzFollower(place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunHoltzFollowerMainAi(Place, State);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => HoltzBehavior.AdvanceHoltzFollowerTypedAnimation(Place, State);
}

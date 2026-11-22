namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// A Holtz's flame follower (#2164), which shares the Rio's definition: its placement names it the follower
/// of the parent its room lists right before it, whose freeze, visibility and position it copies.
/// </summary>
/// <param name="species">The Holtz species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the follower occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="state">What the enemy's setup built (#2554).</param>
internal sealed partial class HoltzFollowerEntity(HoltzBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, HoltzFollowerState state)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Holtz species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the follower occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="parentState">The state of the parent the room lists right before the follower, which offered it its creation (#2554).</param>
    /// <param name="parentPlace">The place of that parent.</param>
    internal HoltzFollowerEntity(HoltzBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        HoltzEnemyState parentState, RoomEnemyPlace parentPlace)
        : this(species, occupant, place, common, species.InitializeHoltzFollower(place, parentState, parentPlace))
    {
    }

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly HoltzFollowerState state = state;

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunHoltzFollowerMainAi(Place, state);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => HoltzBehavior.AdvanceHoltzFollowerTypedAnimation(Place, state);
}

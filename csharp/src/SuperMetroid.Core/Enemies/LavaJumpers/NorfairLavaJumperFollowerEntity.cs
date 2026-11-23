namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>
/// A Squeept's cosmetic follower (#2164), which shares the Squeept's definition: its placement names it the second half
/// of the jumping parent its room lists right before it, shown only while that parent rises.
/// </summary>
/// <param name="species">The Squeept species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the follower occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="state">What the enemy's setup built (#2554).</param>
internal sealed partial class NorfairLavaJumperFollowerEntity(SqueeptBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, NorfairLavaJumperFollowerState state)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Squeept species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the follower occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="parentState">The state of the parent the room lists right before the follower, which offered it its creation (#2554).</param>
    /// <param name="parentPlace">The place of that parent.</param>
    internal NorfairLavaJumperFollowerEntity(SqueeptBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place,
        EnemyCommonParticipation common, SqueeptState parentState, RoomEnemyPlace parentPlace)
        : this(species, occupant, place, common, species.InitializeNorfairLavaJumperFollower(place, parentState, parentPlace))
    {
    }

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly NorfairLavaJumperFollowerState state = state;

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunNorfairLavaJumperFollowerMain(Place, state);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => SqueeptBehavior.AdvanceNorfairLavaJumperFollowerTypedAnimation(Place, state);
}

namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>
/// A Squeept's cosmetic follower (#2164), which shares the Squeept's definition: its placement names it the second half
/// of the jumping parent its room lists right before it, shown only while that parent rises.
/// </summary>
/// <param name="species">The Squeept species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the follower occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="parent">The parent the room lists right before the follower, which offered it its creation (#2554).</param>
internal sealed partial class NorfairLavaJumperFollowerEntity(SqueeptBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    SqueeptEntity parent)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The <c>parent</c> the object was made with, which it keeps so it can be made again from what it holds (#2602).</summary>
    private readonly SqueeptEntity parent = parent;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly NorfairLavaJumperFollowerState state = species.InitializeNorfairLavaJumperFollower(place, parent);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunNorfairLavaJumperFollowerMain(Place, state);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => SqueeptBehavior.AdvanceNorfairLavaJumperFollowerTypedAnimation(Place, state);
}

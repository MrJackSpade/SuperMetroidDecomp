using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// A wall of Ceres Ridley's getaway chamber, beside its turning floor (#1953). It only holds its place: the getaway scene
/// shows the chamber's walls (#1545), and its animation makes it ignore Samus.
/// </summary>
/// <param name="doors">The room's owner of the Ceres doors and walls.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the wall occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="state">What the enemy's setup built (#2554).</param>
internal sealed partial class CeresGetawayChamberWallEntity(CeresDoorBehavior doors, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, CeresDoorAnimationState state)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="doors">The room's owner of the Ceres doors and walls.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the wall occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="side">The animation the wall starts in, for the side of the chamber it stands on.</param>
    internal CeresGetawayChamberWallEntity(CeresDoorBehavior doors, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        CeresDoorSegment side)
        : this(doors, occupant, place, common, doors.Start(place, HorizontalDirection.Right, side))
    {
    }

    /// <summary>The wall's animation, which its setup starts as the wall is built (#2554).</summary>
    private readonly CeresDoorAnimationState state = state;

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame)
    {
        // The wall only holds its place.
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => doors.AdvanceAnimation(Place, state, context.Samus);
}

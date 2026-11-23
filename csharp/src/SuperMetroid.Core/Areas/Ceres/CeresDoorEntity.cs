using SuperMetroid.Core.Earthquakes;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// An ordinary Ceres door (#1953): it opens as Samus comes near and closes behind her, and keeps the station's escape
/// quake going through the foreground, the background and the enemies.
/// </summary>
/// <param name="doors">The room's owner of the Ceres doors and walls.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the door occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="state">What the enemy's setup built (#2554).</param>
internal sealed partial class CeresDoorEntity(CeresDoorBehavior doors, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, CeresDoorAnimationState state)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="doors">The room's owner of the Ceres doors and walls.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the door occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="facing">The way the door faces, choosing its pictures.</param>
    internal CeresDoorEntity(CeresDoorBehavior doors, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        HorizontalDirection facing)
        : this(doors, occupant, place, common, doors.Start(place, facing, CeresDoorSegment.Door))
    {
    }

    /// <summary>The door's animation, which its setup starts as the door is built (#2554).</summary>
    private readonly CeresDoorAnimationState state = state;

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => doors.EscapeQuake.Keep(EarthquakeReach.RoomBlocksSceneryAndEnemies);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => doors.AdvanceAnimation(Place, state, context.Samus);
}

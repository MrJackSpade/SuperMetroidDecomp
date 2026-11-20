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
/// <param name="opening">The animation the door starts in, for the way it faces.</param>
internal sealed class CeresDoorEntity(CeresDoorBehavior doors, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, CeresDoorSegment opening)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The door's animation, which its setup starts as the door is built (#2554).</summary>
    internal CeresDoorAnimationState State { get; } = doors.Start(place, opening);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => doors.EscapeQuake.Keep(EarthquakeReach.RoomBlocksSceneryAndEnemies);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => doors.AdvanceAnimation(Place, CeresDoorBehavior.LookOnlyRidleyRoomDoorShows, State, context.Samus);
}

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
/// <param name="facing">The way the door faces, choosing its pictures.</param>
internal sealed partial class CeresDoorEntity(CeresDoorBehavior doors, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, HorizontalDirection facing)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The constructor's <c>facing</c>, kept so a debugger state rebuilds the object through it (#2602).</summary>
    private readonly HorizontalDirection facing = facing;

    /// <summary>The door's animation, which its setup starts as the door is built (#2554).</summary>
    private readonly CeresDoorAnimationState state = doors.Start(place, facing, CeresDoorSegment.Door);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => doors.EscapeQuake.Keep(EarthquakeReach.RoomBlocksSceneryAndEnemies);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => doors.AdvanceAnimation(Place, state, context.Samus);
}

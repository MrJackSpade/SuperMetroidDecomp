using SuperMetroid.Core.Earthquakes;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The door of Ceres Ridley's room (#1953): it shows its actor frames or, while its animation shows it so, its fixed
/// overlay picture (#2573), and keeps the station's escape quake going through the background and the enemies, leaving
/// the foreground still.
/// </summary>
/// <param name="doors">The room's owner of the Ceres doors and walls.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the door occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class CeresRidleyRoomDoorEntity(CeresDoorBehavior doors, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>
    /// The door's animation, which it starts as it is built (#2554), and through which the room's Ceres doors show its
    /// overlay while its look is the overlay (#2573).
    /// </summary>
    internal CeresDoorAnimationState State { get; } = doors.StartRidleyRoomDoor(place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => doors.EscapeQuake.Keep(EarthquakeReach.SceneryAndEnemies);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => doors.AdvanceAnimation(Place, State, context.Samus);

    /// <summary>
    /// How the door shows (#2573): its overlay picture from its animation's <see cref="CeresDoorCueKind.ShowOverlay"/>
    /// until its <see cref="CeresDoorCueKind.MakeVisibleAndShowActor"/>, otherwise its actor frames.
    /// </summary>
    public CeresRidleyRoomDoorLook Look => State.Look;
}

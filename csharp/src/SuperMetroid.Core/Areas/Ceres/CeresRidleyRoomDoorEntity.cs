using SuperMetroid.Core.Earthquakes;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The door of Ceres Ridley's room (#1953): it attaches itself to Ridley's scene, which draws it while the door's
/// animation hands its drawing over, and keeps the station's escape quake going through the background and the enemies,
/// leaving the foreground still.
/// </summary>
/// <param name="doors">The room's owner of the Ceres doors and walls.</param>
/// <param name="place">The place the door occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class CeresRidleyRoomDoorEntity(CeresDoorBehavior doors, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation, IDrawnByCeresRidleyScene
{
    /// <inheritdoc/>
    public void Initialize()
    {
        doors.Start(Place, CeresDoorSegment.RidleyRoomDoor);
        Place.SetSpeciesState(new CeresRidleyRoomDoorState());
        // Ridley's scene draws this door while the door hands its drawing to him (#1877).
        doors.RidleyScene.AttachRoomDoor(Place);
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => doors.EscapeQuake.Keep(EarthquakeReach.BackgroundAndEnemies);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => doors.AdvanceAnimation(Place, context.Samus);

    /// <summary>
    /// Whether the door's animation has handed its drawing to Ridley's scene (<see cref="CeresDoorCueKind.HandDrawingToRidley"/>)
    /// and not yet taken it back (<see cref="CeresDoorCueKind.MakeVisibleAndTakeDrawingBack"/>).
    /// </summary>
    public bool HandsDrawingToRidley => Place.RequireSpeciesState<CeresRidleyRoomDoorState>().HandsDrawingToRidley;
}

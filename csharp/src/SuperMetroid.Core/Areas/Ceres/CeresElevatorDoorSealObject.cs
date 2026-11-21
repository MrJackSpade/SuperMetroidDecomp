using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The room object that seals the Ceres elevator room's door during the escape (#2494). Its setup has sealed the door
/// when it spawns; it holds its place until its first update and leaves the room then, doing nothing else.
/// </summary>
internal sealed partial class CeresElevatorDoorSealObject : RoomObjectBehaviour
{
    /// <inheritdoc/>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context) =>
        context.RoomObjects.DeleteObject(Ensure.NotNull(place));
}

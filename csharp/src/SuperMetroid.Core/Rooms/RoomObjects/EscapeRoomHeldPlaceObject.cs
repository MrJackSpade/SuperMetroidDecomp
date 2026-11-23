namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The room object an escape room's population places only to hold one of the room's places (#2426). It stays in the
/// room for good and does nothing on any update: its only effect on the game is the place it holds, so the room fills one
/// spawn sooner (<see cref="RoomObjectPlaces"/>).
/// </summary>
internal sealed partial class EscapeRoomHeldPlaceObject : RoomObjectBehaviour
{
    /// <inheritdoc/>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context) => Ensure.NotNull(place);
}

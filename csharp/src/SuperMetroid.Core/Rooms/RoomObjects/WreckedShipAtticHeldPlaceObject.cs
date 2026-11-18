namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The room object the Wrecked Ship attic's population places only to hold one of the room's places (#1554, #2455). It
/// stays in the room for good and does nothing on any update: its only effect on the game is the place it holds, so the
/// attic fills one spawn sooner (<see cref="RoomObjectPlaces"/>).
/// </summary>
internal sealed class WreckedShipAtticHeldPlaceObject : RoomObjectBehaviour
{
    /// <inheritdoc/>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context) => Ensure.NotNull(place);
}

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The room object a coloured door placed off its room holds its place with (#2572). Nothing in the room reaches a block
/// off its grid, so the door's terrain, its shots and its opening are never seen; what the game observes is the place it
/// holds in the room's object order, which sets where the room's later objects update and how soon the room fills
/// (<see cref="RoomObjectPlaces"/>). It stays in the room for good and does nothing on any update.
/// </summary>
internal sealed class OffRoomDoorHeldPlaceObject : RoomObjectBehaviour
{
    /// <inheritdoc/>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context) => Ensure.NotNull(place);
}

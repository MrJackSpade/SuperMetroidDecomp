using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>
/// The Wrecked Ship attic's room object (#1554). It stays in the room for good and does nothing on any update: its only
/// effect on the game is the place it holds in the room's update order, which a full room's spawns observe.
/// </summary>
internal sealed class WreckedShipAtticObject : RoomObjectBehaviour
{
    /// <inheritdoc/>
    internal override void Update(RoomObjectSlot place, RoomObjectStepContext context) => Ensure.NotNull(place);
}

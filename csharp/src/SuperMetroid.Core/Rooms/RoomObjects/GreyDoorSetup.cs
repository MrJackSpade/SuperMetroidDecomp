using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A grey door's setup (#2481), the Bomb Torizo's among them: the cap it places, opened by its placement's condition.</summary>
/// <param name="Door">The facing and closing of the grey cap the placement places.</param>
/// <param name="Opening">The condition that opens the door and the door it locks.</param>
internal sealed record GreyDoorSetup(GreyDoorVariant Door, GreyDoorArgument Opening) : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups) => setups.SetUpGreyDoor(Door, Opening);
}

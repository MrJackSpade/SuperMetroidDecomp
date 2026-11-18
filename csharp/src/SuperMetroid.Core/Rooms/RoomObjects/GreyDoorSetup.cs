namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A grey door's setup, the Bomb Torizo's among them, opened by its placement's condition.</summary>
/// <param name="Opening">The condition that opens the door and the door it locks, or null for none.</param>
internal sealed record GreyDoorSetup(GreyDoorArgument? Opening) : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups, RoomObjectKind kind) => setups.SetUpGreyDoor(kind, Opening);
}

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>An eye door's part's setup, locked by its placement's door, or by none.</summary>
/// <param name="LockedBy">The door the part locks, or null for none.</param>
internal sealed record EyeDoorSetup(LockedDoorArgument? LockedBy) : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups, RoomObjectKind kind) => setups.SetUpEyeDoor(kind, LockedBy);
}

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A coloured door's setup: its cap, locked by its placement's door, or by none.</summary>
/// <param name="LockedBy">The door the cap locks, or null for a cap that locks none.</param>
internal sealed record ColoredDoorSetup(LockedDoorArgument? LockedBy) : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups, RoomObjectKind kind) => setups.SetUpColoredDoor(kind, LockedBy);
}

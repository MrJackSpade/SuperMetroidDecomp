using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A coloured door's setup (#2481): the cap it places, locked by its placement's door, or by none.</summary>
/// <param name="Door">The colour and facing of the cap the placement places.</param>
/// <param name="LockedBy">The door the cap locks, or null for a cap that locks none.</param>
internal sealed record ColoredDoorSetup(ColoredDoorVariant Door, LockedDoorArgument? LockedBy) : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups) => setups.SetUpColoredDoor(Door, LockedBy);
}

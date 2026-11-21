using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>An eye door's body's or bottom's setup (#2481): which part it is, its door's facing, and the door it locks.</summary>
/// <param name="Part">The part the placement places and the way its door faces.</param>
/// <param name="LockedBy">The door the part locks.</param>
internal sealed record EyeDoorPartSetup(EyeDoorPart Part, DoorLock LockedBy) : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override RoomObjectKind Kind => RoomObjectKindDefinitions.EyeDoorParts[Part];

    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups) => setups.SetUpEyeDoorPart(Part, LockedBy);
}

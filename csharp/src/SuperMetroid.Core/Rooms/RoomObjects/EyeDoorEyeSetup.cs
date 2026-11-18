using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>An eye door's eye's setup (#2481): the way its door faces and the door it locks, which every eye names.</summary>
/// <param name="Orientation">The way the eye's door faces.</param>
/// <param name="LockedBy">The door the eye locks.</param>
internal sealed record EyeDoorEyeSetup(EyeDoorOrientation Orientation, LockedDoorArgument LockedBy) : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups) => setups.SetUpEyeDoorEye(Orientation, LockedBy);
}

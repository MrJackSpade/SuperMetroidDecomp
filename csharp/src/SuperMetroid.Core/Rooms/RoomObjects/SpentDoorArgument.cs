using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A cap that has been opened for good: its door is recorded as opened, and the cap keeps which doors were open at the
/// opening. It is never checked again.
/// </summary>
public sealed record SpentDoorArgument : RoomObjectRoomArgument
{
    /// <summary>The door the cap stood for.</summary>
    internal readonly LockedDoor Door;

    /// <summary>The doors open at the opening, this one included, in the order room data lists doors.</summary>
    internal readonly IReadOnlyList<LockedDoor> OpenedDoors;

    /// <summary>Keeps the doors open as the opening left them.</summary>
    /// <param name="door">The door the cap stood for.</param>
    /// <param name="openedDoors">The doors open at the opening.</param>
    internal SpentDoorArgument(LockedDoor door, IReadOnlyList<LockedDoor> openedDoors)
    {
        Door = Ensure.NotNull(door);
        OpenedDoors = Ensure.NotNull(openedDoors);
    }
}

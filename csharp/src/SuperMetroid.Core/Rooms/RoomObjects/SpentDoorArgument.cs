using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A cap that has been opened for good: its door is recorded as opened, and the cap is never checked again.</summary>
public sealed record SpentDoorArgument : RoomObjectRoomArgument
{
    /// <summary>The door the cap stood for.</summary>
    internal readonly LockedDoor Door;

    /// <summary>Marks the cap of <paramref name="door"/> spent.</summary>
    /// <param name="door">The door the cap stood for.</param>
    internal SpentDoorArgument(LockedDoor door) => Door = Ensure.NotNull(door);
}

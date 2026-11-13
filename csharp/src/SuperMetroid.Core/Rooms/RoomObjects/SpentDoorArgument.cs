namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A cap that has been opened for good: its door is recorded as opened, and the cap is never checked again.</summary>
public sealed record SpentDoorArgument : RoomObjectRoomArgument
{
    /// <summary>The argument of every spent cap.</summary>
    internal static SpentDoorArgument Instance { get; } = new();

    /// <summary>Only <see cref="Instance"/> exists.</summary>
    private SpentDoorArgument()
    {
    }
}

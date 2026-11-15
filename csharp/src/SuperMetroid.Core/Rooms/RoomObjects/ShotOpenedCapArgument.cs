namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The argument of a blue cap a shot opened: it names no door of its own, and whether it closes behind Samus is the room's
/// <see cref="Compatibility.ShotOpenedCapClosing{TDoor}"/> rule (#1906).
/// </summary>
public sealed record ShotOpenedCapArgument : RoomObjectRoomArgument
{
    /// <summary>The argument of every shot-opened cap.</summary>
    internal static ShotOpenedCapArgument Instance { get; } = new();

    /// <summary>Only <see cref="Instance"/> exists.</summary>
    private ShotOpenedCapArgument()
    {
    }
}

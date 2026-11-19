using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The published artwork name of each elevator platform look (<see cref="ElevatorPlatformDraw"/>, #1994, #2548): the name the installed
/// room-object visual file keys its picture by, written out so renaming a member never changes it.
/// </summary>
internal static class ElevatorPlatformDrawKeys
{
    /// <summary>Each ElevatorPlatformDraw member's installed key.</summary>
    internal static InstalledKeys<ElevatorPlatformDraw> Installed { get; } = new(new Dictionary<ElevatorPlatformDraw, string>
    {
        [ElevatorPlatformDraw.Dim] = "dim",
        [ElevatorPlatformDraw.Glowing] = "glowing",
        [ElevatorPlatformDraw.Bright] = "bright",
    });
}

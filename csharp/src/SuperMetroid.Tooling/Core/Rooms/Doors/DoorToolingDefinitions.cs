namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Development-tool members of <see cref="DoorDefinitions"/>; never linked by player hosts.</summary>
internal static class DoorToolingDefinitions
{
    /// <summary>The <c>UNUSED_Door_83A804</c> door.</summary>
    public static CartridgeDoorHeader UnusedRoom83A804 { get; } = DoorDefinitions.Find(nameof(UnusedRoom83A804));
    /// <summary>The <c>UNUSED_Door_83A810</c> door.</summary>
    public static CartridgeDoorHeader UnusedRoom83A810 { get; } = DoorDefinitions.Find(nameof(UnusedRoom83A810));
}

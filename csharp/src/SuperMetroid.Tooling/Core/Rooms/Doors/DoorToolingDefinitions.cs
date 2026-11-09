namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Development-tool members of <see cref="DoorDefinitions"/>; never linked by player hosts.</summary>
internal static class DoorToolingDefinitions
{
    /// <summary>The <c>UNUSED_Door_83A804</c> door.</summary>
    public static DoorDefinition UnlistedDoorToHalfieClimb { get; } = DoorDefinitions.Find(nameof(UnlistedDoorToHalfieClimb));
    /// <summary>The <c>UNUSED_Door_83A810</c> door.</summary>
    public static DoorDefinition UnlistedDoorToMaridiaMissileRefill { get; } = DoorDefinitions.Find(nameof(UnlistedDoorToMaridiaMissileRefill));
}

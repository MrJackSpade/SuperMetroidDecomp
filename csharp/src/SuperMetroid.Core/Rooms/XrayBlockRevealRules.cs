using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms;

/// <summary>Where X-ray's setup copies the room's blocks into its reveal: CanXrayShowBlocks ($91:D143).</summary>
public static class XrayBlockRevealRules
{
    /// <summary>
    /// Whether X-ray reveals <paramref name="room"/>'s blocks while <paramref name="boss"/> is loaded: not in the two
    /// rooms $91:D143 excludes, nor while a boss whose background X-ray preserves is loaded.
    /// </summary>
    public static bool RevealsBlocks(RoomHeaderDefinition room, AreaBoss? boss) =>
        !IsExcludedRoom(room) && !PreservesBossBackground(boss);

    /// <summary>True for the two rooms $91:D143 excludes; every other room is unaffected.</summary>
    private static bool IsExcludedRoom(RoomHeaderDefinition room) =>
        room == RoomHeaderDefinitions.Statues || room == RoomHeaderDefinitions.GlassTunnel;

    /// <summary>
    /// Native CheckIfXrayShouldShowAnyBlocks ($91:D158-D172): Kraid, Crocomire, Phantoon, Draygon and Mother Brain keep
    /// the room background under X-ray; every other boss, and no boss, reveals the blocks.
    /// </summary>
    private static bool PreservesBossBackground(AreaBoss? boss) => boss switch
    {
        null => false,
        AreaBoss.Kraid or AreaBoss.Crocomire or AreaBoss.Phantoon or AreaBoss.Draygon or AreaBoss.MotherBrain => true,
        AreaBoss.CeresRidley or AreaBoss.Torizo or AreaBoss.SporeSpawn or AreaBoss.Ridley or AreaBoss.Botwoon => false,
        _ => throw new ArgumentOutOfRangeException(nameof(boss), boss, "Undefined area boss."),
    };
}

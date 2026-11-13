using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms;

/// <summary>Where X-ray's setup copies the room's blocks into its reveal: whether X-ray can show a block.</summary>
public static class XrayBlockRevealRules
{
    /// <summary>
    /// Whether X-ray reveals <paramref name="room"/>'s blocks while <paramref name="boss"/> is loaded: not in the two
    /// rooms X-ray excludes, nor while a boss whose background X-ray preserves is loaded.
    /// </summary>
    public static bool RevealsBlocks(RoomDefinition room, AreaBoss? boss) =>
        !IsExcludedRoom(room) && !PreservesBossBackground(boss);

    /// <summary>True for the two rooms X-ray excludes; every other room is unaffected.</summary>
    private static bool IsExcludedRoom(RoomDefinition room) =>
        room == RoomDefinitions.Statues || room == RoomDefinitions.GlassTunnel;

    /// <summary>
    /// Kraid, Crocomire, Phantoon, Draygon and Mother Brain keep
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

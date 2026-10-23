namespace SuperMetroid.Core.Rooms;

/// <summary>Where X-ray's setup copies the room's blocks into its reveal: CanXrayShowBlocks ($91:D143).</summary>
public static class XrayBlockRevealRules
{
    /// <summary>
    /// Whether X-ray reveals <paramref name="room"/>'s blocks while the boss <paramref name="bossId"/> is loaded: not in
    /// the two rooms $91:D143 excludes, nor while a boss whose background X-ray preserves is loaded.
    /// </summary>
    public static bool RevealsBlocks(RoomHeaderDefinition room, int bossId) =>
        !IsExcludedRoom(room) && !PreservesBossBackground(bossId);

    /// <summary>True for the two rooms $91:D143 excludes; every other room is unaffected.</summary>
    private static bool IsExcludedRoom(RoomHeaderDefinition room) =>
        room == RoomHeaderDefinitions.Statues || room == RoomHeaderDefinitions.GlassTunnel;

    /// <summary>
    /// Native CheckIfXrayShouldShowAnyBlocks ($91:D158-D172): five equality
    /// branches on the active enemy-header boss identity preserve the room background.
    /// These are control-flow cases, not an indexed sequence or numerical progression.
    /// </summary>
    private static bool PreservesBossBackground(int bossId) => bossId is 3 or 6 or 7 or 8 or 10;
}

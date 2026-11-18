using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms;

/// <summary>Where X-ray's setup copies the room's blocks into its reveal: whether X-ray can show a block.</summary>
public static class XrayBlockRevealRules
{
    /// <summary>
    /// Whether X-ray reveals <paramref name="room"/>'s blocks while <paramref name="boss"/> is loaded: not in the rooms
    /// X-ray excludes, nor while a boss whose background X-ray preserves is loaded (<see cref="XrayBlockRevealDefinitions"/>).
    /// </summary>
    public static bool RevealsBlocks(RoomDefinition room, AreaBoss? boss) =>
        !XrayBlockRevealDefinitions.ExcludedRooms.Contains(Ensure.NotNull(room)) &&
        !(boss is { } loaded && XrayBlockRevealDefinitions.BossesPreservingBackground.Contains(loaded));
}

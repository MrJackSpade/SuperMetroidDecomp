using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms;

/// <summary>Where X-ray's setup copies the room's blocks into its reveal: whether X-ray can show a block.</summary>
public static class XrayBlockRevealRules
{
    /// <summary>
    /// Whether X-ray reveals <paramref name="room"/>'s blocks while <paramref name="boss"/> is loaded: not in a room that
    /// X-ray reveals no blocks in (<see cref="RoomDefinition.XrayRevealsNoBlocks"/>), nor while a boss whose fight keeps
    /// the room's background is loaded (<see cref="AreaBossDefinition.XrayKeepsBackground"/>).
    /// </summary>
    public static bool RevealsBlocks(RoomDefinition room, AreaBoss? boss) =>
        !Ensure.NotNull(room).XrayRevealsNoBlocks &&
        !(boss is { } loaded && Ensure.Listed(AreaBossDefinitions.Bosses, loaded).XrayKeepsBackground);
}

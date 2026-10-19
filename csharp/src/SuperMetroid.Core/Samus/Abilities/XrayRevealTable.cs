using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Exposes the compiled two-stage block-type/BTS lookup authored at $91:CDD6.</summary>
public static class XrayRevealTable
{
    /// <summary>Returns null when the cartridge leaves this block's copied BG1 art unchanged.</summary>
    public static XrayRevealDefinition? Find(RoomCollisionType type, int bts) =>
        XrayRevealDefinitions.Find(type, bts);
}

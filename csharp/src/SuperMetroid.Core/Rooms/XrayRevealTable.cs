namespace SuperMetroid.Core.Rooms;

/// <summary>Exposes the compiled two-stage block-type/BTS lookup authored at $91:CDD6.</summary>
public static class XrayRevealTable
{
    /// <summary>Returns null when the cartridge leaves this block's copied BG1 art unchanged.</summary>
    public static XrayRevealDefinition? Find(RoomCollisionType type, byte bts) =>
        XrayRevealDefinitions.Find(type, bts);
}

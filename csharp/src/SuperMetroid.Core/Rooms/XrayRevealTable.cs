namespace SuperMetroid.Core.Rooms;

/// <summary>Owned native reveal command and its row-major metatile arguments; extensions have no arguments.</summary>
public readonly record struct XrayRevealDefinition(ushort Command, ushort TopLeft,
    ushort TopRight, ushort BottomLeft, ushort BottomRight);

/// <summary>Exposes the compiled two-stage block-type/BTS lookup authored at $91:CDD6.</summary>
public static class XrayRevealTable
{
    /// <summary>Returns null when the cartridge leaves this block's copied BG1 art unchanged.</summary>
    public static XrayRevealDefinition? Find(RoomCollisionType type, byte bts) =>
        XrayRevealDefinitions.Find(type, bts);
}

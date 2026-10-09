namespace SuperMetroid.Core.Rooms;

/// <summary>Owned native reveal command and its row-major metatile arguments; extensions have no arguments.</summary>
/// <param name="Command">Native reveal command pointer that selects how the metatile arguments are copied or how an extension is handled.</param>
/// <param name="TopLeft">Metatile value supplied for the upper-left cell of the reveal footprint; unused cells are zero.</param>
/// <param name="TopRight">Metatile value supplied for the upper-right cell of a wide or square reveal; otherwise zero.</param>
/// <param name="BottomLeft">Metatile value supplied for the lower-left cell of a tall or square reveal; otherwise zero.</param>
/// <param name="BottomRight">Metatile value supplied for the lower-right cell of a square reveal; otherwise zero.</param>
public readonly record struct XrayRevealDefinition(ushort Command, ushort TopLeft,
    ushort TopRight, ushort BottomLeft, ushort BottomRight);

/// <summary>Exposes the compiled two-stage block-type/BTS lookup authored at $91:CDD6.</summary>
public static class XrayRevealTable
{
    /// <summary>Returns null when the cartridge leaves this block's copied BG1 art unchanged.</summary>
    public static XrayRevealDefinition? Find(RoomCollisionType type, byte bts) =>
        XrayRevealDefinitions.Find(type, bts);
}

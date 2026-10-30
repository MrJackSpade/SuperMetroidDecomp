namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The cartridge's room level space, which the import reads past each room's grid (#1469).</summary>
internal static class RoomLevelSpaceDefinitions
{
    /// <summary>
    /// The level word <c>$82:EA73</c> fills the room's whole level space with before the room is decompressed over its
    /// beginning: block zero, unmirrored, solid.
    /// </summary>
    internal const ushort PrefillWord = 0x8000;

    /// <summary>The level words that fill covers: byte offsets $0000 through $63FE of level space.</summary>
    internal const int PrefilledWordCount = 0x3200;
}

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The four dynamic collectible graphics slots that instruction <c>$84:8764</c> loads and points blocks at.</summary>
internal static class CollectibleDynamicGraphicsDefinitions
{
    /// <summary>The first BG character of the four eight-character slots.</summary>
    internal const int FirstTileNumber = 0x03e0;
    /// <summary>The first TileTable word of the four slots' block definitions (byte offset $0470).</summary>
    internal const int FirstBlockDefinitionWord = 0x0470 / 2;
}

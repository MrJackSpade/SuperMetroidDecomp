namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// Typed view of a room's graphics in gameplay VRAM and CGRAM (#630). Its characters are bound as background artwork
/// (#1350); only the collectible slot size and the tileset palette remain here.
/// </summary>
[PpuLayoutBoundary("rendering view: room tileset graphics")]
public static class RoomGraphicsVram
{
    /// <summary>Words in one collectible character slot: two 16x16 frames of four characters.</summary>
    public const int CollectibleSlotWordCount = 128;

    /// <summary>The tileset palette in <paramref name="cgram"/>.</summary>
    public static CgramPalette TilesetPalette(IPaletteColors cgram) => new(cgram, RoomGraphicsLayout.TilesetPalette);
}

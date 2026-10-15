namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Typed view of a room's graphics in gameplay VRAM and CGRAM (#630).</summary>
[PpuLayoutBoundary("rendering view: room tileset graphics")]
public readonly struct RoomGraphicsVram(SnesVram vram)
{
    /// <summary>Words in one collectible character slot: two 16x16 frames of four characters.</summary>
    public const int CollectibleSlotWordCount = 0x80;

    /// <summary>The area tileset's characters.</summary>
    public VramRegion AreaCharacters => new(vram, RoomGraphicsLayout.AreaCharacters);

    /// <summary>The common-room-element characters.</summary>
    public VramRegion CommonRoomElementCharacters => new(vram, RoomGraphicsLayout.CommonRoomElementCharacters);

    /// <summary>Collectible character slot <paramref name="slot"/> (0-3).</summary>
    public VramRegion CollectibleSlot(int slot) =>
        new VramRegion(vram, RoomGraphicsLayout.CollectibleCharacters).Slice(slot * CollectibleSlotWordCount, CollectibleSlotWordCount);

    /// <summary>The tileset palette in <paramref name="cgram"/>.</summary>
    public static CgramPalette TilesetPalette(SnesCgram cgram) => new(cgram, RoomGraphicsLayout.TilesetPalette);
}

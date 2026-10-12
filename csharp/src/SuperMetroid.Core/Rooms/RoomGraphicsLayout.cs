using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>Where a room's tileset, common-room-element and collectible graphics live in gameplay VRAM and CGRAM (#630).</summary>
[PpuLayoutBoundary("catalog: room tileset graphics")]
public static class RoomGraphicsLayout
{
    /// <summary>
    /// VMADD $0000-$3FFF: the area tileset's BG characters uploaded first by <c>$82:E783</c>; ordinary
    /// areas fill $0000-$27FF, Ceres tileset $11 the whole BG character allocation.
    /// </summary>
    public static readonly PpuLayoutCell AreaCharacters = new(nameof(AreaCharacters), PpuLayoutRegion.Vram,
        0x0000, 0x4000, "4-bpp BG characters", GameplayVramLayout.BackgroundCharacters);

    /// <summary>VMADD $2800-$3FFF: the common-room-element (CRE) BG characters uploaded before the area tileset.</summary>
    public static readonly PpuLayoutCell CommonRoomElementCharacters = new(nameof(CommonRoomElementCharacters), PpuLayoutRegion.Vram,
        0x2800, 0x1800, "4-bpp BG characters", GameplayVramLayout.BackgroundCharacters);

    /// <summary>
    /// VMADD $3E00-$3FFF: the four $80-word character slots instruction <c>$84:8764</c> rotates
    /// through for animated collectible PLMs (characters $3E0-$3FF).
    /// </summary>
    public static readonly PpuLayoutCell CollectibleCharacters = new(nameof(CollectibleCharacters), PpuLayoutRegion.Vram,
        0x3e00, 0x0200, "4-bpp BG characters, four 8-character slots", GameplayVramLayout.BackgroundCharacters);

    /// <summary>CGADD $00-$7F: the tileset palette copied by <c>LoadCRETilesTilesetTilesAndPalette</c>.</summary>
    public static readonly PpuLayoutCell TilesetPalette = PpuMemoryLayout.Colors(nameof(TilesetPalette), 0, 0x80,
        "Bgr555 BG palettes (CGDATA words)");
}

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

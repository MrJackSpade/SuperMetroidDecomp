namespace SuperMetroid.Core.Rooms.LevelData;

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
}

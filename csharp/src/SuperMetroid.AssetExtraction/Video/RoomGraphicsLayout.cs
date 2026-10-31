using SuperMetroid.Core;


namespace SuperMetroid.AssetExtraction.Video;

/// <summary>Where a room's tileset, common-room-element and collectible graphics live in gameplay VRAM and CGRAM (#630).</summary>
[EncodingOwner("room graphics VRAM and colour allocations")]
internal static class RoomGraphicsLayout
{
    /// <summary>
    /// VMADD $0000-$3FFF: the area tileset's BG characters uploaded first by <c>$82:E783</c>; ordinary
    /// areas fill $0000-$27FF, Ceres tileset $11 the whole BG character allocation.
    /// </summary>
    public static readonly PpuLayoutCell AreaCharacters = new(nameof(AreaCharacters), PpuLayoutRegion.Vram,
        0x0000, 0x4000, "4-bpp BG characters", GameplayVramLayout.BackgroundCharacters);
}

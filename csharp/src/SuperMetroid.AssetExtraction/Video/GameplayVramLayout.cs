using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Video;

/// <summary>
/// Ordinary gameplay's VRAM allocation (#630), as programmed by the room-load PPU setup
/// (BG1SC=$51, BG2SC=$49, BG3SC=$5A, BG12NBA=$00, BG34NBA=$04, OBSEL=$03). Room, enemy and
/// effect catalogs declare their own uses of these planes as aliases of the cells here.
/// </summary>
[PpuLayoutBoundary("catalog: ordinary gameplay VRAM")]
internal static class GameplayVramLayout
{
    /// <summary>VMADD of <c>Bg1Tilemap</c>, for render packets and the PPU backend.</summary>
    [PpuLayoutAccess]
    public const ushort Bg1TilemapWord = 0x5000;

    /// <summary>VMADD of <c>Bg2Tilemap</c>, for render packets and the PPU backend.</summary>
    [PpuLayoutAccess]
    public const ushort Bg2TilemapWord = 0x4800;

    /// <summary>VMADD of <see cref="HudCharacters"/>, for render packets and the PPU backend.</summary>
    [PpuLayoutAccess]
    public const ushort HudCharacterWord = 0x4000;

    /// <summary>VMADD $0000-$3FFF: the BG1/BG2 4-bpp character base (BG12NBA=$00); room tilesets load here.</summary>
    public static readonly PpuLayoutCell BackgroundCharacters = new(nameof(BackgroundCharacters), PpuLayoutRegion.Vram,
        0x0000, 0x4000, "4-bpp BG characters (16 words each)", PpuMemoryLayout.Vram);

    /// <summary>VMADD $4000-$47FF: the BG3 2-bpp character base (BG34NBA=$04) holding the HUD, minimap and layer-3 FX glyphs.</summary>
    public static readonly PpuLayoutCell HudCharacters = new(nameof(HudCharacters), PpuLayoutRegion.Vram,
        HudCharacterWord, 0x0800, "2-bpp BG3 characters (8 words each)", PpuMemoryLayout.Vram);
}

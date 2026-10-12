namespace SuperMetroid.Core.Hardware;

/// <summary>Shared physical PPU capacities and Super Metroid's stable screen layout.</summary>
/// <remarks>
/// This catalog intentionally contains only hardware sizes or engine-wide placements.
/// Dimensions belonging to one animation, temporary buffer, or algorithm remain beside
/// that code even when their numeric value happens to match one of these constants.
/// </remarks>
[PpuLayoutBoundary("catalog: PPU memory geometry and gameplay VRAM bases")]
public static class SnesPpuLayout
{
    /// <summary>INIDISP ($2100) brightness nibble: fifteen is full intensity.</summary>
    public const byte MaximumMasterBrightness = 15;

    /// <summary>Width of the SNES low-resolution raster used by the game.</summary>
    public const int ScreenWidthPixels = 256;

    /// <summary>NTSC visible height emitted by Super Metroid.</summary>
    public const int ScreenHeightPixels = 224;

    /// <summary>The visible low-resolution raster: <see cref="ScreenWidthPixels"/> by <see cref="ScreenHeightPixels"/>.</summary>
    public static Extent16<ScreenPixels> ScreenSize => new(ScreenWidthPixels, ScreenHeightPixels);

    /// <summary>
    /// The first stored NTSC output row is physical PPU scanline one. Background
    /// sampling uses that physical line; OBJ evaluation uses the preceding line.
    /// Verified against the independent PPU with #516's captured frame memory.
    /// </summary>
    public const int FirstVisibleBackgroundScanline = 1;

    /// <summary>Physical scanlines reserved for the gameplay BG3 HUD.</summary>
    public const int GameplayHudHeightPixels = 32;

    /// <summary>Pixels on either axis of one SNES background character.</summary>
    public const int BackgroundTileSizePixels = 8;

    /// <summary>Characters on either axis of one BGSC tilemap page.</summary>
    public const int TilemapPageWidthInTiles = 32;

    /// <summary>Words in one 32-by-32 BGSC tilemap page.</summary>
    public const int TilemapPageWordCount =
        TilemapPageWidthInTiles * TilemapPageWidthInTiles;

    /// <summary>Physical 15-bit VMADD capacity.</summary>
    [PpuLayoutAccess]
    public const int VramWordCount = 0x8000;

    /// <summary>Physical 64-KiB VRAM capacity.</summary>
    [PpuLayoutAccess]
    public const int VramByteCount = VramWordCount * 2;

    /// <summary>Number of native BGR555 entries in CGRAM.</summary>
    [PpuLayoutAccess]
    public const int CgramColorCount = 256;

    /// <summary>Size of the complete CGRAM image uploaded by NMI.</summary>
    [PpuLayoutAccess]
    public const int CgramByteCount = CgramColorCount * 2;

    /// <summary>Number of hardware OBJ records.</summary>
    [PpuLayoutAccess]
    public const int OamSpriteCount = 128;

    /// <summary>Size of OAM's four-byte-per-OBJ low table.</summary>
    [PpuLayoutAccess]
    public const int OamLowTableByteCount = OamSpriteCount * 4;

    /// <summary>Size of OAM's two-bits-per-OBJ high table.</summary>
    [PpuLayoutAccess]
    public const int OamHighTableByteCount = OamSpriteCount / 4;

    /// <summary>Complete low-plus-high OAM DMA payload.</summary>
    [PpuLayoutAccess]
    public const int OamUploadByteCount = OamLowTableByteCount + OamHighTableByteCount;
}

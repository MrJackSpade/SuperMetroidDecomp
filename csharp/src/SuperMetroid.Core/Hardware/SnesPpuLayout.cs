namespace SuperMetroid.Core.Hardware;

/// <summary>Shared physical PPU capacities and Super Metroid's stable screen layout.</summary>
/// <remarks>
/// This catalog intentionally contains only hardware sizes or engine-wide placements.
/// Dimensions belonging to one animation, temporary buffer, or algorithm remain beside
/// that code even when their numeric value happens to match one of these constants.
/// </remarks>
public static class SnesPpuLayout
{
    /// <summary>Width of the SNES low-resolution raster used by the game.</summary>
    public const int ScreenWidthPixels = 256;

    /// <summary>NTSC visible height emitted by Super Metroid.</summary>
    public const int ScreenHeightPixels = 224;

    /// <summary>Physical scanlines reserved for the gameplay BG3 HUD.</summary>
    public const int GameplayHudHeightPixels = 32;

    /// <summary>Room scanlines below the HUD.</summary>
    public const int GameplayViewportHeightPixels =
        ScreenHeightPixels - GameplayHudHeightPixels;

    /// <summary>Pixels on either axis of one SNES background character.</summary>
    public const int BackgroundTileSizePixels = 8;

    /// <summary>Characters on either axis of one BGSC tilemap page.</summary>
    public const int TilemapPageWidthInTiles = 32;

    /// <summary>Words in one 32-by-32 BGSC tilemap page.</summary>
    public const int TilemapPageWordCount =
        TilemapPageWidthInTiles * TilemapPageWidthInTiles;

    /// <summary>Bytes in one two-byte-per-entry BGSC tilemap page.</summary>
    public const int TilemapPageByteCount = TilemapPageWordCount * 2;

    /// <summary>Physical 15-bit VMADD capacity.</summary>
    public const int VramWordCount = 0x8000;

    /// <summary>Physical 64-KiB VRAM capacity.</summary>
    public const int VramByteCount = VramWordCount * 2;

    /// <summary>Number of native BGR555 entries in CGRAM.</summary>
    public const int CgramColorCount = 256;

    /// <summary>Size of the complete CGRAM image uploaded by NMI.</summary>
    public const int CgramByteCount = CgramColorCount * 2;

    /// <summary>Number of hardware OBJ records.</summary>
    public const int OamSpriteCount = 128;

    /// <summary>Size of OAM's four-byte-per-OBJ low table.</summary>
    public const int OamLowTableByteCount = OamSpriteCount * 4;

    /// <summary>Size of OAM's two-bits-per-OBJ high table.</summary>
    public const int OamHighTableByteCount = OamSpriteCount / 4;

    /// <summary>Complete low-plus-high OAM DMA payload.</summary>
    public const int OamUploadByteCount = OamLowTableByteCount + OamHighTableByteCount;

    /// <summary>Ordinary gameplay BG1's first VMADD tilemap word.</summary>
    public const ushort GameplayBg1TilemapWord = 0x5000;

    /// <summary>Ordinary gameplay BG2's first VMADD tilemap word.</summary>
    public const ushort GameplayBg2TilemapWord = 0x4800;

    /// <summary>IRQ-visible gameplay HUD BG3's first VMADD tilemap word.</summary>
    public const ushort GameplayHudTilemapWord = 0x5800;

    /// <summary>Ordinary gameplay BG3 character base selected by BG34NBA=$04.</summary>
    public const ushort GameplayHudCharacterBaseWord = 0x4000;

    /// <summary>
    /// Gameplay BG3's 32-by-64 tilemap base selected by BG3SC <c>$5A</c>. The HUD uses
    /// rows zero through three; room FX use the cleared lower rows and vertical page.
    /// </summary>
    public const ushort RoomFxTilemapWord = 0x5800;

    /// <summary>Shared menu BG1 tilemap location.</summary>
    public const ushort MenuBg1TilemapWord = 0x5000;

    /// <summary>Shared menu BG2 tilemap location.</summary>
    public const ushort MenuBg2TilemapWord = 0x5800;
}

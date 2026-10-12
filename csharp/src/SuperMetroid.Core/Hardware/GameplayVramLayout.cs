namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Ordinary gameplay's VRAM allocation (#630), as programmed by the room-load PPU setup
/// (BG1SC=$51, BG2SC=$49, BG3SC=$5A, BG12NBA=$00, BG34NBA=$04, OBSEL=$03). Room, enemy and
/// effect catalogs declare their own uses of these planes as aliases of the cells here.
/// </summary>
[PpuLayoutBoundary("catalog: ordinary gameplay VRAM")]
public static class GameplayVramLayout
{
    /// <summary>VMADD of <see cref="Bg1Tilemap"/>, for render packets and the PPU backend.</summary>
    [PpuLayoutAccess]
    public const ushort Bg1TilemapWord = 0x5000;

    /// <summary>VMADD of <see cref="Bg2Tilemap"/>, for render packets and the PPU backend.</summary>
    [PpuLayoutAccess]
    public const ushort Bg2TilemapWord = 0x4800;

    /// <summary>VMADD of <see cref="HudTilemap"/>, for render packets and the PPU backend.</summary>
    [PpuLayoutAccess]
    public const ushort HudTilemapWord = 0x5800;

    /// <summary>VMADD of <see cref="HudCharacters"/>, for render packets and the PPU backend.</summary>
    [PpuLayoutAccess]
    public const ushort HudCharacterWord = 0x4000;

    /// <summary>VMADD $0000-$3FFF: the BG1/BG2 4-bpp character base (BG12NBA=$00); room tilesets load here.</summary>
    public static readonly PpuLayoutCell BackgroundCharacters = new(nameof(BackgroundCharacters), PpuLayoutRegion.Vram,
        0x0000, 0x4000, "4-bpp BG characters (16 words each)", PpuMemoryLayout.Vram);

    /// <summary>VMADD $4000-$47FF: the BG3 2-bpp character base (BG34NBA=$04) holding the HUD, minimap and layer-3 FX glyphs.</summary>
    public static readonly PpuLayoutCell HudCharacters = new(nameof(HudCharacters), PpuLayoutRegion.Vram,
        HudCharacterWord, 0x0800, "2-bpp BG3 characters (8 words each)", PpuMemoryLayout.Vram);

    /// <summary>VMADD $4800-$4FFF: BG2's two horizontal 32x32 tilemap screens (BG2SC=$49).</summary>
    public static readonly PpuLayoutCell Bg2Tilemap = new(nameof(Bg2Tilemap), PpuLayoutRegion.Vram,
        Bg2TilemapWord, 0x0800, "BG tilemap words (64x32)", PpuMemoryLayout.Vram);

    /// <summary>VMADD $5000-$57FF: BG1's two horizontal 32x32 tilemap screens (BG1SC=$51), the level ring.</summary>
    public static readonly PpuLayoutCell Bg1Tilemap = new(nameof(Bg1Tilemap), PpuLayoutRegion.Vram,
        Bg1TilemapWord, 0x0800, "BG tilemap words (64x32)", PpuMemoryLayout.Vram);

    /// <summary>VMADD $5800-$5FFF: BG3's tilemap (BG3SC=$5A): the four HUD rows followed by the layer-3 FX screen.</summary>
    public static readonly PpuLayoutCell HudTilemap = new(nameof(HudTilemap), PpuLayoutRegion.Vram,
        HudTilemapWord, 0x0800, "BG3 tilemap words (32x64)", PpuMemoryLayout.Vram);

    /// <summary>
    /// VMADD $4000-$4FFF: the destination of the standard BG3 transfer at <c>$82:8318</c>, whose
    /// second half clears the BG2 tilemap.
    /// </summary>
    public static readonly PpuLayoutCell HudCharacterUpload = new(nameof(HudCharacterUpload), PpuLayoutRegion.Vram,
        HudCharacterWord, 0x1000, "2-bpp BG3 characters then zeroed BG2 tilemap words", PpuMemoryLayout.Vram);

    /// <summary>VMADD $6000-$7FFF: the OBJ character table (OBSEL=$03), Samus, projectiles and enemies.</summary>
    public static readonly PpuLayoutCell ObjectCharacters = new(nameof(ObjectCharacters), PpuLayoutRegion.Vram,
        0x6000, 0x2000, "4-bpp OBJ characters (16 words each)", PpuMemoryLayout.Vram);
}

/// <summary>Typed view of ordinary gameplay's VRAM planes (#630).</summary>
[PpuLayoutBoundary("rendering view: ordinary gameplay VRAM")]
public readonly struct GameplayVram(SnesVram vram)
{
    /// <summary>BG1/BG2 4-bpp characters.</summary>
    public VramRegion BackgroundCharacters => new(vram, GameplayVramLayout.BackgroundCharacters);

    /// <summary>BG3 HUD/FX characters.</summary>
    public VramRegion HudCharacters => new(vram, GameplayVramLayout.HudCharacters);

    /// <summary>The standard BG3 upload: HUD characters followed by the cleared BG2 tilemap.</summary>
    public VramRegion HudCharacterUpload => new(vram, GameplayVramLayout.HudCharacterUpload);

    /// <summary>BG1's level tilemap.</summary>
    public VramRegion Bg1Tilemap => new(vram, GameplayVramLayout.Bg1Tilemap);

    /// <summary>BG2's background tilemap.</summary>
    public VramRegion Bg2Tilemap => new(vram, GameplayVramLayout.Bg2Tilemap);

    /// <summary>BG3's HUD and layer-3 FX tilemap.</summary>
    public VramRegion HudTilemap => new(vram, GameplayVramLayout.HudTilemap);

    /// <summary>OBJ characters.</summary>
    public VramRegion ObjectCharacters => new(vram, GameplayVramLayout.ObjectCharacters);

    /// <summary>The queued-DMA destination <paramref name="wordOffset"/> words into the BG3 HUD tilemap.</summary>
    public static VramDmaDestination HudTilemapDma(int wordOffset = 0) => VramDmaDestination.At(GameplayVramLayout.HudTilemap, wordOffset);

    /// <summary>The queued-DMA destination at the start of the OBJ character table, where the standard OBJ upload lands.</summary>
    public static VramDmaDestination ObjectCharactersDma() => VramDmaDestination.At(GameplayVramLayout.ObjectCharacters);
}

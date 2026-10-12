using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The shared menu VRAM allocation installed by <c>LoadInitialMenuTiles</c> ($81:8DDB) and used
/// by file select, options and game over (#630).
/// </summary>
[PpuLayoutBoundary("catalog: shared menu VRAM")]
public static class MenuVramLayout
{
    /// <summary>VMADD of <see cref="Bg1Tilemap"/>, for render packets and the PPU backend.</summary>
    [PpuLayoutAccess]
    public const ushort Bg1TilemapWord = 0x5000;

    /// <summary>VMADD of <see cref="Bg2Tilemap"/>, for render packets and the PPU backend.</summary>
    [PpuLayoutAccess]
    public const ushort Bg2TilemapWord = 0x5800;

    /// <summary>VMADD $5000-$53FF: the menu BG1 32x32 tilemap.</summary>
    public static readonly PpuLayoutCell Bg1Tilemap = new(nameof(Bg1Tilemap), PpuLayoutRegion.Vram,
        Bg1TilemapWord, SnesPpuLayout.TilemapPageWordCount, "BG tilemap words (32x32)", PpuMemoryLayout.Vram);

    /// <summary>VMADD $5000-$57FF: the file-select room map's 64x32 area-map tilemap, spilling past the menu BG1 page.</summary>
    public static readonly PpuLayoutCell AreaMapTilemap = new(nameof(AreaMapTilemap), PpuLayoutRegion.Vram,
        Bg1TilemapWord, 2 * SnesPpuLayout.TilemapPageWordCount, "BG tilemap words (64x32)", PpuMemoryLayout.Vram);

    /// <summary>VMADD $5800-$5BFF: the menu BG2 32x32 tilemap.</summary>
    public static readonly PpuLayoutCell Bg2Tilemap = new(nameof(Bg2Tilemap), PpuLayoutRegion.Vram,
        Bg2TilemapWord, SnesPpuLayout.TilemapPageWordCount, "BG tilemap words (32x32)", PpuMemoryLayout.Vram);
}

/// <summary>Typed view of the shared menu VRAM planes (#630).</summary>
[PpuLayoutBoundary("rendering view: shared menu VRAM")]
public readonly struct MenuVram(SnesVram vram)
{
    /// <summary>The menu BG1 tilemap.</summary>
    public VramRegion Bg1Tilemap => new(vram, MenuVramLayout.Bg1Tilemap);

    /// <summary>The file-select room map's area-map tilemap.</summary>
    public VramRegion AreaMapTilemap => new(vram, MenuVramLayout.AreaMapTilemap);

    /// <summary>The menu BG2 tilemap.</summary>
    public VramRegion Bg2Tilemap => new(vram, MenuVramLayout.Bg2Tilemap);
}

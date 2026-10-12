using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Fixed VRAM destinations of installed artwork uploads (#630).</summary>
[PpuLayoutBoundary("catalog: installed artwork VRAM destinations")]
public static class AssetVramLayout
{
    /// <summary>VMADD $6380-$63FF: the eight ice/wave trail characters inside the standard OBJ table (bytes $C700).</summary>
    public static readonly PpuLayoutCell ProjectileIceWaveTrail = new(nameof(ProjectileIceWaveTrail), PpuLayoutRegion.Vram,
        ProjectileTrailAtlasDefinitions.IceWaveDestinationWord, 0x80,
        "4-bpp OBJ characters", GameplayVramLayout.ObjectCharacters);

    /// <summary>VMADD $6480-$64BF: the four missile trail characters inside the standard OBJ table (bytes $C900).</summary>
    public static readonly PpuLayoutCell ProjectileMissileTrail = new(nameof(ProjectileMissileTrail), PpuLayoutRegion.Vram,
        ProjectileTrailAtlasDefinitions.MissileDestinationWord, 0x40,
        "4-bpp OBJ characters", GameplayVramLayout.ObjectCharacters);

    /// <summary>VMADD $7000-$71FF (bytes $E000): the 32 Ceres door characters replacing variant two's $B0:C400 DMA.</summary>
    public static readonly PpuLayoutCell CeresDoorTiles = new(nameof(CeresDoorTiles), PpuLayoutRegion.Vram,
        CeresDoorVisualRomData.TileVramDestination / sizeof(ushort), CeresDoorVisualRomData.TileByteCount / sizeof(ushort),
        "4-bpp OBJ characters", GameplayVramLayout.ObjectCharacters);

    /// <summary>VMADD $0000-$2AFF: the world map's 4-bpp foreground characters from $8E:8000.</summary>
    public static readonly PpuLayoutCell WorldMapForeground = new(nameof(WorldMapForeground), PpuLayoutRegion.Vram,
        WorldMapArtworkFormat.ForegroundDestination / sizeof(ushort), WorldMapArtworkFormat.ForegroundBytes / sizeof(ushort),
        "4-bpp BG characters", PpuMemoryLayout.Vram);

    /// <summary>VMADD $4000-$42FF (bytes $8000): the world map's 2-bpp background characters from $8E:D600.</summary>
    public static readonly PpuLayoutCell WorldMapBackground = new(nameof(WorldMapBackground), PpuLayoutRegion.Vram,
        WorldMapArtworkFormat.BackgroundDestination / sizeof(ushort), WorldMapArtworkFormat.BackgroundBytes / sizeof(ushort),
        "2-bpp BG characters", PpuMemoryLayout.Vram);
}

/// <summary>Typed view of the installed artwork VRAM destinations (#630).</summary>
[PpuLayoutBoundary("rendering view: installed artwork VRAM destinations")]
public readonly struct AssetVram(SnesVram vram)
{
    /// <summary>Ice/wave trail characters.</summary>
    public VramRegion ProjectileIceWaveTrail => new(vram, AssetVramLayout.ProjectileIceWaveTrail);
    /// <summary>Missile trail characters.</summary>
    public VramRegion ProjectileMissileTrail => new(vram, AssetVramLayout.ProjectileMissileTrail);
    /// <summary>Ceres door characters.</summary>
    public VramRegion CeresDoorTiles => new(vram, AssetVramLayout.CeresDoorTiles);
    /// <summary>World map foreground characters.</summary>
    public VramRegion WorldMapForeground => new(vram, AssetVramLayout.WorldMapForeground);
    /// <summary>World map background characters.</summary>
    public VramRegion WorldMapBackground => new(vram, AssetVramLayout.WorldMapBackground);

    /// <summary>Queued-DMA destination of the ice/wave trail characters.</summary>
    public static VramDmaDestination ProjectileIceWaveTrailDma => VramDmaDestination.At(AssetVramLayout.ProjectileIceWaveTrail);
    /// <summary>Queued-DMA destination of the missile trail characters.</summary>
    public static VramDmaDestination ProjectileMissileTrailDma => VramDmaDestination.At(AssetVramLayout.ProjectileMissileTrail);
    /// <summary>Queued-DMA destination of the standard BG3 upload (HUD characters then the cleared BG2 tilemap).</summary>
    public static VramDmaDestination HudCharacterUploadDma => VramDmaDestination.At(GameplayVramLayout.HudCharacterUpload);
}

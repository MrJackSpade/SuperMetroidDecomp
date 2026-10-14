namespace SuperMetroid.Core.Assets;

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

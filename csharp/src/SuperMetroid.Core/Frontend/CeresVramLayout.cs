using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The Ceres flight and destruction cinematics' Mode 1 VRAM allocations (#630); their Mode 7
/// gunship and station planes use <see cref="Mode7VramLayout"/>.
/// </summary>
[PpuLayoutBoundary("catalog: Ceres flight and destruction cinematic VRAM")]
public static class CeresVramLayout
{
    /// <summary>VMADD $6000-$7FFF: the space/gunship/Ceres OBJ characters (bytes $C000-$FFFF), later overlaid by the shared cinematic sheet.</summary>
    public static readonly PpuLayoutCell ObjectCharacters = new(nameof(ObjectCharacters), PpuLayoutRegion.Vram,
        0x6000, 0x2000, "4-bpp OBJ characters", PpuMemoryLayout.Vram);

    /// <summary>VMADD $5C00-$5FFF: the Mode 1 BG1 "SPACE COLONY" caption tilemap.</summary>
    public static readonly PpuLayoutCell SpaceColonyTilemap = new(nameof(SpaceColonyTilemap), PpuLayoutRegion.Vram,
        CeresFlightRomData.Layers.SpaceColonyTilemapWord, 0x0400, "BG tilemap words (32x32)", PpuMemoryLayout.Vram);

    /// <summary>VMADD $5C00-$5FFF (bytes $B800-$BFFF): the Zebes reveal's Mode 1 tilemap, reusing the caption plane.</summary>
    public static readonly PpuLayoutCell ZebesTilemap = new(nameof(ZebesTilemap), PpuLayoutRegion.Vram,
        0x5c00, 0x0400, "BG tilemap words (32x32)", SpaceColonyTilemap);
}

/// <summary>Typed view of the Ceres cinematics' Mode 1 VRAM (#630).</summary>
[PpuLayoutBoundary("rendering view: Ceres flight and destruction cinematic VRAM")]
public readonly struct CeresVram(SnesVram vram)
{
    /// <summary>The cinematic OBJ characters.</summary>
    public VramRegion ObjectCharacters => new(vram, CeresVramLayout.ObjectCharacters);

    /// <summary>The SPACE COLONY caption tilemap.</summary>
    public VramRegion SpaceColonyTilemap => new(vram, CeresVramLayout.SpaceColonyTilemap);

    /// <summary>The Zebes reveal tilemap.</summary>
    public VramRegion ZebesTilemap => new(vram, CeresVramLayout.ZebesTilemap);
}

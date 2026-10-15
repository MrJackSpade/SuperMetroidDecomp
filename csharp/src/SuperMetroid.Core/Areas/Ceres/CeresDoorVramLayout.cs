namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Fixed VRAM destination of the installed Ceres door characters (#630).</summary>
[PpuLayoutBoundary("catalog: Ceres door VRAM destination")]
public static class CeresDoorVramLayout
{
    /// <summary>VMADD $7000-$71FF (bytes $E000): the 32 Ceres door characters replacing variant two's $B0:C400 DMA.</summary>
    public static readonly PpuLayoutCell Tiles = new(nameof(Tiles), PpuLayoutRegion.Vram,
        CeresDoorVisualRomData.TileVramDestination / sizeof(ushort), CeresDoorVisualRomData.TileByteCount / sizeof(ushort),
        "4-bpp OBJ characters", GameplayVramLayout.ObjectCharacters);
}
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>Fixed VRAM destinations of the installed world map characters (#630).</summary>
[PpuLayoutBoundary("catalog: world map VRAM destinations")]
public static class WorldMapVramLayout
{
    /// <summary>VMADD $0000-$2AFF: the world map's 4-bpp foreground characters from $8E:8000.</summary>
    public static readonly PpuLayoutCell Foreground = new(nameof(Foreground), PpuLayoutRegion.Vram,
        WorldMapArtworkFormat.ForegroundDestination / sizeof(ushort), WorldMapArtworkFormat.ForegroundBytes / sizeof(ushort),
        "4-bpp BG characters", PpuMemoryLayout.Vram);

    /// <summary>VMADD $4000-$42FF (bytes $8000): the world map's 2-bpp background characters from $8E:D600.</summary>
    public static readonly PpuLayoutCell Background = new(nameof(Background), PpuLayoutRegion.Vram,
        WorldMapArtworkFormat.BackgroundDestination / sizeof(ushort), WorldMapArtworkFormat.BackgroundBytes / sizeof(ushort),
        "2-bpp BG characters", PpuMemoryLayout.Vram);
}
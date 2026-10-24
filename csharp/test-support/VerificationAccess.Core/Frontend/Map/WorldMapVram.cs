using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Frontend.Map;

/// <summary>Typed view of the installed world map characters in VRAM (#630).</summary>
[PpuLayoutBoundary("rendering view: world map VRAM")]
internal readonly struct WorldMapVram(SnesVram vram)
{
    /// <summary>World map foreground characters.</summary>
    public VramRegion Foreground => new(vram, WorldMapVramLayout.Foreground);

    /// <summary>World map background characters.</summary>
    public VramRegion Background => new(vram, WorldMapVramLayout.Background);
}

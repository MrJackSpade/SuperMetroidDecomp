using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Core.Frontend.Map;

/// <summary>Typed view of the installed world map characters in VRAM (#630).</summary>
internal readonly struct WorldMapVram(SnesVram vram)
{
    /// <summary>World map foreground characters.</summary>
    public VramRegion Foreground => new(vram, WorldMapVramLayout.Foreground);

    /// <summary>World map background characters.</summary>
    public VramRegion Background => new(vram, WorldMapVramLayout.Background);
}

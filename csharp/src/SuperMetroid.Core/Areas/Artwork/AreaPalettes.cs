namespace SuperMetroid.Core.Areas.Artwork;

/// <summary>Typed view of the area set-piece palette ranges (#630, #1395).</summary>
[PpuLayoutBoundary("rendering view: area set-piece palettes")]
public readonly struct AreaPalettes(IPaletteColors cgram)
{
    /// <summary>Tourian statue base.</summary>
    public CgramPalette TourianStatueBase => new(cgram, AreaCgramLayout.TourianStatueBase);
    /// <summary>Tourian statue.</summary>
    public CgramPalette TourianStatue => new(cgram, AreaCgramLayout.TourianStatue);
    /// <summary>Tourian statue eye.</summary>
    public CgramPalette TourianStatueEye => new(cgram, AreaCgramLayout.TourianStatueEye);
}

using SuperMetroid.Core.Areas.Tourian;

namespace SuperMetroid.Core.Areas.Artwork;

/// <summary>CGRAM ranges the area set pieces write: the Tourian statues and the Ceres door (#630, #1395).</summary>
[PpuLayoutBoundary("catalog: area set-piece palettes")]
public static class AreaCgramLayout
{
    /// <summary>CGADD $F0-$FF: the Tourian statue base OBJ palette 7.</summary>
    public static readonly PpuLayoutCell TourianStatueBase = PpuMemoryLayout.Colors(nameof(TourianStatueBase),
        TourianStatuePaletteRomData.BaseCgramIndex, TourianStatuePaletteRomData.BaseColorCount, "Bgr555 OBJ palette 7");

    /// <summary>CGADD $A0-$AF: the Tourian statue OBJ palette 2.</summary>
    public static readonly PpuLayoutCell TourianStatue = PpuMemoryLayout.Colors(nameof(TourianStatue),
        TourianStatuePaletteRomData.StatueCgramIndex, TourianStatuePaletteRomData.StatueColorCount, "Bgr555 OBJ palette 2");

    /// <summary>CGADD $F9-$FC: the Tourian statue eye glow.</summary>
    public static readonly PpuLayoutCell TourianStatueEye = PpuMemoryLayout.Colors(nameof(TourianStatueEye),
        TourianStatuePaletteRomData.EyeCgramIndex, TourianStatuePaletteRomData.EyeColorCount, "Bgr555 eye glow");
}

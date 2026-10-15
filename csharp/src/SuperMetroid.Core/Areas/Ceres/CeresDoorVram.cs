namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Typed view of the installed Ceres door characters in VRAM (#630).</summary>
[PpuLayoutBoundary("rendering view: Ceres door VRAM")]
public readonly struct CeresDoorVram(SnesVram vram)
{
    /// <summary>Ceres door characters.</summary>
    public VramRegion Tiles => new(vram, CeresDoorVramLayout.Tiles);
}
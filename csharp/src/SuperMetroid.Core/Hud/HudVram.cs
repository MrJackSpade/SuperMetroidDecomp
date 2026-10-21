namespace SuperMetroid.Core.Hud;

/// <summary>Typed view of room-effect and HUD tilemap VRAM (#630).</summary>
[PpuLayoutBoundary("rendering view: room-effect and HUD tilemap VRAM")]
public static class HudVram
{
    /// <summary>Queued-DMA destination of the mutable HUD rows.</summary>
    public static VramDmaDestination HudMutableRowsDma => VramDmaDestination.At(HudVramLayout.HudMutableRows);
}

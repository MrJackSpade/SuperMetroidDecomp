namespace SuperMetroid.Core.Fx;

/// <summary>Typed view of room-effect and HUD tilemap VRAM (#630).</summary>
[PpuLayoutBoundary("rendering view: room-effect and HUD tilemap VRAM")]
public static class RoomFxVram
{
    /// <summary>Queued-DMA destination of the mutable HUD rows.</summary>
    public static VramDmaDestination HudMutableRowsDma => VramDmaDestination.At(RoomFxVramLayout.HudMutableRows);
}

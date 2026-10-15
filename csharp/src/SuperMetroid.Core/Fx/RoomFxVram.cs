namespace SuperMetroid.Core.Fx;

/// <summary>Typed view of room-effect and HUD tilemap VRAM (#630).</summary>
[PpuLayoutBoundary("rendering view: room-effect and HUD tilemap VRAM")]
public readonly struct RoomFxVram(SnesVram vram)
{
    /// <summary>The cleared layer-3 effect area.</summary>
    public VramRegion Layer3Area => new(vram, RoomFxVramLayout.Layer3Area);

    /// <summary>The room effect's layer-3 tilemap.</summary>
    public VramRegion Layer3Tilemap => new(vram, RoomFxVramLayout.Layer3Tilemap);

    /// <summary>Queued-DMA destination <paramref name="wordOffset"/> words into the scrolling-sky rows.</summary>
    public static VramDmaDestination ScrollingSkyDma(int wordOffset) => VramDmaDestination.At(RoomFxVramLayout.ScrollingSkyRows, wordOffset);

    /// <summary>Queued-DMA destination of the mutable HUD rows.</summary>
    public static VramDmaDestination HudMutableRowsDma => VramDmaDestination.At(RoomFxVramLayout.HudMutableRows);
}

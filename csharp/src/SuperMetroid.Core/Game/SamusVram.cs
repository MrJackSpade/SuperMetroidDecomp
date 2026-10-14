namespace SuperMetroid.Core.Game;

/// <summary>Typed view of Samus's OBJ character VRAM (#630).</summary>
[PpuLayoutBoundary("rendering view: Samus OBJ character VRAM")]
public readonly struct SamusVram(SnesVram vram)
{
    /// <summary>First half of the top-body tiles.</summary>
    public VramRegion BodyTopFirst => new(vram, SamusVramLayout.BodyTopFirst);

    /// <summary>Second half of the top-body tiles.</summary>
    public VramRegion BodyTopSecond => new(vram, SamusVramLayout.BodyTopSecond);

    /// <summary>First half of the bottom-body tiles.</summary>
    public VramRegion BodyBottomFirst => new(vram, SamusVramLayout.BodyBottomFirst);

    /// <summary>Second half of the bottom-body tiles.</summary>
    public VramRegion BodyBottomSecond => new(vram, SamusVramLayout.BodyBottomSecond);

    /// <summary>The selected beam's characters.</summary>
    public VramRegion BeamTiles => new(vram, SamusVramLayout.BeamTiles);

    /// <summary>Queued-DMA destination of the arm-cannon cover character.</summary>
    public static VramDmaDestination ArmCannonTileDma => VramDmaDestination.At(SamusVramLayout.ArmCannonTile);

    /// <summary>Queued-DMA destination of the grapple endpoint character.</summary>
    public static VramDmaDestination GrapplePointDma => VramDmaDestination.At(SamusVramLayout.GrapplePoint);

    /// <summary>Queued-DMA destination of the grapple rope-segment characters.</summary>
    public static VramDmaDestination GrappleSegmentsDma => VramDmaDestination.At(SamusVramLayout.GrappleSegments);

    /// <summary>Queued-DMA destination of the beam characters.</summary>
    public static VramDmaDestination BeamTilesDma => VramDmaDestination.At(SamusVramLayout.BeamTiles);
}

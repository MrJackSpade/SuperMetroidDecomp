using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Samus's OBJ character uploads inside the gameplay OBJ table (#630).</summary>
[PpuLayoutBoundary("catalog: Samus OBJ character VRAM")]
public static class SamusVramLayout
{
    /// <summary>VMADD $6000-$607F: the first half of Samus's top-body tiles (NMI <c>$80:9376</c>, first DMA).</summary>
    public static readonly PpuLayoutCell BodyTopFirst = new(nameof(BodyTopFirst), PpuLayoutRegion.Vram,
        SamusRenderingRomData.TileTransfers.TopDestinations.First, HalfRowWords, "4-bpp OBJ characters", GameplayVramLayout.ObjectCharacters);

    /// <summary>VMADD $6100-$617F: the second half of Samus's top-body tiles.</summary>
    public static readonly PpuLayoutCell BodyTopSecond = new(nameof(BodyTopSecond), PpuLayoutRegion.Vram,
        SamusRenderingRomData.TileTransfers.TopDestinations.Second, HalfRowWords, "4-bpp OBJ characters", GameplayVramLayout.ObjectCharacters);

    /// <summary>VMADD $6080-$60FF: the first half of Samus's bottom-body tiles.</summary>
    public static readonly PpuLayoutCell BodyBottomFirst = new(nameof(BodyBottomFirst), PpuLayoutRegion.Vram,
        SamusRenderingRomData.TileTransfers.BottomDestinations.First, HalfRowWords, "4-bpp OBJ characters", GameplayVramLayout.ObjectCharacters);

    /// <summary>VMADD $6180-$61FF: the second half of Samus's bottom-body tiles.</summary>
    public static readonly PpuLayoutCell BodyBottomSecond = new(nameof(BodyBottomSecond), PpuLayoutRegion.Vram,
        SamusRenderingRomData.TileTransfers.BottomDestinations.Second, HalfRowWords, "4-bpp OBJ characters", GameplayVramLayout.ObjectCharacters);

    /// <summary>VMADD $61F0-$61FF: OBJ tile $1F, the arm-cannon cover character (<c>$90:C7DF</c>).</summary>
    public static readonly PpuLayoutCell ArmCannonTile = new(nameof(ArmCannonTile), PpuLayoutRegion.Vram,
        SamusRenderingRomData.ArmCannon.TileVramDestination, SamusRenderingRomData.ArmCannon.TileUploadByteCount / sizeof(ushort),
        "4-bpp OBJ character", GameplayVramLayout.ObjectCharacters);

    /// <summary>VMADD $6200-$620F: the grapple endpoint character (<c>$9B:BFFB</c>).</summary>
    public static readonly PpuLayoutCell GrapplePoint = new(nameof(GrapplePoint), PpuLayoutRegion.Vram,
        Assets.GrappleTileDefinitions.PointDestination, SamusGrappleRomData.Rendering.PointTileByteCount / sizeof(ushort),
        "4-bpp OBJ character", GameplayVramLayout.ObjectCharacters);

    /// <summary>VMADD $6210-$624F: the four grapple rope-segment characters (<c>$9B:C02B</c>).</summary>
    public static readonly PpuLayoutCell GrappleSegments = new(nameof(GrappleSegments), PpuLayoutRegion.Vram,
        Assets.GrappleTileDefinitions.SegmentDestination, 0x40, "4-bpp OBJ characters", GameplayVramLayout.ObjectCharacters);

    /// <summary>VMADD $6300-$637F: the selected beam's projectile characters (<c>$90:AC8D</c>).</summary>
    public static readonly PpuLayoutCell BeamTiles = new(nameof(BeamTiles), PpuLayoutRegion.Vram,
        Assets.BeamTileAtlasDefinitions.DestinationWord, Assets.BeamTileAtlasDefinitions.ByteCount / sizeof(ushort),
        "4-bpp OBJ characters", GameplayVramLayout.ObjectCharacters);

    /// <summary>Words in each half of a Samus body tile row: eight 4-bpp characters.</summary>
    private const int HalfRowWords = 0x80;
}

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

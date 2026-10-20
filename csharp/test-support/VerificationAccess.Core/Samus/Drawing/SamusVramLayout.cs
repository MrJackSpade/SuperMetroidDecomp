using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Samus.Drawing;

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

    /// <summary>Words in each half of a Samus body tile row: eight 4-bpp characters.</summary>
    private const int HalfRowWords = 0x80;
}

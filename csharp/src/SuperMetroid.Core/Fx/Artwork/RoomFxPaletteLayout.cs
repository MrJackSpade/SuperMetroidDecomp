
namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>CGRAM ranges the room-FX layer-3 blend routines write (#630).</summary>
[PpuLayoutBoundary("catalog: room-FX layer-3 blend palette")]
public static class RoomFxPaletteLayout
{
    /// <summary>CGADD $19-$1B: BG3 palette 6, the three layer-3 FX blend colours.</summary>
    public static readonly PpuLayoutCell Layer3Blend = PpuMemoryLayout.Colors(nameof(Layer3Blend),
        RoomFxRomData.Layer3.PaletteBlendDestinationIndex, RoomFxRomData.Layer3.PaletteBlendColorCount, "Bgr555 layer-3 blend");

    /// <summary>CGADD $1B: the layer-3 blend colour a blend without a third colour clears to black.</summary>
    public static readonly PpuLayoutCell Layer3Empty = PpuMemoryLayout.Colors(nameof(Layer3Empty),
        RoomFxRomData.Layer3.EmptyPaletteColorIndex, 1, "Bgr555 layer-3 blend colour");
}

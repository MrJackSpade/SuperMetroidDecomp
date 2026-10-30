using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// Packs a layer's described line displacement (#1468) into the last four words of a pass's header for Tiles.hlsl: a
/// sine wave that ripples the layer sideways (<see cref="ScrollWave"/>) or a stretch that sags it downward through a band
/// (<see cref="ScrollStretch"/>). The pass's scroll words carry the shape's base scroll; the shader evaluates each line's
/// displacement from these words and adds it, so no table of lines is uploaded.
/// </summary>
internal static class D3D11LineDisplacement
{
    /// <summary>Header word of the shape's kind (<see cref="D3D11LineDisplacementKind"/>).</summary>
    private const int KindWord = 28;
    /// <summary>Header word of the shape's band: its first screen line, then the line after its last in the high half.</summary>
    private const int BandWord = 29;
    /// <summary>Header word of a wave's amplitude in 1/256ths of a pixel, or a stretch's slope in 1/65536ths of a row.</summary>
    private const int ScaleWord = 30;
    /// <summary>
    /// Header word of a wave's angle at line zero, then its period in lines in the high half; or a stretch's displacement of
    /// the lines above its band, the scroll they keep less its base, as a sixteen-bit word.
    /// </summary>
    private const int ShapeWord = 31;

    /// <summary>Packs <paramref name="wave"/> or <paramref name="stretch"/>, or no shape when both are null.</summary>
    /// <param name="header">The pass's cleared constant words.</param>
    /// <param name="wave">The sine wave rippling the layer sideways, or null.</param>
    /// <param name="stretch">The stretch sagging the layer downward, or null.</param>
    /// <exception cref="NotSupportedException">Both shapes are given; a pass displaces its lines by one.</exception>
    internal static void Pack(Span<uint> header, ScrollWave? wave, ScrollStretch? stretch)
    {
        if (wave is not null && stretch is not null)
            throw new NotSupportedException("A layer pass displaces its lines by one described shape, not a wave and a stretch together.");
        if (wave is { } ripple)
        {
            header[KindWord] = (uint)D3D11LineDisplacementKind.Wave;
            header[BandWord] = D3D11ConstantPacking.Halves(checked((ushort)ripple.FirstLine), checked((ushort)ripple.EndLine));
            header[ScaleWord] = (uint)ripple.Amplitude;
            header[ShapeWord] = D3D11ConstantPacking.Halves(ripple.Phase.RawValue, checked((ushort)ripple.Period));
        }
        else if (stretch is { } sag)
        {
            header[KindWord] = (uint)D3D11LineDisplacementKind.Stretch;
            header[BandWord] = D3D11ConstantPacking.Halves(checked((ushort)sag.FirstLine), checked((ushort)sag.EndLine));
            header[ScaleWord] = (uint)sag.Slope;
            header[ShapeWord] = unchecked((ushort)(sag.ScrollAbove - sag.BaseScroll));
        }
    }
}

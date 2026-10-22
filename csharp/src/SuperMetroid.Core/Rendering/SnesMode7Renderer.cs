using System.Runtime.CompilerServices;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rendering;

/// <summary>Software projection of the SNES Mode 7 background layout.</summary>
/// <remarks>
/// Mode 7 does not use the ordinary two-byte BG tilemap words. VRAM's low byte is a
/// 128x128 tile-number map and its high byte is an 8-bpp character plane. This unusual
/// interleave is exactly why the title loader writes <c>$2119</c> and <c>$2118</c> in
/// separate DMA passes at <c>$8B:9BFF-$8B:9C56</c>.
/// </remarks>
[PpuLayoutBoundary("backend: software PPU Mode 7 rasterizer")]
public static class SnesMode7Renderer
{
    /// <summary>Mode 7's square tile-number map spans 128 cells per axis.</summary>
    private const int MapWidthInTiles = 128;
    /// <summary>Each Mode 7 character occupies an eight-pixel square in high VRAM bytes.</summary>
    private const int CharacterWidth = 8;
    /// <summary>Wraps transformed source coordinates to the native 1024-pixel map.</summary>
    private const int Mode7CoordinateMask = 1023;

    /// <summary>
    /// Projects Mode 7 directly over caller-owned storage. Transparent samples leave the
    /// existing destination intact, allowing the gameplay compositor to begin with its
    /// backdrop and avoid a second full-frame raster plus copy.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void CompositeViewport(Span<Rgba32> output, SnesVram vram, IPaletteColors cgram, Mode7RenderRegisters registers, Extent16<ScreenPixels>? size = null) =>
        CompositeViewport(output, vram, cgram, registers, map: null, tiles: null, size);

    /// <summary>
    /// Projects Mode 7 over caller-owned storage with tile numbers from <paramref name="map"/> (#1349), or from VRAM's
    /// map plane when a frontend scene's map lives there, drawing each tile from <paramref name="tiles"/>' pictures once it
    /// carries them (#1392), otherwise from VRAM's characters.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void CompositeViewport(Span<Rgba32> output, SnesVram vram, IPaletteColors cgram, Mode7RenderRegisters registers,
        Mode7MapSnapshot? map, Mode7TilePictures? tiles, Extent16<ScreenPixels>? size = null)
    {
        Ensure.NotNull(vram);
        Ensure.NotNull(cgram);
        Extent16<ScreenPixels> viewport = size ?? SnesPpuLayout.ScreenSize;
        int width = viewport.X;
        int height = viewport.Y;
        if (width == 0 || height == 0)
            throw new ArgumentOutOfRangeException(nameof(size), viewport, "A Mode 7 viewport must have positive dimensions.");
        Ensure.LengthEqual(output, width * height);

        // VRAM and CGRAM cannot change during a software scanout. Reading their backing
        // spans once removes two bounds-checking method calls and one BGR555 conversion
        // from every visible Mode 7 pixel, which is especially expensive under Debug JIT.
        ReadOnlySpan<byte> vramBytes = vram.Bytes;
        ReadOnlySpan<Bgr555> cgramColors = cgram.Colors;
        var pictures = new BgPicturePixels(cgram);
        Span<Rgba32> palette = stackalloc Rgba32[SnesCgram.ColorCount];
        for (int color = 0; color < palette.Length; color++)
            palette[color] = cgramColors[color].ToRgba32();

        // The PPU reads the low thirteen bits of each register as a signed value.
        int signedCenterX = SignExtend13(registers.Center.X);
        int signedCenterY = SignExtend13(registers.Center.Y);
        int clippedH = SignExtend13(registers.Scroll.X) - signedCenterX;
        int clippedV = SignExtend13(registers.Scroll.Y) - signedCenterY;
        // Bit 13 is the sign of the thirteen-bit register difference.
        clippedH = (clippedH & (1 << 13)) != 0 ? clippedH | ~Mode7CoordinateMask : clippedH & Mode7CoordinateMask;
        clippedV = (clippedV & (1 << 13)) != 0 ? clippedV | ~Mode7CoordinateMask : clippedV & Mode7CoordinateMask;
        for (int screenY = 0; screenY < height; screenY++)
        {
            // Mode 7 uses physical scanline one for the first displayed row. Truncate
            // each start-product's low six bits before adding, as the PPU does; one
            // algebraically combined matrix expression loses these hardware roundings.
            int startX = ((registers.MatrixA * clippedH) & ~63) + ((registers.MatrixB * (screenY + 1)) & ~63)
                + ((registers.MatrixB * clippedV) & ~63) + SnesEightEight.FromWhole(signedCenterX);
            int startY = ((registers.MatrixC * clippedH) & ~63) + ((registers.MatrixD * (screenY + 1)) & ~63)
                + ((registers.MatrixD * clippedV) & ~63) + SnesEightEight.FromWhole(signedCenterY);
            for (int screenX = 0; screenX < width; screenX++)
            {
                int sourceX = SnesEightEight.WholeOf(startX + registers.MatrixA * screenX);
                int sourceY = SnesEightEight.WholeOf(startY + registers.MatrixC * screenX);

                bool outside = (uint)sourceX >= 1024 || (uint)sourceY >= 1024;
                if (outside && !registers.WrapOutsideMap && !registers.FillOutsideWithCharacterZero)
                    continue;

                // With M7SEL bit seven clear, even outside samples use the map's
                // ten-bit address components. Otherwise $80 is transparent and $C0
                // selects the corresponding pixel of character zero.
                int wrappedX = sourceX & Mode7CoordinateMask;
                int wrappedY = sourceY & Mode7CoordinateMask;
                int pixelX = wrappedX & 7;
                int pixelY = wrappedY & 7;
                int character = outside && !registers.WrapOutsideMap
                    ? 0
                    : map is not null ? map.TileAt(new Position16<TilemapCells>((ushort)(wrappedX >> 3), (ushort)(wrappedY >> 3)))
                    : vramBytes[((wrappedY >> 3) * MapWidthInTiles + (wrappedX >> 3)) * 2];

                // Each 8-bpp Mode 7 character consumes 64 VRAM words. Its color indexes
                // occupy only their high bytes; low bytes at the same word addresses are
                // part of the tile-number map and must never be interpreted as pixels.
                if (tiles is not null)
                {
                    if (tiles.TrySample(character, new Position16<CharacterPixels>((ushort)pixelX, (ushort)pixelY), out BgSample shown)
                        && pictures.TryDecode(shown, palette, ColorZero.Transparent, out Rgba32 pictured, out _))
                        output[screenY * width + screenX] = pictured;
                    continue;
                }
                int characterWord = character * CharacterWidth * CharacterWidth + pixelY * CharacterWidth + pixelX;
                int colorIndex = vramBytes[characterWord * 2 + 1];
                if (colorIndex != 0)
                    output[screenY * width + screenX] = palette[colorIndex];
            }
        }
    }

    /// <summary>Interprets a Mode 7 center or scroll register using its low thirteen signed bits.</summary>
    private static int SignExtend13(short register) => (register << 19) >> 19;
}

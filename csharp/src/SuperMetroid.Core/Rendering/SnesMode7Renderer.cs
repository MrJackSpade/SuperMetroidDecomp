using SuperMetroid.Core.Gameplay;
using System.Runtime.CompilerServices;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rendering;

/// <summary>Software projection of a Mode 7 floor (#1403): its map's tiles, turned, scaled and scrolled by its transform.</summary>
[PpuLayoutBoundary("backend: software PPU Mode 7 rasterizer")]
public static class SnesMode7Renderer
{
    /// <summary>Wraps transformed source coordinates to the native 1024-pixel map.</summary>
    private const int Mode7CoordinateMask = 1023;

    /// <summary>
    /// Projects <paramref name="floor"/> over caller-owned storage through <paramref name="registers"/>, drawing each tile
    /// its map names from its pre-coloured picture (#1392); transparent pixels leave the destination untouched.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static void CompositeViewport(Span<Rgba32> output, IPaletteColors cgram, AffineTransform registers,
        AffineFloor floor, Extent16<ScreenPixels>? size = null)
    {
        Ensure.NotNull(cgram);
        Ensure.NotNull(floor);
        Mode7TilePictures tiles = floor.Tiles;
        Extent16<ScreenPixels> viewport = size ?? SnesPpuLayout.ScreenSize;
        int width = viewport.X;
        int height = viewport.Y;
        if (width == 0 || height == 0)
            throw new ArgumentOutOfRangeException(nameof(size), viewport, "A Mode 7 viewport must have positive dimensions.");
        Ensure.LengthEqual(output, width * height);

        // CGRAM cannot change during a software scanout. Reading its backing span once removes a
        // bounds-checking method call and a BGR555 conversion from every visible Mode 7 pixel.
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
                if (outside && registers.Outside == Mode7OverflowMode.Transparent)
                    continue;

                // With M7SEL bit seven clear, even outside samples use the map's
                // ten-bit address components. Otherwise $80 is transparent and $C0
                // selects the corresponding pixel of character zero.
                int wrappedX = sourceX & Mode7CoordinateMask;
                int wrappedY = sourceY & Mode7CoordinateMask;
                int pixelX = wrappedX & 7;
                int pixelY = wrappedY & 7;
                int character = outside && registers.Outside != Mode7OverflowMode.Wrap
                    ? 0
                    : floor.TileAt(new Position16<TilemapCells>((ushort)(wrappedX >> 3), (ushort)(wrappedY >> 3)));
                if (tiles.TrySample(character, new Position16<CharacterPixels>((ushort)pixelX, (ushort)pixelY), out BgSample shown)
                    && pictures.TryDecode(shown, palette, ColorZero.Transparent, out Rgba32 pictured, out _))
                    output[screenY * width + screenX] = pictured;
            }
        }
    }

    /// <summary>Interprets a Mode 7 center or scroll register using its low thirteen signed bits.</summary>
    private static int SignExtend13(short register) => (register << 19) >> 19;
}

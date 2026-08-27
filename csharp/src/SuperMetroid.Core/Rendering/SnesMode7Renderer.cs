using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Software projection of the SNES Mode 7 background layout.</summary>
/// <remarks>
/// Mode 7 does not use the ordinary two-byte BG tilemap words. VRAM's low byte is a
/// 128x128 tile-number map and its high byte is an 8-bpp character plane. This unusual
/// interleave is exactly why the title loader writes <c>$2119</c> and <c>$2118</c> in
/// separate DMA passes at <c>$8B:9BFF-$8B:9C56</c>.
/// </remarks>
public static class SnesMode7Renderer
{
    private const int MapWidthInTiles = 128;
    private const int CharacterWidth = 8;
    private const int Mode7CoordinateMask = 0x03ff;

    /// <summary>
    /// Renders one Mode 7 viewport using the signed 8.8 matrix/register convention.
    /// </summary>
    /// <remarks>
    /// The identity values A=D=$0100 and B=C=0 produce one source pixel per screen pixel.
    /// Outside-map policy defaults to M7SEL=$80: bit 7 disables wrapping and bit 6 leaves
    /// overflow transparent. Character-zero fill is available for the distinct $C0 mode.
    /// Transparent color zero is returned with alpha zero so OBJ/backdrop composition can
    /// retain the normal renderer contract.
    /// </remarks>
    public static Rgba32[] RenderViewport(
        SnesVram vram,
        SnesCgram cgram,
        short matrixA,
        short matrixB,
        short matrixC,
        short matrixD,
        short centerX,
        short centerY,
        short horizontalOffset,
        short verticalOffset,
        int width = 256,
        int height = 224,
        bool fillOutsideWithCharacterZero = false)
    {
        ArgumentNullException.ThrowIfNull(vram);
        ArgumentNullException.ThrowIfNull(cgram);
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height));

        var output = new Rgba32[checked(width * height)];
        for (int screenY = 0; screenY < height; screenY++)
        {
            // The hardware transforms `(screen + scroll - center)`, then adds the center
            // back. Scroll therefore moves along the already-rotated map axes. Adding it
            // after the matrix happens to work for an identity transform but rotates the
            // transparent-overflow boundary around the wrong point in every Ceres shot.
            int relativeY = screenY + verticalOffset - centerY;
            for (int screenX = 0; screenX < width; screenX++)
            {
                int relativeX = screenX + horizontalOffset - centerX;
                int sourceX = ((matrixA * relativeX + matrixB * relativeY) >> 8) + centerX;
                int sourceY = ((matrixC * relativeX + matrixD * relativeY) >> 8) + centerY;

                bool outside = (uint)sourceX >= 1024 || (uint)sourceY >= 1024;
                if (outside && !fillOutsideWithCharacterZero)
                    continue;

                // M7SEL=$80 makes an out-of-bounds sample transparent. Only $C0 selects
                // the corresponding pixel of character zero. In-bounds coordinates use
                // the map's ordinary ten-bit address components.
                int wrappedX = sourceX & Mode7CoordinateMask;
                int wrappedY = sourceY & Mode7CoordinateMask;
                int pixelX = wrappedX & 7;
                int pixelY = wrappedY & 7;
                int character = outside
                    ? 0
                    : vram.ReadByte((((wrappedY >> 3) * MapWidthInTiles + (wrappedX >> 3)) * 2));

                // Each 8-bpp Mode 7 character consumes 64 VRAM words. Its color indexes
                // occupy only their high bytes; low bytes at the same word addresses are
                // part of the tile-number map and must never be interpreted as pixels.
                int characterWord = character * CharacterWidth * CharacterWidth + pixelY * CharacterWidth + pixelX;
                int colorIndex = vram.ReadByte(characterWord * 2 + 1);
                if (colorIndex != 0)
                    output[screenY * width + screenX] = cgram.GetRgba(colorIndex);
            }
        }

        return output;
    }
}

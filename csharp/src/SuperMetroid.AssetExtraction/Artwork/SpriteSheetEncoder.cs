using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Encodes sheets into their installed files (#1392) with buffers it reuses from one sheet to the next (#1490), so a
/// thread encoding many sheets allocates only their finished files. Not shared between threads.
/// </summary>
internal sealed class SpriteSheetEncoder : IDisposable
{
    /// <summary>The PNG encoder's rows and compressed stream.</summary>
    private readonly RgbaPngBuffers png = new();

    /// <summary>The palette-index image of the sheet being encoded.</summary>
    private RgbaPixel[] paletteImage = [];

    /// <summary>The planes a laid-out sheet is painted on before it is encoded.</summary>
    private readonly SpriteSheetCanvas canvas = new();

    /// <summary>
    /// Paints a laid-out sheet on the reused canvas and encodes it, so a sheet that is only written needs no planes of
    /// its own (#1490).
    /// </summary>
    internal EncodedSpriteSheet Encode(SpriteSheetLayout layout)
    {
        Ensure.NotNull(layout);
        SpriteSheetPlanes planes = canvas.Cleared(layout.Size);
        SpriteSheetDocument manifest = layout.Paint(planes);
        return Encode(planes, manifest);
    }

    /// <summary>Encodes one sheet's image, palette-index image and frame manifest.</summary>
    /// <param name="planes">The sheet's pixel planes.</param>
    /// <param name="manifest">The sheet's frame manifest.</param>
    internal EncodedSpriteSheet Encode(SpriteSheetPlanes planes, SpriteSheetDocument manifest)
    {
        Ensure.NotNull(manifest);
        byte[] image = RgbaPng.Encode(planes.Size, planes.Pixels, png);
        byte[] palette = RgbaPng.Encode(planes.Size, PaletteImage(planes), png);
        return new EncodedSpriteSheet(image, palette, SpriteSheetManifestWriter.Write(manifest));
    }

    /// <summary>
    /// The palette-index image: each index in red, priority in green and part colour group's code in blue, opaque where
    /// a colour is shown. It is laid out in the reused image buffer, valid until the next sheet.
    /// </summary>
    private ReadOnlySpan<RgbaPixel> PaletteImage(SpriteSheetPlanes planes)
    {
        ReadOnlySpan<byte> indices = planes.PaletteIndices;
        ReadOnlySpan<byte> priorities = planes.Priorities;
        ReadOnlySpan<byte> partGroups = planes.PartGroups;
        if (paletteImage.Length < indices.Length)
            paletteImage = new RgbaPixel[indices.Length];
        Span<RgbaPixel> pixels = paletteImage.AsSpan(0, indices.Length);
        for (int pixel = 0; pixel < indices.Length; pixel++)
            pixels[pixel] = new RgbaPixel(indices[pixel], priorities[pixel], partGroups[pixel], indices[pixel] == 0 ? (byte)0 : byte.MaxValue);
        return pixels;
    }

    /// <inheritdoc />
    public void Dispose() => png.Dispose();
}

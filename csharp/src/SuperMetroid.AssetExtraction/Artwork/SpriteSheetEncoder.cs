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

    /// <summary>The palette map of the sheet being encoded.</summary>
    private RgbaPixel[] paletteImage = [];

    /// <summary>The planes a laid-out sheet is painted on before it is encoded.</summary>
    private readonly SpriteSheetCanvas canvas = new();

    /// <summary>The installation's rendered pictures, which refuse a picture rendered twice (#1635).</summary>
    private readonly SheetPictureRegistry pictures;

    /// <summary>Starts an encoder rendering into one installation's pictures.</summary>
    /// <param name="pictures">The installation's rendered pictures, shared by every encoder of the run.</param>
    internal SpriteSheetEncoder(SheetPictureRegistry pictures) => this.pictures = Ensure.NotNull(pictures);

    /// <summary>
    /// Paints a laid-out sheet on the reused canvas and encodes it, so a sheet that is only written needs no planes of
    /// its own (#1490).
    /// </summary>
    /// <param name="layout">The sheet's layout.</param>
    /// <param name="sheet">Names the sheet if a picture on it was already rendered.</param>
    internal EncodedSpriteSheet Encode(SpriteSheetLayout layout, string sheet)
    {
        Ensure.NotNull(layout);
        SpriteSheetPlanes planes = canvas.Cleared(layout.Size);
        SpriteSheetDocument manifest = layout.Paint(planes);
        return Encode(planes, manifest, sheet);
    }

    /// <summary>Paints a sheet of pictures already painted (#1513) on the reused canvas and encodes it.</summary>
    /// <param name="layout">The sheet's pictures.</param>
    /// <param name="sheet">Names the sheet if a picture on it was already rendered.</param>
    internal EncodedSpriteSheet Encode(PictureSheetLayout layout, string sheet)
    {
        Ensure.NotNull(layout);
        SpriteSheetPlanes planes = canvas.Cleared(layout.Size);
        SpriteSheetDocument manifest = layout.Paint(planes);
        return Encode(planes, manifest, sheet);
    }

    /// <summary>
    /// Encodes one composed sheet as the import installs it (#1544): its image, the palette map of each pixel's ink, naming
    /// its colour by its place among the image's own colours, and the manifest of those colours and the frames.
    /// </summary>
    /// <param name="planes">The sheet's pixel planes, composed against the cartridge's colours.</param>
    /// <param name="manifest">The sheet's composed manifest.</param>
    /// <param name="sheet">Names the sheet if a picture on it was already rendered.</param>
    /// <exception cref="InvalidDataException">A picture on the sheet was already rendered in this installation (#1635).</exception>
    internal EncodedSpriteSheet Encode(SpriteSheetPlanes planes, SpriteSheetDocument manifest, string sheet)
    {
        Ensure.NotNull(manifest);
        CompactedSheet compacted = SheetCompaction.Compact(planes, manifest);
        SpriteSheetPlanes held = compacted.Planes();
        pictures.Admit(sheet, held, compacted.Manifest);
        InstalledSheetLayers installed = InstalledSheetColouring.Install(compacted.Manifest, held.PaletteIndices, held.FrontTiles);
        byte[] image = RgbaPng.Encode(held.Size, held.Pixels, png);
        byte[] palette = RgbaPng.Encode(held.Size, PaletteImage(installed.Inks), png);
        return new EncodedSpriteSheet(image, palette, SpriteSheetManifestWriter.Write(installed.Manifest));
    }

    /// <summary>
    /// The palette map: each pixel's ink as <see cref="InstalledSheetColouring.MapPixelOf"/> installs it. It is laid out in
    /// the reused image buffer, valid until the next sheet.
    /// </summary>
    private ReadOnlySpan<RgbaPixel> PaletteImage(ReadOnlySpan<SpriteImageInk> inks)
    {
        if (paletteImage.Length < inks.Length)
            paletteImage = new RgbaPixel[inks.Length];
        Span<RgbaPixel> pixels = paletteImage.AsSpan(0, inks.Length);
        for (int pixel = 0; pixel < inks.Length; pixel++)
            pixels[pixel] = InstalledSheetColouring.MapPixelOf(inks[pixel]);
        return pixels;
    }

    /// <inheritdoc />
    public void Dispose() => png.Dispose();
}

using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>One immutable eight-tile beam sheet with calculated stock orientations and independently editable pixels.</summary>
public sealed class BeamTileAtlas
{
    // Unresolved artwork remains required; only the five documented Plasma pen roles have a narrow exception.
    /// <summary>Selected pen values stored where the imported sheet differs from calculated stock pixels or required ink.</summary>
    private readonly Dictionary<int, byte> pixels = new();

    /// <summary>Beam identity used to resolve stock orientation, shared-tile scope, and permitted Plasma ink.</summary>
    private readonly int selection;

    /// <summary>Optional atlas that supplies pixels shared with another beam selection.</summary>
    private readonly BeamTileAtlas? sharedSource;

    /// <summary>Whether sharing falls back to the source atlas for every pixel instead of only shared tile regions.</summary>
    private readonly bool wholeSheetShared;

    /// <summary>Creates an atlas from imported pixels, storing deviations while leaving resolvable stock pixels implicit.</summary>
    /// <param name="selectedPixels">Validated indexed pixels in the beam sheet's native upload order.</param>
    /// <param name="selection">Native beam identity controlling stock pixel relationships and pen roles.</param>
    private BeamTileAtlas(byte[] selectedPixels, int selection)
    {
        this.selection = selection;
        for (int pixel = 0; pixel < selectedPixels.Length; pixel++)
        {
            if (BeamTileAtlasDefinitions.TryStockPlasmaInk(selection, pixel, out byte ink))
            {
                if (selectedPixels[pixel] != ink) pixels[pixel] = selectedPixels[pixel];
                continue;
            }
            if (!BeamTileAtlasDefinitions.TryDerivedPixelSource(selection, pixel, out int source)
                || selectedPixels[pixel] != (source < 0 ? 0 : selectedPixels[source]))
                pixels[pixel] = selectedPixels[pixel];
        }
    }

    /// <summary>Creates a derived atlas whose unspecified pixels are resolved from a shared source atlas.</summary>
    /// <param name="pixels">Selected pixel values that differ from the chosen source or stock fallback.</param>
    /// <param name="selection">Native beam identity for this atlas's tile-sharing rules.</param>
    /// <param name="sharedSource">Atlas consulted when a pixel is omitted from <paramref name="pixels"/>.</param>
    /// <param name="wholeSheetShared">Whether the source provides all pixels rather than only shared tile regions.</param>
    private BeamTileAtlas(Dictionary<int, byte> pixels, int selection, BeamTileAtlas sharedSource, bool wholeSheetShared)
    {
        this.pixels = pixels;
        this.selection = selection;
        this.sharedSource = sharedSource;
        this.wholeSheetShared = wholeSheetShared;
    }

    /// <summary>Builds an atlas that inherits selected pixels from another beam, preserving local overrides.</summary>
    /// <param name="source">Atlas supplying pixels in the shared region, or across the whole sheet when requested.</param>
    /// <param name="wholeSheet">When true, use <paramref name="source"/> as the fallback for every pixel.</param>
    /// <returns>A new immutable atlas that resolves omitted pixels through the selected sharing rule.</returns>
    internal BeamTileAtlas SharePixelsFrom(BeamTileAtlas source, bool wholeSheet)
    {
        var selected = new Dictionary<int, byte>(pixels);
        for (int pixel = 0; pixel < BeamTileAtlasDefinitions.Width * BeamTileAtlasDefinitions.Height; pixel++)
        {
            if (!wholeSheet && !BeamTileAtlasDefinitions.IsSharedTilePixel(selection, pixel)) continue;
            byte value = Pixel(pixel);
            if (value == source.Pixel(pixel)) selected.Remove(pixel);
            else selected[pixel] = value;
        }
        return new(selected, selection, source, wholeSheet);
    }
    /// <summary>Materializes the selected eight-tile DMA payload; no expanded stock transfer is cached.</summary>
    public ReadOnlyMemory<byte> Transfer => SnesPlanarTileEncoder.Encode(
        Enumerable.Range(0, BeamTileAtlasDefinitions.Width * BeamTileAtlasDefinitions.Height)
            .Select(Pixel).ToArray(), BeamTileAtlasDefinitions.Width, BeamTileAtlasDefinitions.Height, 4);

    /// <summary>Resolves one selected pen value from local edits, shared pixels, stock ink, or stock orientation rules.</summary>
    /// <param name="pixel">Linear index into the 64-by-8 sheet in upload order.</param>
    /// <returns>The four-bit pen index required by the selected beam sheet.</returns>
    /// <exception cref="InvalidOperationException">No local, shared, or defined stock source supplies the required pixel.</exception>
    private byte Pixel(int pixel)
    {
        if (pixels.TryGetValue(pixel, out byte selected)) return selected;
        if (sharedSource is not null && (wholeSheetShared || BeamTileAtlasDefinitions.IsSharedTilePixel(selection, pixel)))
            return sharedSource.Pixel(pixel);
        if (BeamTileAtlasDefinitions.TryStockPlasmaInk(selection, pixel, out byte ink)) return ink;
        if (BeamTileAtlasDefinitions.TryDerivedPixelSource(selection, pixel, out int source))
            return source < 0 ? (byte)0 : Pixel(source);
        throw new InvalidOperationException("Required beam artwork pixel is absent.");
    }

    /// <summary>Imports a 64-by-8 indexed sheet in native eight-character upload order, preserving selected four-bit pen indices and independently edited departures from stock pixel relationships.</summary>
    /// <param name="png">Caller-owned indexed PNG stream, left open; palette RGB values do not select the beam's runtime colors.</param>
    /// <param name="selection">Native beam identity $00..$0B, bounded Chainsaw $0D, or SpaceTime $0E; not the fourteen-sheet artwork ordinal.</param>
    /// <returns>An immutable selected sheet that materializes its $0100-byte four-bit planar payload through <see cref="Transfer"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="selection"/> is outside the supported ordinary and bounded beam domain.</exception>
    /// <exception cref="InvalidDataException">The PNG format, dimensions, or palette indices cannot represent the required four-bit beam characters.</exception>
    public static BeamTileAtlas Load(Stream png, int selection)
    {
        if ((uint)selection >= BeamTileAtlasDefinitions.SelectionCount && selection is not (ChainsawBeamGraphicsDefinitions.Selection or SpacetimeBeamGraphicsDefinitions.Selection))
            throw new ArgumentOutOfRangeException(nameof(selection));
        var image = IndexedPng.Read(png, BeamTileAtlasDefinitions.Width, BeamTileAtlasDefinitions.Height);
        // Retain the original encoder's exact four-bit input validation at import.
        _ = SnesPlanarTileEncoder.Encode(image.Pixels, image.Width, image.Height, 4);
        return new(image.Pixels, selection);
    }
}


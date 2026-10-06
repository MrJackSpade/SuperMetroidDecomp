using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>One immutable eight-tile beam sheet with calculated stock orientations and independently editable pixels.</summary>
public sealed class BeamTileAtlas
{
    // Unresolved artwork remains required; only the five documented Plasma pen roles have a narrow exception.
    private readonly Dictionary<int, byte> pixels = new();
    private readonly int selection;
    private readonly BeamTileAtlas? sharedSource;
    private readonly bool wholeSheetShared;

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

    private BeamTileAtlas(Dictionary<int, byte> pixels, int selection, BeamTileAtlas sharedSource, bool wholeSheetShared)
    {
        this.pixels = pixels;
        this.selection = selection;
        this.sharedSource = sharedSource;
        this.wholeSheetShared = wholeSheetShared;
    }

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

    public void LoadTo(SnesVram vram) =>
        vram.LoadBytes(BeamTileAtlasDefinitions.DestinationWord * 2, Transfer.Span);

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


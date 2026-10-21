using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>One immutable eight-tile beam sheet with calculated stock orientations and independently editable pixels.</summary>
public sealed class BeamTileAtlas
{
    /// <summary>Stores edited or independently required pixels; derived stock pixels are calculated on demand.</summary>
    /// <remarks>Unresolved artwork remains required except for the five documented Plasma pen roles.</remarks>
    internal readonly Dictionary<int, byte> pixels = new();
    /// <summary>Beam combination whose stock pixel relationships determine this sheet.</summary>
    private readonly SamusBeamCombination selection;
    /// <summary>Another atlas supplying pixels shared by the selected beam combination.</summary>
    private readonly BeamTileAtlas? sharedSource;
    /// <summary>Applies the shared source across the full sheet instead of designated tiles.</summary>
    private readonly bool wholeSheetShared;

    /// <summary>Stores only pixels that differ from derivable stock relationships.</summary>
    private BeamTileAtlas(byte[] selectedPixels, SamusBeamCombination selection)
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

    /// <summary>Creates an atlas that resolves shared pixels through another selected sheet.</summary>
    private BeamTileAtlas(Dictionary<int, byte> pixels, SamusBeamCombination selection, BeamTileAtlas sharedSource, bool wholeSheetShared)
    {
        this.pixels = pixels;
        this.selection = selection;
        this.sharedSource = sharedSource;
        this.wholeSheetShared = wholeSheetShared;
    }

    /// <summary>Reuses source pixels where they match while retaining independent edits in this sheet.</summary>
    internal BeamTileAtlas SharePixelsFrom(BeamTileAtlas source, BeamTileSharing sharing)
    {
        var selected = new Dictionary<int, byte>(pixels);
        for (int pixel = 0; pixel < BeamTileAtlasDefinitions.Width * BeamTileAtlasDefinitions.Height; pixel++)
        {
            if (sharing != BeamTileSharing.WholeSheet && !BeamTileAtlasDefinitions.IsSharedTilePixel(selection, pixel)) continue;
            byte value = Pixel(pixel);
            if (value == source.Pixel(pixel)) selected.Remove(pixel);
            else selected[pixel] = value;
        }
        return new(selected, selection, source, sharing == BeamTileSharing.WholeSheet);
    }
    /// <summary>Materializes the selected eight-tile DMA payload; no expanded stock transfer is cached.</summary>
    public ReadOnlyMemory<byte> Transfer => SnesPlanarTileEncoder.Encode(
        Enumerable.Range(0, BeamTileAtlasDefinitions.Width * BeamTileAtlasDefinitions.Height)
            .Select(Pixel).ToArray(), new(BeamTileAtlasDefinitions.Width, BeamTileAtlasDefinitions.Height), 4);

    /// <summary>Resolves an edited, shared, stock Plasma, or derived pixel in that priority order.</summary>
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
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="selection"/> has no artwork ($0C or $0F).</exception>
    /// <exception cref="InvalidDataException">The PNG format, dimensions, or palette indices cannot represent the required four-bit beam characters.</exception>
    public static BeamTileAtlas Load(Stream png, SamusBeamCombination selection)
    {
        _ = BeamTileAtlasDefinitions.ArtworkOrdinal(selection);
        var image = IndexedPng.Read(png, new(BeamTileAtlasDefinitions.Width, BeamTileAtlasDefinitions.Height));
        // Retain the original encoder's exact four-bit input validation at import.
        _ = SnesPlanarTileEncoder.Encode(image.Pixels, image.Size, 4);
        return new(image.Pixels, selection);
    }
}


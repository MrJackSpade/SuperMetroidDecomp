using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>One immutable eight-tile beam sheet, its pixels as installed (#1596).</summary>
public sealed class BeamTileAtlas
{
    /// <summary>The sheet's installed four-bit pen indices, row-major across its eight characters.</summary>
    private readonly byte[] pixels;

    /// <summary>Keeps the sheet's installed pixels.</summary>
    private BeamTileAtlas(byte[] pixels) => this.pixels = pixels;

    /// <summary>The sheet's eight-tile DMA payload, encoded from its installed pixels.</summary>
    public ReadOnlyMemory<byte> Characters => SnesPlanarTileEncoder.Encode(
        pixels, new(BeamTileAtlasDefinitions.Width, BeamTileAtlasDefinitions.Height), 4);

    /// <summary>Imports a 64-by-8 indexed sheet in native eight-character upload order, keeping its four-bit pen indices as installed.</summary>
    /// <param name="png">Caller-owned indexed PNG stream, left open; palette RGB values do not select the beam's runtime colors.</param>
    /// <param name="selection">Native beam identity $00..$0B, bounded Chainsaw $0D, or SpaceTime $0E; not the fourteen-sheet artwork ordinal.</param>
    /// <returns>An immutable selected sheet that materializes its $0100-byte four-bit planar payload through <see cref="Characters"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="selection"/> has no artwork ($0C or $0F).</exception>
    /// <exception cref="InvalidDataException">The PNG format, dimensions, or palette indices cannot represent the required four-bit beam characters.</exception>
    public static BeamTileAtlas Load(Stream png, SamusBeamCombination selection)
    {
        _ = BeamTileAtlasDefinitions.ArtworkOrdinal(selection);
        var image = IndexedPng.Read(png, new(BeamTileAtlasDefinitions.Width, BeamTileAtlasDefinitions.Height));
        // Retain the original encoder's exact four-bit input validation at import.
        _ = SnesPlanarTileEncoder.Encode(image.Pixels, image.Size, 4);
        return new([.. image.Pixels]);
    }
}

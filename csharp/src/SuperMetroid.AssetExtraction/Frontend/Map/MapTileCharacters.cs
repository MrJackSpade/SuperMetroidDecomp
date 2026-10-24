using SuperMetroid.Core.Map.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The installed map-screen or pause-interface characters as the import pipeline reads them (#1409): a 256-by-64 indexed
/// PNG compiled to 256 row-major 8-by-8 SNES 4-bpp characters, which the screen tile sheets compose from.
/// </summary>
public sealed class MapTileCharacters : MapTileAtlas
{
    /// <summary>Owned SNES 4-bpp character bytes compiled from indexed PNG pixels.</summary>
    private readonly byte[] planar;

    /// <summary>Retains already compiled planar characters.</summary>
    /// <param name="planar">Whole 4-bpp characters.</param>
    internal MapTileCharacters(byte[] planar) : base(planar.Length / (MapTileAtlasFormat.ByteCount / MapTileAtlasFormat.CharacterCount)) =>
        this.planar = planar;

    /// <summary>The planar 4-bpp characters.</summary>
    public ReadOnlySpan<byte> Characters => planar;

    /// <summary>Compiles a 256-by-64 noninterlaced indexed PNG into 256 row-major 8-by-8 SNES 4-bpp characters, requiring pixel indices 0..15; PNG palette RGB values do not determine the runtime display palette.</summary>
    /// <param name="png">Caller-owned stream containing shared map artwork or the same-sized additional pause-menu atlas.</param>
    /// <returns>The immutable 8192-byte planar character atlas.</returns>
    /// <exception cref="InvalidDataException">The PNG is malformed, the wrong size or uses an index outside 4 bits.</exception>
    public static MapTileCharacters Read(Stream png)
    {
        var image = IndexedPng.Read(png, new(MapTileAtlasFormat.Width, MapTileAtlasFormat.Height));
        return new(SnesPlanarTileEncoder.Encode(image.Pixels, image.Size, 4));
    }

    /// <summary>The characters <paramref name="atlas"/> was read with.</summary>
    /// <exception cref="InvalidDataException">The atlas was not read from installed artwork.</exception>
    public static MapTileCharacters Of(MapTileAtlas atlas) => atlas as MapTileCharacters
        ?? throw new InvalidDataException("The map characters were not read from installed artwork.");
}

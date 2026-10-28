using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The installed map and menu sprites as the import pipeline reads them (#1409): their compositions, and the menu OBJ
/// characters the menu sprite sheets compose them over.
/// </summary>
public sealed class MapSpriteArtwork : MapSpriteCompositions
{
    /// <summary>Shared 4-bpp map object characters loaded into the current menu's OBJ region.</summary>
    internal readonly MapObjectTileArtwork characters;

    /// <summary>Retains validated compositions and the decoded indexed character atlas.</summary>
    /// <param name="compositions">The validated compositions.</param>
    /// <param name="characters">The decoded menu OBJ characters.</param>
    private MapSpriteArtwork(MapSpriteCompositions compositions, MapObjectTileArtwork characters) : base(compositions) =>
        this.characters = characters;

    /// <summary>The shared menu OBJ page the catalog's compositions draw from (#1358).</summary>
    public SpriteArtwork Page => field ??= characters.Page();

    /// <summary>Loads all 26 named sprite compositions and their indexed character atlas.</summary>
    /// <param name="json">The compositions document.</param>
    /// <param name="png">The indexed menu OBJ character atlas.</param>
    /// <exception cref="InvalidDataException">The document or the atlas is invalid.</exception>
    public static MapSpriteArtwork Read(Stream json, Stream png)
    {
        MapSpriteCompositions compositions = Load(json);
        var image = IndexedPng.Read(png, new(MapSpriteFormat.Width, MapSpriteFormat.Height));
        return new(compositions, new MapObjectTileArtwork(image));
    }

    /// <summary>The artwork <paramref name="catalog"/> was read with.</summary>
    /// <exception cref="InvalidDataException">The catalog was not read from installed artwork.</exception>
    public static MapSpriteArtwork Of(MapSpriteCompositions catalog) => catalog as MapSpriteArtwork
        ?? throw new InvalidDataException("The map sprites were not read from installed artwork.");
}

using System.Text.Json;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The installed map and menu sprites as the import pipeline reads them (#1409, #1463): the ordered compositions of the
/// map and menu sprites, whose palette inheritance retains live animation, and the menu OBJ characters the menu sprite
/// sheets compose them over. Both are kept as installed (#1615): every named composition holds its installed parts and
/// the character page holds the installed atlas's pixels whole.
/// </summary>
public sealed class MapSpriteArtwork : MapSpriteCompositions
{
    /// <summary>Every named map marker, elevator, indicator, label and title composition, as installed.</summary>
    private readonly Dictionary<MapSpriteId, SpriteComposition> frames;

    /// <summary>Retains the validated compositions and the installed menu OBJ characters.</summary>
    /// <param name="frames">The validated compositions, one per named role.</param>
    /// <param name="page">The installed menu OBJ characters.</param>
    private MapSpriteArtwork(Dictionary<MapSpriteId, SpriteComposition> frames, SpriteArtwork page)
    {
        this.frames = frames;
        Page = page;
    }

    /// <summary>The shared menu OBJ page the catalog's compositions draw from (#1358).</summary>
    public SpriteArtwork Page { get; }

    /// <summary>A named map marker, elevator, label, or indicator as a whole frame over the menu OBJ page (#1392).</summary>
    internal SpriteFrame Frame(MapSpriteId id)
    {
        string name = MapSpriteNames.Name(id);
        return Ensure.Listed(frames, id).ToFrame(name, 0);
    }

    /// <summary>Loads all 26 named sprite compositions and their indexed character atlas.</summary>
    /// <param name="json">The compositions document.</param>
    /// <param name="png">The indexed menu OBJ character atlas.</param>
    /// <exception cref="InvalidDataException">The document or the atlas is invalid.</exception>
    public static MapSpriteArtwork Read(Stream json, Stream png)
    {
        Dictionary<MapSpriteId, SpriteComposition> frames = Compile(json);
        var image = IndexedPng.Read(png, new(MapSpriteFormat.Width, MapSpriteFormat.Height));
        return new(frames, new SpriteArtwork(new SpriteArtworkId(SpriteArtworkKind.MenuObjects),
            SnesPlanarTileEncoder.Encode(image.Pixels, image.Size, 4)));
    }

    /// <summary>Loads all 26 named sprite compositions.</summary>
    /// <exception cref="InvalidDataException">The document is invalid or misses a named composition.</exception>
    private static Dictionary<MapSpriteId, SpriteComposition> Compile(Stream json)
    {
        MapSpriteDocument document;
        try { document = JsonAssetDocument.Read<MapSpriteDocument>(json, ArtworkJson.Options)
            ?? throw new InvalidDataException("Map sprite document is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid map sprite JSON.", error); }
        if (document.Version != MapSpriteFormat.Version || document.Frames is null || document.Frames.Count != MapSpriteFormat.FrameCount)
            throw new InvalidDataException("Map sprite content requires version 1 and all 26 named frames.");
        var frames = new Dictionary<MapSpriteId, SpriteComposition>();
        foreach (MapSpriteId id in MapSpriteFormat.FrameOrder)
        {
            string name = MapSpriteNames.Name(id);
            if (!document.Frames.TryGetValue(name, out var parts) || parts is null || parts.Length > MapSpriteFormat.MaximumParts)
                throw new InvalidDataException($"Map sprite {name} requires an ordered array of at most 128 parts.");
            frames.Add(id, MenuSpriteCompiler.Compile(parts, name));
        }
        return frames;
    }

    /// <summary>Enumerates the named map sprite roles in the installed document's order.</summary>
    public static IEnumerable<MapSpriteFrameName> FrameNames =>
        MapSpriteFormat.FrameOrder.Select(id => new MapSpriteFrameName(id, MapSpriteNames.Name(id)));

    /// <summary>The map sprite sheet's frames, one per named role.</summary>
    public static IEnumerable<string> SheetFrames => FrameNames.Select(frame => frame.Name);

    /// <summary>The artwork <paramref name="catalog"/> was read with.</summary>
    /// <exception cref="InvalidDataException">The catalog was not read from installed artwork.</exception>
    public static MapSpriteArtwork Of(MapSpriteCompositions catalog) => catalog as MapSpriteArtwork
        ?? throw new InvalidDataException("The map sprites were not read from installed artwork.");
}

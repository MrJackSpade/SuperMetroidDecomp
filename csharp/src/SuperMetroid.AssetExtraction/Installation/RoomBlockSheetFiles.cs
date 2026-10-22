using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>
/// Installs the pre-coloured room artwork (#1392): each tileset's block sheet and each library background's image, each
/// as an image, palette map and pictures, the tilemap words each background pictures, and the provenance of their
/// hashes. A player override of a sheet's or image's file replaces the stock one.
/// </summary>
public static class RoomBlockSheetFiles
{
    /// <summary>The provenance manifest's file name.</summary>
    public const string ProvenanceFileName = "room-block-sheets-provenance.json";

    /// <summary>The manifest naming each background image and the tilemap words it pictures.</summary>
    public const string BackgroundManifestFileName = "room-backgrounds.json";

    /// <summary>The background manifest's schema revision.</summary>
    private const int BackgroundManifestVersion = 1;

    /// <summary>The manifest naming each boss body sheet and where it pictures each tile word.</summary>
    public const string BodyManifestFileName = "boss-bodies.json";

    /// <summary>The body manifest's schema revision.</summary>
    private const int BodyManifestVersion = 1;

    /// <summary>Composes and writes every tileset's block sheet and background image, then validates the result.</summary>
    /// <param name="content">Installed stock content holding the characters, definitions, palettes, tilemaps and uploads the sheets compose from.</param>
    /// <param name="directory">Destination directory, created if absent.</param>
    /// <param name="sourceCartridgeSha256">Identity written into the provenance manifest.</param>
    public static void Extract(string content, string directory, string sourceCartridgeSha256)
    {
        RoomSheetComposition composition = RoomBlockSheetComposer.Compose(RoomBlockSheetInputs.Load(content));
        SpriteSheetStore store = SpriteSheetStore.Create(directory, ProvenanceFileName);
        foreach ((TilesetDefinition tileset, ComposedSpriteSheet sheet) in composition.BlockSheets)
            store.Write(RoomBlockSheet.NameOf(tileset), sheet);
        foreach ((string name, ComposedRoomBackground background) in composition.Backgrounds)
            store.Write(name, background.Sheet);
        foreach (ComposedBossBody body in composition.Bodies.Values)
            store.Write(BossBodySheet.NameOf(body.Body, body.Tileset), body.Sheet);
        store.Finish(sourceCartridgeSha256);
        var bodies = new BodyManifest(BodyManifestVersion, [.. composition.Bodies.Values.Select(body => new BodyDocument(body.Body,
            RoomBlockSheet.NameOf(body.Tileset), [.. body.Tiles.Select(tile => new BodyTileDocument(tile.Tile.Word.Raw, tile.Tile.Shown.Artwork.Name,
                tile.Tile.Shown.WordOffset, tile.Frame, tile.Corner.X, tile.Corner.Y))]))]);
        File.WriteAllBytes(Path.Combine(directory, BodyManifestFileName), JsonSerializer.SerializeToUtf8Bytes(bodies, JsonOptions));
        var manifest = new BackgroundManifest(BackgroundManifestVersion,
            composition.Backgrounds.ToDictionary(entry => entry.Key, entry => entry.Value.Words, StringComparer.Ordinal));
        File.WriteAllBytes(Path.Combine(directory, BackgroundManifestFileName), JsonSerializer.SerializeToUtf8Bytes(manifest, JsonOptions));
        _ = Load(directory, null);
    }

    /// <summary>
    /// Loads the block sheet of every tileset a retail room loads and every background image the manifest names,
    /// preferring a player override of each file.
    /// </summary>
    /// <exception cref="InvalidDataException">The manifest, a stock hash, an image or a picture manifest is invalid, or a sheet is not of blocks.</exception>
    public static RoomBlockSheetCatalog Load(string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, BackgroundManifestFileName);
        BackgroundManifest manifest = JsonSerializer.Deserialize<BackgroundManifest>(File.ReadAllBytes(manifestPath), JsonOptions)
            ?? throw new InvalidDataException($"Room background manifest {manifestPath} is empty.");
        if (manifest.Version != BackgroundManifestVersion || manifest.Words is null)
            throw new InvalidDataException($"Room background manifest {manifestPath} is not revision {BackgroundManifestVersion}.");
        string bodyPath = Path.Combine(stockDirectory, BodyManifestFileName);
        BodyManifest bodies = JsonSerializer.Deserialize<BodyManifest>(File.ReadAllBytes(bodyPath), JsonOptions)
            ?? throw new InvalidDataException($"Boss body manifest {bodyPath} is empty.");
        if (bodies.Version != BodyManifestVersion || bodies.Bodies is null)
            throw new InvalidDataException($"Boss body manifest {bodyPath} is not revision {BodyManifestVersion}.");
        IReadOnlyList<TilesetDefinition> tilesets = RoomBlockUploads.Tilesets();
        Dictionary<string, TilesetDefinition> byName = tilesets.ToDictionary(RoomBlockSheet.NameOf, StringComparer.Ordinal);
        SpriteSheetStore store = SpriteSheetStore.Open(stockDirectory, overrideDirectory, ProvenanceFileName,
            tilesets.Count + manifest.Words.Count + bodies.Bodies.Length);
        return new RoomBlockSheetCatalog(
            tilesets.ToDictionary(tileset => tileset, tileset => new RoomBlockSheet(tileset, store.Read(RoomBlockSheet.NameOf(tileset)))),
            manifest.Words.ToDictionary(entry => entry.Key, entry => new RoomBackgroundImage(entry.Key, store.Read(entry.Key), [.. entry.Value]),
                StringComparer.Ordinal),
            bodies.Bodies.Select(body => Body(body, byName.TryGetValue(body.Tileset, out TilesetDefinition tileset) ? tileset
                : throw new InvalidDataException($"Boss body manifest {bodyPath} names unknown tileset {body.Tileset}."), store))
                .ToDictionary(body => body.Tileset));
    }

    /// <summary>The body sheet <paramref name="document"/> describes, read from <paramref name="store"/>.</summary>
    private static BossBodySheet Body(BodyDocument document, TilesetDefinition tileset, SpriteSheetStore store) =>
        new(document.Body, tileset, store.Read(BossBodySheet.NameOf(document.Body, tileset)),
            document.Tiles.Select(tile => new BossBodyTile(new BgTileArtwork(new SnesBgTilemapWord(tile.Word),
                new BgArtworkShown(new BgArtworkIdentity(tile.Artwork), tile.Offset)), tile.Frame, new Position16<ImagePixels>(tile.X, tile.Y))));

    /// <summary>Used by installer completeness checks; overrides do not affect stock provenance.</summary>
    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);

    /// <summary>The background manifest: each image's name and the tilemap words it pictures.</summary>
    /// <param name="Version">The schema revision.</param>
    /// <param name="Words">Each image's tilemap words, page by page.</param>
    private sealed record BackgroundManifest(int Version, Dictionary<string, ushort[]> Words);

    /// <summary>The body manifest: each boss body sheet and where it pictures each tile word.</summary>
    /// <param name="Version">The schema revision.</param>
    /// <param name="Bodies">The bodies.</param>
    private sealed record BodyManifest(int Version, BodyDocument[] Bodies);

    /// <summary>One boss body sheet.</summary>
    /// <param name="Body">The boss.</param>
    /// <param name="Tileset">The installed name of the tileset of the boss's room.</param>
    /// <param name="Tiles">Where the sheet pictures each tile word over each artwork.</param>
    private sealed record BodyDocument(BossBody Body, string Tileset, BodyTileDocument[] Tiles);

    /// <summary>Where a body sheet pictures one tile word over one artwork.</summary>
    /// <param name="Word">The tile word.</param>
    /// <param name="Artwork">The artwork shown at the word's character.</param>
    /// <param name="Offset">The character's word offset within the artwork.</param>
    /// <param name="Frame">The frame picturing the word.</param>
    /// <param name="X">The tile's left pixel within the frame.</param>
    /// <param name="Y">The tile's top pixel within the frame.</param>
    private sealed record BodyTileDocument(ushort Word, string Artwork, int Offset, string Frame, ushort X, ushort Y);

    /// <summary>Stable camel-case manifest fields.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter() },
    };
}

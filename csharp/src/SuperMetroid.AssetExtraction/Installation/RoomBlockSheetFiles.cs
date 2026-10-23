using System.Collections.Immutable;
using System.Text.Json;
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

    /// <summary>The manifest naming each tile word sheet and where it pictures each tile word.</summary>
    public const string WordManifestFileName = "tile-words.json";

    /// <summary>The tile word manifest's schema revision.</summary>
    private const int WordManifestVersion = 1;

    /// <summary>The manifest naming each message box image and the words it pictures.</summary>
    public const string MessageManifestFileName = "message-boxes.json";

    /// <summary>The message box manifest's schema revision.</summary>
    private const int MessageManifestVersion = 1;

    /// <summary>The manifest naming each Ceres tileset's Mode 7 tiles and what each was pictured over.</summary>
    public const string Mode7ManifestFileName = "mode7-tiles.json";

    /// <summary>The Mode 7 manifest's schema revision.</summary>
    private const int Mode7ManifestVersion = 1;

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
        ComposedTileWordSheet[] wordSheets = [.. composition.TileWords.Values, .. composition.Bg3Words.Values];
        foreach (ComposedTileWordSheet words in wordSheets)
            store.Write(TileWordSheet.NameOf(words.Layer, words.Tileset), words.Sheet);
        foreach ((TilesetDefinition tileset, ComposedSpriteSheet messages) in composition.MessageBoxes)
            store.Write(MessageBoxSheet.NameOf(tileset), messages);
        foreach (ComposedMode7Tiles tiles in composition.FloorTiles.Values)
            store.Write(Mode7TileSheet.NameOf(tiles.Tileset), tiles.Sheet);
        store.Finish(sourceCartridgeSha256);
        File.WriteAllBytes(Path.Combine(directory, Mode7ManifestFileName), JsonSerializer.SerializeToUtf8Bytes(
            new Mode7Manifest(Mode7ManifestVersion, composition.FloorTiles.Values.ToDictionary(tiles => RoomBlockSheet.NameOf(tiles.Tileset),
                tiles => tiles.Shown.Select(shown => shown is { } bound ? new ShownDocument(bound.Artwork.Name, bound.WordOffset) : null).ToArray(),
                StringComparer.Ordinal)),
            JsonOptions));
        File.WriteAllBytes(Path.Combine(directory, MessageManifestFileName), JsonSerializer.SerializeToUtf8Bytes(
            new MessageManifest(MessageManifestVersion, composition.Messages.ToDictionary(entry => entry.Key, entry => entry.Value.ToArray(),
                StringComparer.Ordinal)), JsonOptions));
        var sheets = new WordManifest(WordManifestVersion, [.. wordSheets.Select(words => new WordSheetDocument(words.Layer,
            RoomBlockSheet.NameOf(words.Tileset), [.. words.Tiles.Select(tile => new WordTileDocument(tile.Tile.Word.Raw, tile.Tile.Shown.Artwork.Name,
                tile.Tile.Shown.WordOffset, tile.Frame, tile.Corner.X, tile.Corner.Y))]))]);
        File.WriteAllBytes(Path.Combine(directory, WordManifestFileName), JsonSerializer.SerializeToUtf8Bytes(sheets, JsonOptions));
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
        string wordPath = Path.Combine(stockDirectory, WordManifestFileName);
        WordManifest words = JsonSerializer.Deserialize<WordManifest>(File.ReadAllBytes(wordPath), JsonOptions)
            ?? throw new InvalidDataException($"Tile word manifest {wordPath} is empty.");
        if (words.Version != WordManifestVersion || words.Sheets is null)
            throw new InvalidDataException($"Tile word manifest {wordPath} is not revision {WordManifestVersion}.");
        string messagePath = Path.Combine(stockDirectory, MessageManifestFileName);
        MessageManifest messages = JsonSerializer.Deserialize<MessageManifest>(File.ReadAllBytes(messagePath), JsonOptions)
            ?? throw new InvalidDataException($"Message box manifest {messagePath} is empty.");
        if (messages.Version != MessageManifestVersion || messages.Words is null)
            throw new InvalidDataException($"Message box manifest {messagePath} is not revision {MessageManifestVersion}.");
        Dictionary<string, ImmutableArray<ushort>> messageWords = messages.Words.ToDictionary(entry => entry.Key,
            entry => ImmutableArray.Create(entry.Value), StringComparer.Ordinal);
        string mode7Path = Path.Combine(stockDirectory, Mode7ManifestFileName);
        Mode7Manifest mode7 = JsonSerializer.Deserialize<Mode7Manifest>(File.ReadAllBytes(mode7Path), JsonOptions)
            ?? throw new InvalidDataException($"Mode 7 manifest {mode7Path} is empty.");
        if (mode7.Version != Mode7ManifestVersion || mode7.Tiles is null)
            throw new InvalidDataException($"Mode 7 manifest {mode7Path} is not revision {Mode7ManifestVersion}.");
        IReadOnlyList<TilesetDefinition> tilesets = RoomBlockUploads.Tilesets();
        Dictionary<string, TilesetDefinition> byName = tilesets.ToDictionary(RoomBlockSheet.NameOf, StringComparer.Ordinal);
        SpriteSheetStore store = SpriteSheetStore.Open(stockDirectory, overrideDirectory, ProvenanceFileName,
            2 * tilesets.Count + manifest.Words.Count + words.Sheets.Length + mode7.Tiles.Count);
        return new RoomBlockSheetCatalog(
            tilesets.ToDictionary(tileset => tileset, tileset => new RoomBlockSheet(tileset, store.Read(RoomBlockSheet.NameOf(tileset)))),
            manifest.Words.ToDictionary(entry => entry.Key, entry => new RoomBackgroundImage(entry.Key, store.Read(entry.Key), [.. entry.Value]),
                StringComparer.Ordinal),
            Sheets(TileWordLayer.Bg12), Sheets(TileWordLayer.Bg3),
            tilesets.ToDictionary(tileset => tileset, tileset => new MessageBoxSheet(tileset, store.Read(MessageBoxSheet.NameOf(tileset)), messageWords)),
            mode7.Tiles.Select(entry => byName.TryGetValue(entry.Key, out TilesetDefinition tileset) ? new Mode7TileSheet(tileset,
                    store.Read(Mode7TileSheet.NameOf(tileset)), [.. entry.Value.Select(shown => shown is null ? (BgArtworkShown?)null
                        : new BgArtworkShown(new BgArtworkIdentity(shown.Artwork), shown.Offset))])
                : throw new InvalidDataException($"Mode 7 manifest {mode7Path} names unknown tileset {entry.Key}."))
                .ToDictionary(tiles => tiles.Tileset));

        // The installed sheets of one layer's words, each under its tileset.
        Dictionary<TilesetDefinition, TileWordSheet> Sheets(TileWordLayer layer) => words.Sheets.Where(sheet => sheet.Layer == layer)
            .Select(sheet => Words(sheet, byName.TryGetValue(sheet.Tileset, out TilesetDefinition tileset) ? tileset
                : throw new InvalidDataException($"Tile word manifest {wordPath} names unknown tileset {sheet.Tileset}."), store))
            .ToDictionary(sheet => sheet.Tileset);
    }

    /// <summary>The tile word sheet <paramref name="document"/> describes, read from <paramref name="store"/>.</summary>
    private static TileWordSheet Words(WordSheetDocument document, TilesetDefinition tileset, SpriteSheetStore store) =>
        new(document.Layer, tileset, store.Read(TileWordSheet.NameOf(document.Layer, tileset)),
            document.Tiles.Select(tile => new TileWordSheetTile(new BgTileArtwork(new SnesBgTilemapWord(tile.Word),
                new BgArtworkShown(new BgArtworkIdentity(tile.Artwork), tile.Offset)), tile.Frame, new Position16<ImagePixels>(tile.X, tile.Y))));

    /// <summary>Used by installer completeness checks; overrides do not affect stock provenance.</summary>
    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);

    /// <summary>The background manifest: each image's name and the tilemap words it pictures.</summary>
    /// <param name="Version">The schema revision.</param>
    /// <param name="Words">Each image's tilemap words, page by page.</param>
    private sealed record BackgroundManifest(int Version, Dictionary<string, ushort[]> Words);

    /// <summary>The Mode 7 manifest: what each Ceres tileset's tiles were pictured over, by tileset and tile.</summary>
    /// <param name="Version">The schema revision.</param>
    /// <param name="Tiles">Each tile's artwork, by tileset; null for a tile not pictured.</param>
    private sealed record Mode7Manifest(int Version, Dictionary<string, ShownDocument?[]> Tiles);

    /// <summary>What showed at a tile's characters.</summary>
    /// <param name="Artwork">The artwork.</param>
    /// <param name="Offset">The characters' word offset within it.</param>
    private sealed record ShownDocument(string Artwork, int Offset);

    /// <summary>The message box manifest: the words each message box image pictures, the same in every tileset's sheet.</summary>
    /// <param name="Version">The schema revision.</param>
    /// <param name="Words">Each image's words, by image name.</param>
    private sealed record MessageManifest(int Version, Dictionary<string, ushort[]> Words);

    /// <summary>The tile word manifest: each tile word sheet and where it pictures each tile word.</summary>
    /// <param name="Version">The schema revision.</param>
    /// <param name="Sheets">The sheets.</param>
    private sealed record WordManifest(int Version, WordSheetDocument[] Sheets);

    /// <summary>One tile word sheet.</summary>
    /// <param name="Layer">The backgrounds the words are written over.</param>
    /// <param name="Tileset">The installed name of the tileset whose rooms the words are written in.</param>
    /// <param name="Tiles">Where the sheet pictures each tile word over each artwork.</param>
    private sealed record WordSheetDocument(TileWordLayer Layer, string Tileset, WordTileDocument[] Tiles);

    /// <summary>Where a tile word sheet pictures one tile word over one artwork.</summary>
    /// <param name="Word">The tile word.</param>
    /// <param name="Artwork">The artwork shown at the word's character.</param>
    /// <param name="Offset">The character's word offset within the artwork.</param>
    /// <param name="Frame">The frame picturing the word.</param>
    /// <param name="X">The tile's left pixel within the frame.</param>
    /// <param name="Y">The tile's top pixel within the frame.</param>
    private sealed record WordTileDocument(ushort Word, string Artwork, int Offset, string Frame, ushort X, ushort Y);

    /// <summary>Stable camel-case manifest fields.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };
}

using SuperMetroid.Core.Assets;
using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Enemies;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>
/// Installs the pre-coloured room artwork (#1392, #1466): each tileset's block sheet, the appearance inputs each tileset's
/// blocks depend on, and each library background's image, each as an image, palette map and pictures, the tilemap words
/// each background pictures with the inputs its changing cells depend on, the written picture sheets with the inputs each
/// picture depends on, and the provenance of their hashes. A player override of a sheet's or image's file replaces the stock one.
/// </summary>
public static class RoomBlockSheetFiles
{
    /// <summary>The provenance manifest's file name.</summary>
    public const string ProvenanceFileName = "room-block-sheets-provenance.json";

    /// <summary>The manifest naming each background image and the tilemap words it pictures.</summary>
    public const string BackgroundManifestFileName = "room-backgrounds.json";

    /// <summary>The background manifest's schema revision: revision 2 records each image's changing cells (#1466).</summary>
    private const int BackgroundManifestVersion = 2;

    /// <summary>The manifest naming each written picture sheet and the inputs each of its pictures depends on.</summary>
    public const string WrittenManifestFileName = "written-pictures.json";

    /// <summary>
    /// The written picture manifest's schema revision: revision 1 names each picture by what is written and the values of
    /// its appearance inputs (#1466).
    /// </summary>
    private const int WrittenManifestVersion = 1;

    /// <summary>The manifest naming each message box image and the words it pictures.</summary>
    public const string MessageManifestFileName = "message-boxes.json";

    /// <summary>The message box manifest's schema revision.</summary>
    private const int MessageManifestVersion = 1;

    /// <summary>The manifest naming each Ceres tileset's Mode 7 tiles, what each was pictured over and the tileset's floor map.</summary>
    public const string Mode7ManifestFileName = "mode7-tiles.json";

    /// <summary>The Mode 7 manifest's schema revision: revision 2 adds each tileset's imported floor map (#1422).</summary>
    private const int Mode7ManifestVersion = 2;

    /// <summary>Composes and writes every tileset's block sheet and background image; the installer validates them with <see cref="ValidateStock"/>.</summary>
    /// <param name="content">Installed stock content holding the characters, definitions, palettes, tilemaps and uploads the sheets compose from.</param>
    /// <param name="directory">Destination directory, created if absent.</param>
    /// <param name="sourceCartridgeSha256">Identity written into the provenance manifest.</param>
    public static void Extract(string content, string directory, string sourceCartridgeSha256)
    {
        RoomSheetComposition composition = RoomBlockSheetComposer.Compose(RoomBlockSheetInputs.Load(content));
        SpriteSheetStore store = SpriteSheetStore.Create(directory, ProvenanceFileName);
        foreach ((TilesetDefinition tileset, RoomBlockAppearances appearances) in composition.BlockAppearances)
            RoomBlockAppearanceFiles.Write(directory, tileset, appearances);
        ComposedWrittenPictureSheet[] writtenSheets = [.. composition.Written.Values, .. composition.Bg3Pictures.Values];
        store.WriteAll([
            .. composition.BlockSheets.Select(entry => new NamedComposedSheet(RoomBlockSheet.NameOf(entry.Key), entry.Value)),
            .. composition.Backgrounds.Select(entry => new NamedComposedSheet(entry.Key, entry.Value.Sheet)),
            .. writtenSheets.Select(written => new NamedComposedSheet(WrittenPictureSheet.NameOf(written.Layer, written.Tileset), written.Sheet)),
            .. composition.MessageBoxes.Select(entry => new NamedComposedSheet(MessageBoxSheet.NameOf(entry.Key), entry.Value)),
            .. composition.FloorTiles.Values.Select(tiles => new NamedComposedSheet(FloorTileSheet.NameOf(tiles.Tileset), tiles.Sheet)),
        ]);
        store.Finish(sourceCartridgeSha256);
        File.WriteAllBytes(Path.Combine(directory, Mode7ManifestFileName), JsonSerializer.SerializeToUtf8Bytes(
            new Mode7Manifest(Mode7ManifestVersion, composition.FloorTiles.Values.ToDictionary(tiles => RoomBlockSheet.NameOf(tiles.Tileset),
                tiles => new FloorTilesDocument(
                    tiles.Shown.Select(shown => shown is not null).ToArray(),
                    tiles.Map),
                StringComparer.Ordinal)),
            JsonOptions));
        File.WriteAllBytes(Path.Combine(directory, MessageManifestFileName), JsonSerializer.SerializeToUtf8Bytes(
            new MessageManifest(MessageManifestVersion, composition.Messages.ToDictionary(entry => entry.Key, entry => BackgroundTileWords.ToWords(entry.Value.AsSpan()),
                StringComparer.Ordinal)), JsonOptions));
        var sheets = new WrittenManifest(WrittenManifestVersion, [.. writtenSheets.Select(written => new WrittenSheetDocument(written.Layer,
            RoomBlockSheet.NameOf(written.Tileset), [.. written.Inputs.OrderBy(entry => entry.Key, StringComparer.Ordinal)
                .Select(entry => new WrittenPictureDocument(entry.Key, [.. entry.Value.Select(input => input.Token)]))]))]);
        File.WriteAllBytes(Path.Combine(directory, WrittenManifestFileName), JsonSerializer.SerializeToUtf8Bytes(sheets, JsonOptions));
        var manifest = new BackgroundManifest(BackgroundManifestVersion,
            composition.Backgrounds.ToDictionary(entry => entry.Key, entry => entry.Value.Words, StringComparer.Ordinal),
            composition.Backgrounds.ToDictionary(entry => entry.Key, entry => entry.Value.Cells.Select(cell => new CellDocument(cell.Cell, cell.Picture,
                [.. cell.Inputs.Select(input => input.Token)])).ToArray(), StringComparer.Ordinal));
        File.WriteAllBytes(Path.Combine(directory, BackgroundManifestFileName), JsonSerializer.SerializeToUtf8Bytes(manifest, JsonOptions));
    }

    /// <summary>
    /// Loads the block sheet of every tileset a retail room loads and every background image the manifest names,
    /// preferring a player override of each file.
    /// </summary>
    /// <exception cref="InvalidDataException">The manifest, a stock hash, an image or a picture manifest is invalid, or a sheet is not of blocks.</exception>
    public static InstalledRoomArtwork Load(string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, BackgroundManifestFileName);
        BackgroundManifest manifest = JsonSerializer.Deserialize<BackgroundManifest>(File.ReadAllBytes(manifestPath), JsonOptions)
            ?? throw new InvalidDataException($"Room background manifest {manifestPath} is empty.");
        if (manifest.Version != BackgroundManifestVersion || manifest.Words is null || manifest.Cells is null)
            throw new InvalidDataException($"Room background manifest {manifestPath} is not revision {BackgroundManifestVersion}.");
        string writtenPath = Path.Combine(stockDirectory, WrittenManifestFileName);
        WrittenManifest written = JsonSerializer.Deserialize<WrittenManifest>(File.ReadAllBytes(writtenPath), JsonOptions)
            ?? throw new InvalidDataException($"Written picture manifest {writtenPath} is empty.");
        if (written.Version != WrittenManifestVersion || written.Sheets is null)
            throw new InvalidDataException($"Written picture manifest {writtenPath} is not revision {WrittenManifestVersion}.");
        string messagePath = Path.Combine(stockDirectory, MessageManifestFileName);
        MessageManifest messages = JsonSerializer.Deserialize<MessageManifest>(File.ReadAllBytes(messagePath), JsonOptions)
            ?? throw new InvalidDataException($"Message box manifest {messagePath} is empty.");
        if (messages.Version != MessageManifestVersion || messages.Words is null)
            throw new InvalidDataException($"Message box manifest {messagePath} is not revision {MessageManifestVersion}.");
        Dictionary<string, ImmutableArray<BackgroundTile>> messageTiles = messages.Words.ToDictionary(entry => entry.Key,
            entry => ImmutableArray.Create(BackgroundTileWords.ToTiles(entry.Value)), StringComparer.Ordinal);
        string mode7Path = Path.Combine(stockDirectory, Mode7ManifestFileName);
        Mode7Manifest mode7 = JsonSerializer.Deserialize<Mode7Manifest>(File.ReadAllBytes(mode7Path), JsonOptions)
            ?? throw new InvalidDataException($"Mode 7 manifest {mode7Path} is empty.");
        if (mode7.Version != Mode7ManifestVersion || mode7.Tiles is null)
            throw new InvalidDataException($"Mode 7 manifest {mode7Path} is not revision {Mode7ManifestVersion}.");
        IReadOnlyList<TilesetDefinition> tilesets = RoomBlockUploads.Tilesets();
        Dictionary<string, TilesetDefinition> byName = tilesets.ToDictionary(RoomBlockSheet.NameOf, StringComparer.Ordinal);
        SpriteSheetStore store = SpriteSheetStore.Open(stockDirectory, overrideDirectory, ProvenanceFileName,
            2 * tilesets.Count + manifest.Words.Count + written.Sheets.Length + mode7.Tiles.Count);
        // The inputs each block depends on are the stock record's: an override repaints a block's pictures, not what they
        // depend on.
        return new InstalledRoomArtwork(
            tilesets.ToDictionary(tileset => tileset, tileset => new RoomBlockSheet(tileset, store.Read(RoomBlockSheet.NameOf(tileset)),
                RoomBlockAppearanceFiles.Read(stockDirectory, tileset))),
            manifest.Words.ToDictionary(entry => entry.Key, entry => new RoomBackgroundImage(entry.Key, store.Read(entry.Key),
                    (manifest.Cells.TryGetValue(entry.Key, out CellDocument[]? cells) && cells is not null ? cells
                        : throw new InvalidDataException($"Room background manifest {manifestPath} records no cells of {entry.Key}."))
                    .Select(cell => new RoomBackgroundCell(cell.Cell, cell.Picture ?? throw new InvalidDataException($"Room background manifest {manifestPath} names no picture of a cell of {entry.Key}."),
                        Inputs(cell.Inputs)))),
                StringComparer.Ordinal),
            Sheets(WrittenPictureLayer.Bg12), Sheets(WrittenPictureLayer.Bg3),
            tilesets.ToDictionary(tileset => tileset, tileset => new MessageBoxSheet(tileset, store.Read(MessageBoxSheet.NameOf(tileset)), messageTiles)),
            mode7.Tiles.Select(entry => byName.TryGetValue(entry.Key, out TilesetDefinition tileset) ? new FloorTileSheet(tileset,
                    store.Read(FloorTileSheet.NameOf(tileset)), [.. entry.Value.Pictured ?? throw new InvalidDataException(
                        $"Mode 7 manifest {mode7Path} records no tiles for {entry.Key}.")],
                    entry.Value.Map ?? throw new InvalidDataException($"Mode 7 manifest {mode7Path} has no floor map for {entry.Key}."))
                : throw new InvalidDataException($"Mode 7 manifest {mode7Path} names unknown tileset {entry.Key}."))
                .ToDictionary(tiles => tiles.Tileset));

        // The installed sheets of one layer's written pictures, each under its tileset. The inputs each picture depends on
        // are the stock record's: an override repaints a sheet's pictures, not what they depend on.
        Dictionary<TilesetDefinition, WrittenPictureSheet> Sheets(WrittenPictureLayer layer) => written.Sheets.Where(sheet => sheet.Layer == layer)
            .Select(sheet => Written(sheet, byName.TryGetValue(sheet.Tileset, out TilesetDefinition tileset) ? tileset
                : throw new InvalidDataException($"Written picture manifest {writtenPath} names unknown tileset {sheet.Tileset}."), store))
            .ToDictionary(sheet => sheet.Tileset);
    }

    /// <summary>The written picture sheet <paramref name="document"/> describes, read from <paramref name="store"/>.</summary>
    private static WrittenPictureSheet Written(WrittenSheetDocument document, TilesetDefinition tileset, SpriteSheetStore store) =>
        new(document.Layer, tileset, store.Read(WrittenPictureSheet.NameOf(document.Layer, tileset)),
            (document.Pictures ?? throw new InvalidDataException($"Written picture sheet {WrittenPictureSheet.NameOf(document.Layer, tileset)} records no pictures."))
                .ToDictionary(picture => picture.Picture ?? throw new InvalidDataException(
                    $"Written picture sheet {WrittenPictureSheet.NameOf(document.Layer, tileset)} records a picture with no name."),
                    picture => Inputs(picture.Inputs), StringComparer.Ordinal));

    /// <summary>The inputs <paramref name="tokens"/> spell, as the manifests record them.</summary>
    /// <exception cref="InvalidDataException">The tokens are missing or spell an unknown input.</exception>
    private static ImmutableArray<AppearanceInput> Inputs(string[]? tokens) =>
        [.. (tokens ?? throw new InvalidDataException("A manifest entry records no appearance inputs."))
            .Select(token => AppearanceInputTokens.Parse(token, EncounterCharacterPlacements.Catalog.LookNamed))];

    /// <summary>Used by installer completeness checks; overrides do not affect stock provenance.</summary>
    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);

    /// <summary>The background manifest: each image's name, the tilemap words it pictures and its changing cells.</summary>
    /// <param name="Version">The schema revision.</param>
    /// <param name="Words">Each image's tilemap words, page by page.</param>
    /// <param name="Cells">Each image's cells whose pictures change with appearance inputs.</param>
    private sealed record BackgroundManifest(int Version, Dictionary<string, ushort[]> Words, Dictionary<string, CellDocument[]> Cells);

    /// <summary>One cell of a background image whose picture changes with appearance inputs.</summary>
    /// <param name="Cell">The cell.</param>
    /// <param name="Picture">The name its pictures in the block sheet begin with.</param>
    /// <param name="Inputs">The inputs it depends on, as their tokens spell them.</param>
    private sealed record CellDocument(int Cell, string? Picture, string[]? Inputs);

    /// <summary>The Mode 7 manifest: what each Ceres tileset's tiles were pictured over and the tileset's floor map, by tileset.</summary>
    /// <param name="Version">The schema revision.</param>
    /// <param name="Tiles">Each tileset's tiles and floor map, by its installed name.</param>
    private sealed record Mode7Manifest(int Version, Dictionary<string, FloorTilesDocument> Tiles);

    /// <summary>One Ceres tileset's floor tiles and floor map.</summary>
    /// <param name="Pictured">Whether the sheet pictures each tile, by tile.</param>
    /// <param name="Map">The floor map a room of the tileset starts from: 16384 tile numbers, row-major.</param>
    private sealed record FloorTilesDocument(bool[]? Pictured, byte[] Map);

    /// <summary>The message box manifest: the words each message box image pictures, the same in every tileset's sheet.</summary>
    /// <param name="Version">The schema revision.</param>
    /// <param name="Words">Each image's words, by image name.</param>
    private sealed record MessageManifest(int Version, Dictionary<string, ushort[]> Words);

    /// <summary>The written picture manifest: each written picture sheet and the inputs each of its pictures depends on.</summary>
    /// <param name="Version">The schema revision.</param>
    /// <param name="Sheets">The sheets.</param>
    private sealed record WrittenManifest(int Version, WrittenSheetDocument[] Sheets);

    /// <summary>One written picture sheet.</summary>
    /// <param name="Layer">The backgrounds the pictures are written over.</param>
    /// <param name="Tileset">The installed name of the tileset whose rooms the pictures are written in.</param>
    /// <param name="Pictures">Each picture the sheet holds and the inputs it depends on.</param>
    private sealed record WrittenSheetDocument(WrittenPictureLayer Layer, string Tileset, WrittenPictureDocument[]? Pictures);

    /// <summary>One picture a written picture sheet holds, by the name its frames begin with.</summary>
    /// <param name="Picture">The name.</param>
    /// <param name="Inputs">The inputs it depends on, as their tokens spell them.</param>
    private sealed record WrittenPictureDocument(string? Picture, string[]? Inputs);

    /// <summary>Stable camel-case manifest fields.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };
}

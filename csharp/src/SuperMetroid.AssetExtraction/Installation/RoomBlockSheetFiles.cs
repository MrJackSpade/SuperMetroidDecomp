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
/// each background pictures with the inputs its changing cells depend on, the tile word sheets with the inputs each word
/// depends on, and the provenance of their hashes. A player override of a sheet's or image's file replaces the stock one.
/// </summary>
public static class RoomBlockSheetFiles
{
    /// <summary>The provenance manifest's file name.</summary>
    public const string ProvenanceFileName = "room-block-sheets-provenance.json";

    /// <summary>The manifest naming each background image and the tilemap words it pictures.</summary>
    public const string BackgroundManifestFileName = "room-backgrounds.json";

    /// <summary>The background manifest's schema revision: revision 2 records each image's changing cells (#1466).</summary>
    private const int BackgroundManifestVersion = 2;

    /// <summary>The manifest naming each tile word sheet and where it pictures each tile word.</summary>
    public const string WordManifestFileName = "tile-words.json";

    /// <summary>The tile word manifest's schema revision: revision 2 names each picture by its words' appearance inputs (#1466).</summary>
    private const int WordManifestVersion = 2;

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
        ComposedTileWordSheet[] wordSheets = [.. composition.TileWords.Values, .. composition.Bg3Words.Values];
        store.WriteAll([
            .. composition.BlockSheets.Select(entry => new NamedComposedSheet(RoomBlockSheet.NameOf(entry.Key), entry.Value)),
            .. composition.Backgrounds.Select(entry => new NamedComposedSheet(entry.Key, entry.Value.Sheet)),
            .. wordSheets.Select(words => new NamedComposedSheet(TileWordSheet.NameOf(words.Layer, words.Tileset), words.Sheet)),
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
        var sheets = new WordManifest(WordManifestVersion, [.. wordSheets.Select(words => new WordSheetDocument(words.Layer,
            RoomBlockSheet.NameOf(words.Tileset), [.. words.Tiles.Select(tile => new WordTileDocument(BackgroundTileWords.ToWord(tile.Tile.Tile), tile.Tile.Appearance,
                tile.Frame, tile.Corner.X, tile.Corner.Y))],
            [.. words.Inputs.OrderBy(entry => BackgroundTileWords.ToWord(entry.Key)).Select(entry => new WordInputsDocument(BackgroundTileWords.ToWord(entry.Key),
                [.. entry.Value.Select(input => input.Token)]))]))]);
        File.WriteAllBytes(Path.Combine(directory, WordManifestFileName), JsonSerializer.SerializeToUtf8Bytes(sheets, JsonOptions));
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
        string wordPath = Path.Combine(stockDirectory, WordManifestFileName);
        WordManifest words = JsonSerializer.Deserialize<WordManifest>(File.ReadAllBytes(wordPath), JsonOptions)
            ?? throw new InvalidDataException($"Tile word manifest {wordPath} is empty.");
        if (words.Version != WordManifestVersion || words.Sheets is null)
            throw new InvalidDataException($"Tile word manifest {wordPath} is not revision {WordManifestVersion}.");
        string mode7Path = Path.Combine(stockDirectory, Mode7ManifestFileName);
        Mode7Manifest mode7 = JsonSerializer.Deserialize<Mode7Manifest>(File.ReadAllBytes(mode7Path), JsonOptions)
            ?? throw new InvalidDataException($"Mode 7 manifest {mode7Path} is empty.");
        if (mode7.Version != Mode7ManifestVersion || mode7.Tiles is null)
            throw new InvalidDataException($"Mode 7 manifest {mode7Path} is not revision {Mode7ManifestVersion}.");
        IReadOnlyList<TilesetDefinition> tilesets = RoomBlockUploads.Tilesets();
        Dictionary<string, TilesetDefinition> byName = tilesets.ToDictionary(RoomBlockSheet.NameOf, StringComparer.Ordinal);
        SpriteSheetStore store = SpriteSheetStore.Open(stockDirectory, overrideDirectory, ProvenanceFileName,
            2 * tilesets.Count + manifest.Words.Count + words.Sheets.Length + mode7.Tiles.Count);
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
            Sheets(TileWordLayer.Bg12), Sheets(TileWordLayer.Bg3),
            tilesets.ToDictionary(tileset => tileset, tileset => new MessageBoxSheet(tileset, store.Read(MessageBoxSheet.NameOf(tileset)))),
            mode7.Tiles.Select(entry => byName.TryGetValue(entry.Key, out TilesetDefinition tileset) ? new FloorTileSheet(tileset,
                    store.Read(FloorTileSheet.NameOf(tileset)), [.. entry.Value.Pictured ?? throw new InvalidDataException(
                        $"Mode 7 manifest {mode7Path} records no tiles for {entry.Key}.")],
                    entry.Value.Map ?? throw new InvalidDataException($"Mode 7 manifest {mode7Path} has no floor map for {entry.Key}."))
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
            document.Tiles.Select(tile => new TileWordSheetTile(new TileWordAppearance(BackgroundTileWords.ToTile(checked((ushort)tile.Word)),
                tile.Appearance ?? ""), tile.Frame, new Position16<ImagePixels>(checked((ushort)tile.X), checked((ushort)tile.Y)))),
            (document.Inputs ?? throw new InvalidDataException($"Tile word sheet {TileWordSheet.NameOf(document.Layer, tileset)} records no word inputs."))
                .ToDictionary(word => BackgroundTileWords.ToTile(checked((ushort)word.Word)), word => Inputs(word.Inputs)));

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

    /// <summary>The tile word manifest: each tile word sheet and where it pictures each tile word.</summary>
    /// <param name="Version">The schema revision.</param>
    /// <param name="Sheets">The sheets.</param>
    private sealed record WordManifest(int Version, WordSheetDocument[] Sheets);

    /// <summary>One tile word sheet.</summary>
    /// <param name="Layer">The backgrounds the words are written over.</param>
    /// <param name="Tileset">The installed name of the tileset whose rooms the words are written in.</param>
    /// <param name="Tiles">Where the sheet pictures each tile word for each value of its inputs.</param>
    /// <param name="Inputs">The inputs each tile word that depends on any depends on.</param>
    private sealed record WordSheetDocument(TileWordLayer Layer, string Tileset, WordTileDocument[] Tiles, WordInputsDocument[]? Inputs);

    /// <summary>Where a tile word sheet pictures one tile word for one set of values of its inputs.</summary>
    /// <param name="Word">The tile word.</param>
    /// <param name="Appearance">The values of its inputs, as a picture's name spells them; empty for a word that depends on none.</param>
    /// <param name="Frame">The frame picturing the word.</param>
    /// <param name="X">The tile's left pixel within the frame.</param>
    /// <param name="Y">The tile's top pixel within the frame.</param>
    [SpatialComponents("External format: the installed tile word manifest stores each tile's frame corner as x/y.")]
    private sealed record WordTileDocument(int Word, string? Appearance, string Frame, int X, int Y);

    /// <summary>The inputs one tile word depends on.</summary>
    /// <param name="Word">The tile word.</param>
    /// <param name="Inputs">The inputs, as their tokens spell them.</param>
    private sealed record WordInputsDocument(int Word, string[]? Inputs);

    /// <summary>Stable camel-case manifest fields.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };
}

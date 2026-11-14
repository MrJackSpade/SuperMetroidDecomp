using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Hud;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>
/// Installs the pre-coloured room artwork (#1392, #1466): each tileset's block sheet, the appearance inputs each tileset's
/// blocks depend on, and each library background's image, each as a sheet manifest whose pictures the installation's picture
/// store holds (#1635), each background's size with the inputs its changing cells depend on (#1708), the written
/// picture sheets with the inputs each picture depends on, each tileset's sheet of the HUD's pictures (#1731), the turning
/// floor of each tileset whose rooms show one (#1467, #1649); the tilesets and those manifests name the sheets (#1693). A
/// player edits a sheet's or image's installed file in place.
/// </summary>
public static class RoomBlockSheetFiles
{
    /// <summary>The manifest naming each background image, its size and its changing cells.</summary>
    public const string BackgroundManifestFileName = "room-backgrounds.json";

    /// <summary>
    /// The background manifest's schema revision: revision 2 records each image's changing cells (#1466); revision 3 no
    /// longer pictures a cell over a collectible's graphics (#1513); revision 4 installs only the library backgrounds the
    /// player sees as their own pictures, none of the boss rooms', the sky rooms' or the cleared ones (#1649); revision 5
    /// records one period of what each library background shows, its one page where its two pages hold one tilemap, which
    /// the room shows repeated across and down (#1651); revision 8 records each image's size in place of the tilemap words
    /// it pictures, which the game only counted (#1708); revision 9 names each changing cell's pictures by its image and its
    /// place in it (#1717); revision 10 names each library background by what it shows and where, not by the cartridge's
    /// tileset numbers (#1720).
    /// </summary>
    private const int BackgroundManifestVersion = 10;

    /// <summary>The manifest naming each written picture sheet and the inputs each of its pictures depends on.</summary>
    public const string WrittenManifestFileName = "written-pictures.json";

    /// <summary>
    /// The written picture manifest's schema revision: revision 1 names each picture by what is written and the values of
    /// its appearance inputs (#1466); revision 2 no longer pictures the cell the effect area is cleared to (#1514); revision
    /// 3 names an encounter's look input by the encounter and the look (#1538); revision 4 installs the effect overlay's
    /// sheets as <c>OverlayPictures.</c> (#1601); revision 5 installs each liquid's surface strip and body and each
    /// atmosphere's repeating picture in place of the effects' tilemap pages (#1706); revision 6 names each sheet's layer by
    /// name, installs the effect overlay's sheets as <c>RoomEffects.</c> and no longer records the HUD's pictures, which are
    /// a sheet of their own, <c>Hud.</c> (#1731); revision 7 names a block animation's frames by its
    /// block animation, not by the hardware's tiles (#1726).
    /// </summary>
    private const int WrittenManifestVersion = 7;

    /// <summary>The manifest naming each tileset whose turning floor is installed: each whose rooms show one (#1649).</summary>
    public const string FloorManifestFileName = "room-floors.json";

    /// <summary>
    /// The floor manifest's schema revision: revision 1 names the tilesets whose floors are pictures (#1467); revision 2
    /// names only the tilesets whose rooms show a turning floor, each floor pictured as the part of it that shows anything,
    /// placed by its origin (#1649).
    /// </summary>
    private const int FloorManifestVersion = 2;

    /// <summary>
    /// Composes every tileset's block sheet and background image and writes the manifests of what they picture; the sheets
    /// are returned for the picture store to write (<see cref="PictureStoreFiles"/>, #1635).
    /// </summary>
    /// <param name="content">Installed stock content holding the characters, definitions, palettes, tilemaps and uploads the sheets compose from.</param>
    /// <param name="directory">The family's directory, created if absent.</param>
    internal static InstalledSheetFamily Extract(string content, string directory)
    {
        RoomSheetComposition composition = RoomBlockSheetComposer.Compose(RoomBlockSheetInputs.Load(content), RoomBlockInventory.Load(content));
        Directory.CreateDirectory(directory);
        foreach ((TilesetDefinition tileset, RoomBlockAppearances appearances) in composition.BlockAppearances)
            RoomBlockAppearanceFiles.Write(directory, tileset, appearances);
        ComposedWrittenPictureSheet[] writtenSheets = [.. composition.Written.Values, .. composition.RoomEffects.Values];
        File.WriteAllBytes(Path.Combine(directory, FloorManifestFileName), JsonSerializer.SerializeToUtf8Bytes(
            new FloorManifest(FloorManifestVersion, [.. composition.Floors.Keys.Select(RoomBlockSheet.NameOf).Order(StringComparer.Ordinal)]),
            JsonOptions));
        var sheets = new WrittenManifest(WrittenManifestVersion, [.. writtenSheets.Select(written => new WrittenSheetDocument(written.Layer,
            RoomBlockSheet.NameOf(written.Tileset), [.. written.Inputs.OrderBy(entry => entry.Key, StringComparer.Ordinal)
                .Select(entry => new WrittenPictureDocument(entry.Key, [.. entry.Value.Select(input => input.Token)]))]))]);
        File.WriteAllBytes(Path.Combine(directory, WrittenManifestFileName), JsonSerializer.SerializeToUtf8Bytes(sheets, JsonOptions));
        var manifest = new BackgroundManifest(BackgroundManifestVersion,
            composition.Backgrounds.ToDictionary(entry => entry.Key, entry => entry.Value.Size, StringComparer.Ordinal),
            composition.Backgrounds.ToDictionary(entry => entry.Key, entry => entry.Value.Cells.Select(cell => new CellDocument(cell.Cell, cell.Picture,
                [.. cell.Inputs.Select(input => input.Token)])).ToArray(), StringComparer.Ordinal));
        File.WriteAllBytes(Path.Combine(directory, BackgroundManifestFileName), JsonSerializer.SerializeToUtf8Bytes(manifest, JsonOptions));
        return InstalledSheetFamily.Install(directory, [
            .. composition.BlockSheets.Select(entry => new NamedComposedSheet(RoomBlockSheet.NameOf(entry.Key), entry.Value)),
            .. composition.Backgrounds.Select(entry => new NamedComposedSheet(entry.Key, entry.Value.Sheet)),
            .. writtenSheets.Select(written => new NamedComposedSheet(WrittenPictureSheet.NameOf(written.Layer, written.Tileset), written.Sheet)),
            .. composition.Hud.Select(entry => new NamedComposedSheet(HudPictureSheet.NameOf(entry.Key), entry.Value)),
            .. composition.MessageBoxes.Select(entry => new NamedComposedSheet(MessageBoxSheet.NameOf(entry.Key), entry.Value)),
            .. composition.Floors.Select(entry => new NamedComposedSheet(RoomFloorSheet.NameOf(entry.Key), entry.Value)),
        ]);
    }

    /// <summary>
    /// Loads the installed block sheet of every tileset a retail room loads and every background image the manifest names,
    /// as the player left each file.
    /// </summary>
    /// <exception cref="InvalidDataException">The manifest, an image or a picture manifest is invalid, or a sheet is not of blocks.</exception>
    public static InstalledRoomArtwork Load(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string manifestPath = Path.Combine(directory, BackgroundManifestFileName);
        BackgroundManifest manifest = JsonSerializer.Deserialize<BackgroundManifest>(File.ReadAllBytes(manifestPath), JsonOptions)
            ?? throw new InvalidDataException($"Room background manifest {manifestPath} is empty.");
        if (manifest.Version != BackgroundManifestVersion || manifest.Sizes is null || manifest.Cells is null)
            throw new InvalidDataException($"Room background manifest {manifestPath} is not revision {BackgroundManifestVersion}.");
        string writtenPath = Path.Combine(directory, WrittenManifestFileName);
        WrittenManifest written = JsonSerializer.Deserialize<WrittenManifest>(File.ReadAllBytes(writtenPath), JsonOptions)
            ?? throw new InvalidDataException($"Written picture manifest {writtenPath} is empty.");
        if (written.Version != WrittenManifestVersion || written.Sheets is null)
            throw new InvalidDataException($"Written picture manifest {writtenPath} is not revision {WrittenManifestVersion}.");
        string floorPath = Path.Combine(directory, FloorManifestFileName);
        FloorManifest floors = JsonSerializer.Deserialize<FloorManifest>(File.ReadAllBytes(floorPath), JsonOptions)
            ?? throw new InvalidDataException($"Floor manifest {floorPath} is empty.");
        if (floors.Version != FloorManifestVersion || floors.Tilesets is null)
            throw new InvalidDataException($"Floor manifest {floorPath} is not revision {FloorManifestVersion}.");
        IReadOnlyList<TilesetDefinition> tilesets = RoomBlockUploads.Tilesets();
        Dictionary<string, TilesetDefinition> byName = tilesets.ToDictionary(RoomBlockSheet.NameOf, StringComparer.Ordinal);
        RequireRetailSelections(directory, tilesets, manifest, written, floors);
        SpriteSheetStore store = SpriteSheetStore.Open(directory);
        // The inputs each block depends on are the extraction's record: an edit repaints a block's pictures, not what they
        // depend on.
        return new InstalledRoomArtwork(
            tilesets.ToDictionary(tileset => tileset, tileset => new RoomBlockSheet(tileset, store.Read(RoomBlockSheet.NameOf(tileset)),
                RoomBlockAppearanceFiles.Read(directory, tileset))),
            manifest.Sizes.ToDictionary(entry => entry.Key, entry => new RoomBackgroundImage(entry.Key, store.Read(entry.Key),
                    entry.Value,
                    (manifest.Cells.TryGetValue(entry.Key, out CellDocument[]? cells) && cells is not null ? cells
                        : throw new InvalidDataException($"Room background manifest {manifestPath} records no cells of {entry.Key}."))
                    .Select(cell => new RoomBackgroundCell(cell.Cell, cell.Picture ?? throw new InvalidDataException($"Room background manifest {manifestPath} names no picture of a cell of {entry.Key}."),
                        Inputs(cell.Inputs)))),
                StringComparer.Ordinal),
            Sheets(WrittenPictureLayer.WrittenOverRoom), Sheets(WrittenPictureLayer.FxOverlay),
            tilesets.ToDictionary(tileset => tileset, tileset => new HudPictureSheet(tileset, store.Read(HudPictureSheet.NameOf(tileset)))),
            tilesets.ToDictionary(tileset => tileset, tileset => new MessageBoxSheet(tileset, store.Read(MessageBoxSheet.NameOf(tileset)), GameplayMessageArtwork.Forms())),
            floors.Tilesets.Select(name => byName.TryGetValue(name, out TilesetDefinition tileset)
                    ? new RoomFloorSheet(tileset, store.Read(RoomFloorSheet.NameOf(tileset)))
                    : throw new InvalidDataException($"Floor manifest {floorPath} names unknown tileset {name}."))
                .ToDictionary(floor => floor.Tileset));

        // The installed sheets of one layer's written pictures, each under its tileset. The inputs each picture depends on
        // are the extraction's record: an edit repaints a sheet's pictures, not what they depend on.
        Dictionary<TilesetDefinition, WrittenPictureSheet> Sheets(WrittenPictureLayer layer) => written.Sheets.Where(sheet => sheet.Layer == layer)
            .Select(sheet => Written(sheet, byName.TryGetValue(sheet.Tileset, out TilesetDefinition tileset) ? tileset
                : throw new InvalidDataException($"Written picture manifest {writtenPath} names unknown tileset {sheet.Tileset}."), store))
            .ToDictionary(sheet => sheet.Tileset);
    }

    /// <summary>
    /// Requires the manifests to install the room artwork every retail room state can select (#1536), so that no lookup a
    /// room makes by its tileset, background, sky or effect finds nothing: the block sheet and message boxes of every
    /// tileset are read for each of <paramref name="tilesets"/> itself; this requires an effect overlay picture sheet of each
    /// of them with the artwork of every effect its rooms show (#1706), the turning floor of every tileset whose rooms show one, and every background
    /// image a room state shows (<see cref="RoomBlockUploads.BackgroundNames"/>: none of a library background the player sees
    /// as something else, #1649).
    /// </summary>
    /// <exception cref="InvalidDataException">A manifest installs none of one of them.</exception>
    private static void RequireRetailSelections(string directory, IReadOnlyList<TilesetDefinition> tilesets, BackgroundManifest backgrounds,
        WrittenManifest written, FloorManifest floors)
    {
        string backgroundPath = Path.Combine(directory, BackgroundManifestFileName);
        string writtenPath = Path.Combine(directory, WrittenManifestFileName);
        string floorPath = Path.Combine(directory, FloorManifestFileName);
        foreach (string name in RoomBlockUploads.BackgroundNames())
        {
            if (!backgrounds.Sizes.ContainsKey(name))
                throw new InvalidDataException($"Room background manifest {backgroundPath} names no image {name}, which a retail room state shows.");
        }
        foreach (TilesetDefinition tileset in tilesets)
        {
            string installed = RoomBlockSheet.NameOf(tileset);
            WrittenSheetDocument overlay = written.Sheets.FirstOrDefault(sheet => sheet.Layer == WrittenPictureLayer.FxOverlay && sheet.Tileset == installed)
                ?? throw new InvalidDataException($"Written picture manifest {writtenPath} names no effect overlay picture sheet of {installed}, whose rooms show one.");
            WrittenPictureDocument[] pictures = overlay.Pictures
                ?? throw new InvalidDataException($"Written picture sheet {WrittenPictureSheet.NameOf(overlay.Layer, tileset)} records no pictures.");
            foreach (RoomFxType effect in RoomBlockUploads.OverlayEffectsOf(tileset))
            {
                foreach (string name in RoomEffectPictures.NamesOf(effect))
                {
                    if (!pictures.Any(picture => picture.Picture == name))
                        throw new InvalidDataException($"Written picture sheet {WrittenPictureSheet.NameOf(overlay.Layer, tileset)} pictures no {name}, which a room of {installed} shows.");
                }
            }
        }
        string[] floorTilesets = floors.Tilesets ?? throw new InvalidDataException($"Floor manifest {floorPath} names no tilesets.");
        foreach (TilesetDefinition tileset in RoomBlockUploads.FloorTilesets())
        {
            if (!floorTilesets.Contains(RoomBlockSheet.NameOf(tileset), StringComparer.Ordinal))
                throw new InvalidDataException($"Floor manifest {floorPath} names no floor of tileset {RoomBlockSheet.NameOf(tileset)}, whose rooms show one.");
        }
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
            .Select(AppearanceInputSpellings.Parse)];

    /// <summary>The background manifest: each image's name, its size and its changing cells.</summary>
    /// <param name="Version">The schema revision.</param>
    /// <param name="Sizes">Each image's width and height, which a part of its picture that shows nothing does not record (#1634): one period of what it shows (#1651), laid out as its room shows it (#1617).</param>
    /// <param name="Cells">Each image's cells whose pictures change with appearance inputs.</param>
    private sealed record BackgroundManifest(int Version, Dictionary<string, Extent16<ImagePixels>> Sizes, Dictionary<string, CellDocument[]> Cells);

    /// <summary>One cell of a background image whose picture changes with appearance inputs.</summary>
    /// <param name="Cell">The cell.</param>
    /// <param name="Picture">The name its pictures in the block sheet begin with.</param>
    /// <param name="Inputs">The inputs it depends on, as their tokens spell them.</param>
    private sealed record CellDocument(int Cell, string? Picture, string[]? Inputs);

    /// <summary>The floor manifest: the Ceres tilesets whose turning floors are installed.</summary>
    /// <param name="Version">The schema revision.</param>
    /// <param name="Tilesets">Each tileset's installed name.</param>
    private sealed record FloorManifest(int Version, string[]? Tilesets);

    /// <summary>The written picture manifest: each written picture sheet and the inputs each of its pictures depends on.</summary>
    /// <param name="Version">The schema revision.</param>
    /// <param name="Sheets">The sheets.</param>
    private sealed record WrittenManifest(int Version, WrittenSheetDocument[] Sheets);

    /// <summary>One written picture sheet.</summary>
    /// <param name="Layer">The display layer the pictures show on, by its name.</param>
    /// <param name="Tileset">The installed name of the tileset whose rooms the pictures are written in.</param>
    /// <param name="Pictures">Each picture the sheet holds and the inputs it depends on.</param>
    private sealed record WrittenSheetDocument(WrittenPictureLayer Layer, string Tileset, WrittenPictureDocument[]? Pictures);

    /// <summary>One picture a written picture sheet holds, by the name its frames begin with.</summary>
    /// <param name="Picture">The name.</param>
    /// <param name="Inputs">The inputs it depends on, as their tokens spell them.</param>
    private sealed record WrittenPictureDocument(string? Picture, string[]? Inputs);

    /// <summary>Stable camel-case manifest fields; a written picture sheet's layer is named, never numbered (#1731).</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter<WrittenPictureLayer>(allowIntegerValues: false) },
    };
}

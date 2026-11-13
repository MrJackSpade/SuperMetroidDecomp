using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>
/// Installs the pre-assembled boss body sheets (#1466, #1515, #1540, #1635): each boss's sheet manifest, its pictures held in
/// the installation's picture store, the manifest naming each frame's whole picture and where it lies on the body's area,
/// and the provenance of their hashes. A player edits a sheet's installed files in place.
/// </summary>
public static class BossBodySheetFiles
{
    /// <summary>The provenance manifest's file name.</summary>
    public const string ProvenanceFileName = "boss-body-sheets-provenance.json";

    /// <summary>The manifest naming each boss's frames.</summary>
    public const string ManifestFileName = "boss-bodies.json";

    /// <summary>
    /// The manifest's schema revision: 6 names each frame's one whole picture, where it lies on the body's area and the
    /// depth it is drawn at (#1634), a part drawn over the parts before it with no rectangles it covers (#1540), Kraid's body
    /// below his head and the head it rises with as one frame each; with no cell footprints, blank (#1515), pieces of
    /// pictures laid over a frame (#1513) or frames pictured again to be drawn in front of the sprites; 7 pictures each of
    /// Crocomire's melt images once, with the cells its dissolve erases, and no step of it (#1652); 8 keys each frame by the
    /// animation segment or table key that first shows it (#1623).
    /// </summary>
    private const int ManifestVersion = 9;

    /// <summary>
    /// Composes every boss's body sheet and writes their manifest; the sheets are returned for the picture store to write
    /// (<see cref="PictureStoreFiles"/>, #1635).
    /// </summary>
    /// <param name="content">Installed stock content holding the frames, characters and palettes the sheets compose from.</param>
    /// <param name="directory">The family's directory, created if absent.</param>
    internal static InstalledSheetFamily Extract(string content, string directory) =>
        Write(BossBodySheetComposer.Compose(RoomBlockSheetInputs.Load(content)), directory);

    /// <summary>
    /// Writes the manifest of composed <paramref name="sheets"/> and gives the sheets as the installation holds them, which
    /// <see cref="Load"/> reads once the picture store has written them (#1635).
    /// </summary>
    /// <param name="sheets">Every boss's composed body sheet.</param>
    /// <param name="directory">The family's directory, created if absent.</param>
    internal static InstalledSheetFamily Write(IReadOnlyList<ComposedBossBodySheet> sheets, string directory)
    {
        Ensure.NotNull(sheets);
        Directory.CreateDirectory(directory);
        var manifest = new Manifest(ManifestVersion, sheets.ToDictionary(sheet => sheet.Owner.ToString(), sheet => (SheetDocument?)new SheetDocument(
            sheet.Frames.ToDictionary(frame => frame.Key, frame => (FrameDocument?)Document(frame.Value), StringComparer.Ordinal)),
            StringComparer.Ordinal));
        File.WriteAllBytes(Path.Combine(directory, ManifestFileName), JsonSerializer.SerializeToUtf8Bytes(manifest, JsonOptions));
        return InstalledSheetFamily.Install(directory, ProvenanceFileName,
            [.. sheets.Select(sheet => new NamedComposedSheet(BossBodySheetComposer.SheetNameOf(sheet.Owner), sheet.Sheet))]);
    }

    /// <summary>Loads every boss's body sheet from the installed files, its pictures from the picture store.</summary>
    /// <exception cref="InvalidDataException">The manifest, a sheet manifest or the picture store is invalid, or a boss has no sheet.</exception>
    public static BossBodySheets Load(string directory) => new(LoadEach(directory));

    /// <summary>Loads the body sheet of each boss the files hold, which may be fewer than every boss.</summary>
    /// <exception cref="InvalidDataException">The manifest, an image or a frame manifest is invalid.</exception>
    internal static IReadOnlyList<BossBodySheet> LoadEach(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string manifestPath = Path.Combine(directory, ManifestFileName);
        Manifest manifest = JsonSerializer.Deserialize<Manifest>(File.ReadAllBytes(manifestPath), JsonOptions)
            ?? throw new InvalidDataException($"Boss body manifest {manifestPath} is empty.");
        if (manifest.Version != ManifestVersion || manifest.Sheets is null)
            throw new InvalidDataException($"Boss body manifest {manifestPath} is not revision {ManifestVersion}.");
        SpriteSheetStore store = SpriteSheetStore.Open(directory, ProvenanceFileName, manifest.Sheets.Count);
        return [.. manifest.Sheets.Select(entry =>
        {
            BossBodyOwner owner = Enum.TryParse(entry.Key, out BossBodyOwner parsed) && Enum.IsDefined(parsed) ? parsed
                : throw new InvalidDataException($"Boss body manifest {manifestPath} names unknown boss {entry.Key}.");
            SheetDocument document = entry.Value ?? throw new InvalidDataException($"Boss body manifest {manifestPath} has no sheet for {owner}.");
            SpriteSheet sheet = store.Read(BossBodySheetComposer.SheetNameOf(owner));
            return new BossBodySheet(owner, sheet,
                (document.Frames ?? throw new InvalidDataException($"Boss body manifest {manifestPath} lists no frames for {owner}."))
                    .Select(frame => Frame(sheet, frame.Key, frame.Value, manifestPath)));
        })];
    }

    /// <summary>The manifest entry of <paramref name="frame"/>.</summary>
    private static FrameDocument Document(ComposedBossBodyFrame frame) =>
        new(frame.Picture, frame.Corner, frame.Depth, frame.Dissolving.Count == 0 ? null : [.. frame.Dissolving]);

    /// <summary>
    /// The frame named <paramref name="name"/> that <paramref name="document"/> describes, its picture cut from
    /// <paramref name="sheet"/> and drawn at the depth it names (#1634); a frame naming no picture shows nothing, has no
    /// depth, and the part showing it is not drawn (#1513).
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The entry is missing, places its picture nowhere, names a picture without a defined depth, or a depth without a picture.
    /// </exception>
    private static BossBodyFrame Frame(SpriteSheet sheet, string name, FrameDocument? document, string manifestPath)
    {
        if (document is not { Corner: { } corner })
            throw new InvalidDataException($"Boss body manifest {manifestPath} frame {name} is incomplete.");
        if (document.Picture is null != document.Depth is null || document.Depth is { } depth && !Enum.IsDefined(depth))
            throw new InvalidDataException($"Boss body manifest {manifestPath} frame {name} needs a defined depth exactly when it names a picture.");
        return new BossBodyFrame(name, document.Picture is null ? null : sheet.Frame(document.Picture), corner, document.Depth,
            document.Dissolving is null ? [] : [.. document.Dissolving]);
    }

    /// <summary>The manifest: each boss's sheet, by boss.</summary>
    /// <param name="Version">The schema revision.</param>
    /// <param name="Sheets">Each boss's frames, by boss name.</param>
    private sealed record Manifest(int Version, Dictionary<string, SheetDocument?> Sheets);

    /// <summary>One boss's frames.</summary>
    /// <param name="Frames">Each frame, by frame name.</param>
    private sealed record SheetDocument(Dictionary<string, FrameDocument?>? Frames);

    /// <summary>One frame: its whole picture, where it lies and the depth it is drawn at.</summary>
    /// <param name="Picture">The sheet frame of its picture, or null for a frame that paints nothing.</param>
    /// <param name="Corner">The body pixel its picture's top-left corner lies on.</param>
    /// <param name="Depth">The depth its picture is drawn at, by name (#1634), or null for a frame that paints nothing.</param>
    /// <param name="Dissolving">The picture's cells a column dissolve erases (#1652), or null for a frame no dissolve erases.</param>
    private sealed record FrameDocument(string? Picture, Position16<BackgroundPixels>? Corner,
        [property: JsonConverter(typeof(JsonStringEnumConverter<BossBodyDepth>))]
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] BossBodyDepth? Depth,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] DissolvingCell[]? Dissolving = null);

    /// <summary>Stable camel-case manifest fields.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };
}

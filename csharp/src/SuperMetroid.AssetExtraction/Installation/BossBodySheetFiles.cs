using System.Text.Json;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>
/// Installs the pre-assembled boss body sheets (#1466, #1515, #1540): each boss's sheet as an image, palette map and frames,
/// the manifest naming each frame's whole picture and where it lies on the body's area, and the provenance of their
/// hashes. A player edits a sheet's installed files in place.
/// </summary>
public static class BossBodySheetFiles
{
    /// <summary>The provenance manifest's file name.</summary>
    public const string ProvenanceFileName = "boss-body-sheets-provenance.json";

    /// <summary>The manifest naming each boss's frames.</summary>
    public const string ManifestFileName = "boss-bodies.json";

    /// <summary>
    /// The manifest's schema revision: 5 names each frame's one whole picture and where it lies on the body's area, a part
    /// drawn over the parts before it with no rectangles it covers (#1540), Kraid's body below his head and the head it
    /// rises with as frames of their own; with no cell footprints, blank (#1515) or pieces of pictures laid over a frame
    /// (#1513).
    /// </summary>
    private const int ManifestVersion = 5;

    /// <summary>Composes and writes every boss's body sheet.</summary>
    /// <param name="content">Installed stock content holding the frames, characters and palettes the sheets compose from.</param>
    /// <param name="directory">Destination directory, created if absent.</param>
    /// <param name="sourceCartridgeSha256">Identity written into the provenance manifest.</param>
    public static void Extract(string content, string directory, string sourceCartridgeSha256) =>
        Write(BossBodySheetComposer.Compose(RoomBlockSheetInputs.Load(content)), directory, sourceCartridgeSha256);

    /// <summary>Writes composed <paramref name="sheets"/> and their manifest, as <see cref="Load"/> reads them.</summary>
    /// <param name="sheets">Every boss's composed body sheet.</param>
    /// <param name="directory">Destination directory, created if absent.</param>
    /// <param name="sourceCartridgeSha256">Identity written into the provenance manifest.</param>
    internal static void Write(IReadOnlyList<ComposedBossBodySheet> sheets, string directory, string sourceCartridgeSha256)
    {
        SpriteSheetStore store = SpriteSheetStore.Create(directory, ProvenanceFileName);
        store.WriteAll([.. sheets.Select(sheet => new NamedComposedSheet(BossBodySheetComposer.SheetNameOf(sheet.Owner), sheet.Sheet))]);
        store.Finish(sourceCartridgeSha256);
        var manifest = new Manifest(ManifestVersion, sheets.ToDictionary(sheet => sheet.Owner.ToString(), sheet => (SheetDocument?)new SheetDocument(
            sheet.Frames.ToDictionary(frame => frame.Key, frame => (FrameDocument?)Document(frame.Value), StringComparer.Ordinal)),
            StringComparer.Ordinal));
        File.WriteAllBytes(Path.Combine(directory, ManifestFileName), JsonSerializer.SerializeToUtf8Bytes(manifest, JsonOptions));
    }

    /// <summary>Loads every boss's body sheet from the installed files.</summary>
    /// <exception cref="InvalidDataException">The manifest, an image or a frame manifest is invalid.</exception>
    public static BossBodySheets Load(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string manifestPath = Path.Combine(directory, ManifestFileName);
        Manifest manifest = JsonSerializer.Deserialize<Manifest>(File.ReadAllBytes(manifestPath), JsonOptions)
            ?? throw new InvalidDataException($"Boss body manifest {manifestPath} is empty.");
        if (manifest.Version != ManifestVersion || manifest.Sheets is null)
            throw new InvalidDataException($"Boss body manifest {manifestPath} is not revision {ManifestVersion}.");
        SpriteSheetStore store = SpriteSheetStore.Open(directory, ProvenanceFileName, manifest.Sheets.Count);
        return new BossBodySheets(manifest.Sheets.Select(entry =>
        {
            BossBodyOwner owner = Enum.TryParse(entry.Key, out BossBodyOwner parsed) && Enum.IsDefined(parsed) ? parsed
                : throw new InvalidDataException($"Boss body manifest {manifestPath} names unknown boss {entry.Key}.");
            SheetDocument document = entry.Value ?? throw new InvalidDataException($"Boss body manifest {manifestPath} has no sheet for {owner}.");
            SpriteSheet sheet = store.Read(BossBodySheetComposer.SheetNameOf(owner));
            return new BossBodySheet(owner, sheet,
                (document.Frames ?? throw new InvalidDataException($"Boss body manifest {manifestPath} lists no frames for {owner}."))
                    .Select(frame => Frame(sheet, frame.Key, frame.Value, manifestPath)));
        }));
    }

    /// <summary>The manifest entry of <paramref name="frame"/>.</summary>
    private static FrameDocument Document(ComposedBossBodyFrame frame) => new(frame.Picture, frame.Corner);

    /// <summary>
    /// The frame named <paramref name="name"/> that <paramref name="document"/> describes, its picture cut from
    /// <paramref name="sheet"/>; a frame naming no picture shows nothing, and the part showing it is not drawn (#1513).
    /// </summary>
    /// <exception cref="InvalidDataException">The entry is missing or places its picture nowhere.</exception>
    private static BossBodyFrame Frame(SpriteSheet sheet, string name, FrameDocument? document, string manifestPath)
    {
        if (document is not { Corner: { } corner })
            throw new InvalidDataException($"Boss body manifest {manifestPath} frame {name} is incomplete.");
        return new BossBodyFrame(name, document.Picture is null ? null : sheet.Frame(document.Picture), corner);
    }

    /// <summary>The manifest: each boss's sheet, by boss.</summary>
    /// <param name="Version">The schema revision.</param>
    /// <param name="Sheets">Each boss's frames, by boss name.</param>
    private sealed record Manifest(int Version, Dictionary<string, SheetDocument?> Sheets);

    /// <summary>One boss's frames.</summary>
    /// <param name="Frames">Each frame, by frame name.</param>
    private sealed record SheetDocument(Dictionary<string, FrameDocument?>? Frames);

    /// <summary>One frame: its whole picture and where it lies.</summary>
    /// <param name="Picture">The sheet frame of its picture, or null for a frame that paints nothing.</param>
    /// <param name="Corner">The body pixel its picture's top-left corner lies on.</param>
    private sealed record FrameDocument(string? Picture, Position16<BackgroundPixels>? Corner);

    /// <summary>Stable camel-case manifest fields.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };
}

using System.Collections.Immutable;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>
/// Installs the pre-assembled boss body sheets (#1466, #1515): each boss's sheet as an image, palette map and frames, the
/// manifest naming each frame's whole picture, where it lies on the body's area, the rectangles it covers and its
/// animations' pictures, and the provenance of their hashes. A player edits a sheet's installed files in place.
/// </summary>
public static class BossBodySheetFiles
{
    /// <summary>The provenance manifest's file name.</summary>
    public const string ProvenanceFileName = "boss-body-sheets-provenance.json";

    /// <summary>The manifest naming each boss's frames.</summary>
    public const string ManifestFileName = "boss-bodies.json";

    /// <summary>
    /// The manifest's schema revision: 3 names each frame's one whole picture, placed on the body's area with the
    /// rectangles it covers, and its animations' pictures, with no cell footprints or blank (#1515).
    /// </summary>
    private const int ManifestVersion = 3;

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
    private static FrameDocument Document(ComposedBossBodyFrame frame) => new(frame.Picture, frame.Corner, [.. frame.Covers.Select(Area)],
        [.. frame.Overlays.Select(overlay => new OverlayDocument(overlay.Animation.Name, overlay.Corner, [.. overlay.Covers.Select(Area)],
            new Dictionary<string, string>(overlay.Pictures, StringComparer.Ordinal)))]);

    /// <summary>
    /// The frame named <paramref name="name"/> that <paramref name="document"/> describes, its pictures cut from
    /// <paramref name="sheet"/>; a frame naming no picture covers what lies beneath it and shows nothing there (#1513).
    /// </summary>
    /// <exception cref="InvalidDataException">The entry is missing or incomplete, or covers nothing.</exception>
    private static BossBodyFrame Frame(SpriteSheet sheet, string name, FrameDocument? document, string manifestPath)
    {
        if (document?.Corner is null || document.Covers is null || document.Covers.Length == 0)
            throw new InvalidDataException($"Boss body manifest {manifestPath} frame {name} is incomplete or covers nothing.");
        return new BossBodyFrame(name, document.Picture is null ? null : sheet.Frame(document.Picture), document.Corner.Value,
            Rectangles(document.Covers, manifestPath), [.. (document.Overlays ?? []).Select(overlay => Overlay(sheet, name, overlay, manifestPath))]);
    }

    /// <summary>The overlay <paramref name="document"/> of frame <paramref name="frame"/> describes, its pictures cut from <paramref name="sheet"/>.</summary>
    /// <exception cref="InvalidDataException">The entry is missing or incomplete.</exception>
    private static BossBodyOverlay Overlay(SpriteSheet sheet, string frame, OverlayDocument? document, string manifestPath)
    {
        if (document?.Animation is null || document.Corner is null || document.Covers is null || document.Pictures is null)
            throw new InvalidDataException($"Boss body manifest {manifestPath} frame {frame} has an incomplete overlay.");
        return new BossBodyOverlay(new ArtworkAnimation(document.Animation), document.Corner.Value, Rectangles(document.Covers, manifestPath),
            document.Pictures.ToDictionary(entry => entry.Key, entry => sheet.Frame(entry.Value), StringComparer.Ordinal));
    }

    /// <summary>The manifest entry of <paramref name="rectangle"/>.</summary>
    private static AreaDocument Area(BossBodyRectangle rectangle) => new(rectangle.Corner, rectangle.Size);

    /// <summary>The rectangles <paramref name="documents"/> name.</summary>
    /// <exception cref="InvalidDataException">A rectangle is missing, empty or lies off the body.</exception>
    private static ImmutableArray<BossBodyRectangle> Rectangles(AreaDocument?[] documents, string manifestPath) =>
        [.. documents.Select(document => document is { Corner: { } corner, Size: { X: > 0, Y: > 0 } size }
                && corner.X + size.X <= ushort.MaxValue && corner.Y + size.Y <= ushort.MaxValue
            ? new BossBodyRectangle(corner, size)
            : throw new InvalidDataException($"Boss body manifest {manifestPath} has a rectangle that is missing, empty or off the body."))];

    /// <summary>The manifest: each boss's sheet, by boss.</summary>
    /// <param name="Version">The schema revision.</param>
    /// <param name="Sheets">Each boss's frames, by boss name.</param>
    private sealed record Manifest(int Version, Dictionary<string, SheetDocument?> Sheets);

    /// <summary>One boss's frames.</summary>
    /// <param name="Frames">Each frame, by frame name.</param>
    private sealed record SheetDocument(Dictionary<string, FrameDocument?>? Frames);

    /// <summary>One frame: its whole picture, where it lies, what it covers, and its animations' pictures.</summary>
    /// <param name="Picture">The sheet frame of its picture, or null for a frame that paints nothing.</param>
    /// <param name="Corner">The body pixel its picture's top-left corner lies on.</param>
    /// <param name="Covers">The rectangles of the body it covers.</param>
    /// <param name="Overlays">The pixels each room animation paints.</param>
    private sealed record FrameDocument(string? Picture, Position16<BackgroundPixels>? Corner, AreaDocument?[]? Covers, OverlayDocument?[]? Overlays);

    /// <summary>The pixels of a frame one room animation paints.</summary>
    /// <param name="Animation">The animation's name.</param>
    /// <param name="Corner">The body pixel each picture's top-left corner lies on.</param>
    /// <param name="Covers">The rectangles of the body it paints.</param>
    /// <param name="Pictures">The sheet frame picturing them over each frame of the animation, by frame name.</param>
    private sealed record OverlayDocument(string? Animation, Position16<BackgroundPixels>? Corner, AreaDocument?[]? Covers, Dictionary<string, string>? Pictures);

    /// <summary>A rectangle of the body.</summary>
    /// <param name="Corner">The body pixel of its top-left corner.</param>
    /// <param name="Size">Its width and height in body pixels.</param>
    private sealed record AreaDocument(Position16<BackgroundPixels>? Corner, Extent16<BackgroundPixels>? Size);

    /// <summary>Stable camel-case manifest fields.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };
}

using System.Collections.Immutable;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>
/// Installs the pre-assembled boss body sheets (#1466): each boss's sheet as an image, palette map and frames, the manifest
/// naming each frame's parts with the layer rows each lies on and covers, and the provenance of their hashes. A player
/// override of a sheet's file replaces the stock one.
/// </summary>
public static class BossBodySheetFiles
{
    /// <summary>The provenance manifest's file name.</summary>
    public const string ProvenanceFileName = "boss-body-sheets-provenance.json";

    /// <summary>The manifest naming each boss's frames and their parts.</summary>
    public const string ManifestFileName = "boss-bodies.json";

    /// <summary>The manifest's schema revision.</summary>
    private const int ManifestVersion = 1;

    /// <summary>Composes and writes every boss's body sheet; the installer validates them with <see cref="ValidateStock"/>.</summary>
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
            Document(sheet.Blank), sheet.Frames.ToDictionary(frame => frame.Key, frame => (PartDocument[]?)frame.Value.Select(Document).ToArray(), StringComparer.Ordinal)),
            StringComparer.Ordinal));
        File.WriteAllBytes(Path.Combine(directory, ManifestFileName), JsonSerializer.SerializeToUtf8Bytes(manifest, JsonOptions));
    }

    /// <summary>Loads every boss's body sheet, preferring a player override of each sheet file.</summary>
    /// <exception cref="InvalidDataException">The manifest, a stock hash, an image or a frame manifest is invalid.</exception>
    public static BossBodySheets Load(string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, ManifestFileName);
        Manifest manifest = JsonSerializer.Deserialize<Manifest>(File.ReadAllBytes(manifestPath), JsonOptions)
            ?? throw new InvalidDataException($"Boss body manifest {manifestPath} is empty.");
        if (manifest.Version != ManifestVersion || manifest.Sheets is null)
            throw new InvalidDataException($"Boss body manifest {manifestPath} is not revision {ManifestVersion}.");
        SpriteSheetStore store = SpriteSheetStore.Open(stockDirectory, overrideDirectory, ProvenanceFileName, manifest.Sheets.Count);
        return new BossBodySheets(manifest.Sheets.Select(entry =>
        {
            BossBodyOwner owner = Enum.TryParse(entry.Key, out BossBodyOwner parsed) && Enum.IsDefined(parsed) ? parsed
                : throw new InvalidDataException($"Boss body manifest {manifestPath} names unknown boss {entry.Key}.");
            SheetDocument document = entry.Value ?? throw new InvalidDataException($"Boss body manifest {manifestPath} has no sheet for {owner}.");
            SpriteSheet sheet = store.Read(BossBodySheetComposer.SheetNameOf(owner));
            return new BossBodySheet(owner, sheet, Part(sheet, document.Blank, manifestPath),
                (document.Frames ?? throw new InvalidDataException($"Boss body manifest {manifestPath} lists no frames for {owner}."))
                    .Select(frame => new BossBodyFrame(frame.Key, [.. (frame.Value ?? []).Select(part => Part(sheet, part, manifestPath))])));
        }));
    }

    /// <summary>Used by installer completeness checks; overrides do not affect stock provenance.</summary>
    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);

    /// <summary>The manifest entry of <paramref name="part"/>.</summary>
    private static PartDocument Document(ComposedBossBodyPart part) => new(part.Picture, part.Top, part.Footprint.Top, [.. part.Footprint.Rows],
        part.Animation?.Name, part.Animation is null ? null : new Dictionary<string, string>(part.AnimationPictures, StringComparer.Ordinal));

    /// <summary>The part <paramref name="document"/> describes, its pictures cut from <paramref name="sheet"/>.</summary>
    /// <exception cref="InvalidDataException">The entry is missing, names no picture, or covers nothing.</exception>
    private static BossBodyPart Part(SpriteSheet sheet, PartDocument? document, string manifestPath)
    {
        if (document?.Picture is null || document.Footprint is null)
            throw new InvalidDataException($"Boss body manifest {manifestPath} has an incomplete part.");
        var footprint = new BossBodyFootprint(document.FootprintTop, ImmutableArray.Create(document.Footprint));
        if (footprint.IsEmpty)
            throw new InvalidDataException($"Boss body manifest {manifestPath} part {document.Picture} covers nothing.");
        return new BossBodyPart(document.Picture, sheet.Frame(document.Picture), document.Top, footprint,
            document.Animation is null ? null : new ArtworkAnimation(document.Animation),
            document.AnimationPictures?.ToDictionary(entry => entry.Key, entry => sheet.Frame(entry.Value), StringComparer.Ordinal));
    }

    /// <summary>The manifest: each boss's sheet, by boss.</summary>
    /// <param name="Version">The schema revision.</param>
    /// <param name="Sheets">Each boss's frames, by boss name.</param>
    private sealed record Manifest(int Version, Dictionary<string, SheetDocument?> Sheets);

    /// <summary>One boss's frames.</summary>
    /// <param name="Blank">The part the boss clears its layer with.</param>
    /// <param name="Frames">Each frame's parts, by frame name.</param>
    private sealed record SheetDocument(PartDocument? Blank, Dictionary<string, PartDocument[]?>? Frames);

    /// <summary>One part: its picture, where it lies and what it covers, and its animation's pictures.</summary>
    /// <param name="Picture">The sheet frame of its still picture.</param>
    /// <param name="Top">The layer row its picture's top edge lies on.</param>
    /// <param name="FootprintTop">The first layer row it covers.</param>
    /// <param name="Footprint">The cells it covers in each row from <paramref name="FootprintTop"/>, the lowest bit the leftmost.</param>
    /// <param name="Animation">The animation it follows, or null for a still part.</param>
    /// <param name="AnimationPictures">The sheet frame picturing it for each frame of its animation, by frame name.</param>
    private sealed record PartDocument(string? Picture, int Top, int FootprintTop, uint[]? Footprint, string? Animation,
        Dictionary<string, string>? AnimationPictures);

    /// <summary>Stable camel-case manifest fields.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };
}

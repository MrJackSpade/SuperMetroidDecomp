using System.Text.Json.Serialization;
using SuperMetroid.Core;
using System.Text.Json;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Extracts the visual block references of ordinary shot-block PLM draw lists.
/// The native draw shapes and complete collision-changing block stay compiled.
/// </summary>
public static class RoomObjectShotBlockVisualFiles
{
    /// <summary>Family-relative installed JSON filename for all nineteen ordinary shot-block breakup and restoration draws.</summary>
    public const string VisualFileName = "shot-blocks.json";
    /// <summary>Stock manifest filename recording format version one, cartridge provenance, and the visual JSON's SHA-256.</summary>
    public const string ManifestFileName = "manifest.json";
    /// <summary>Schema version shared by shot-block visual JSON and its stock manifest.</summary>
    private const int FormatVersion = 1;

    /// <summary>Uses camel-case JSON while accepting historical member casing on load.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>Verifies ordinary shot-block bank-$84 draw geometry, complete terrain, and continuation offsets before exporting visual-only run arrays.</summary>
    /// <param name="bus">Cartridge source for all nineteen compiled draws; stored identities are draw-list pointers, not PLM headers or instruction pointers.</param>
    /// <param name="directory">Family directory, created before native validation; receives new visual JSON and manifest files.</param>
    /// <param name="sourceCartridgeSha256">Nonblank caller-supplied provenance hash, recorded without recomputing it; stock load requires the supported cartridge identity.</param>
    /// <remarks>Words retain metatile/parent-flip bits 0..11 in native run order. Existing files are refused and separate JSON/manifest writes are not rolled back together.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory or source hash is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Native draw data differs from compiled cartridge definitions.</exception>
    /// <exception cref="IOException">An output exists or filesystem access fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        RoomObjectVisualRunEntry[] entries = ReadAndVerifyNative(bus);
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(
            new VisualDocument(FormatVersion, entries), JsonOptions);
        using (var output = new FileStream(Path.Combine(directory, VisualFileName),
                   FileMode.CreateNew, FileAccess.Write))
            output.Write(json);
        using var manifest = new FileStream(Path.Combine(directory, ManifestFileName),
            FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(manifest,
            new VisualManifest(FormatVersion, sourceCartridgeSha256), JsonOptions);
    }

    /// <summary>Validates the installed shot-block manifest's provenance and loads the installed shot-block JSON as the player left it, without cartridge access.</summary>
    /// <param name="directory">Installed family directory containing the version-one shot-block JSON and its manifest.</param>
    /// <returns>A ROM-independent catalog that copies edited run arrays and preserves native collision, draw placement, timing, and slot effects.</returns>
    /// <exception cref="ArgumentException"><paramref name="directory"/> is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Manifest provenance, schema, identities, coverage, run shape, or appearance are invalid.</exception>
    /// <exception cref="IOException">The installed manifest or shot-block JSON cannot be read.</exception>
    public static InstalledRoomObjectShotBlockVisuals Load(
        string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string manifestPath = Path.Combine(directory, ManifestFileName);
        VisualManifest manifest = ReadJson<VisualManifest>(manifestPath);
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Shot-block visual manifest {manifestPath} is incompatible.");
        string path = Path.Combine(directory, VisualFileName);
        VisualDocument document = ReadJson<VisualDocument>(path);
        ValidateDocument(document, path);
        return CreateCatalog(document, path);
    }

    /// <summary>Validates visual references for all nineteen compiled breakup/restoration draw lists and builds the visuals from the run appearances that differ from the calculated draws; compiled collision and run placement are never replaced.</summary>
    /// <param name="entries">Complete, duplicate-free draw identity set with unchanged run counts and lengths.</param>
    /// <exception cref="ArgumentNullException"><paramref name="entries"/> is null.</exception>
    /// <exception cref="InvalidDataException">An entry/run is null, a draw identity is unknown/duplicated/missing, the run shape differs, or a block count differs.</exception>
    internal static InstalledRoomObjectShotBlockVisuals Create(IEnumerable<RoomObjectVisualRunEntry> entries) =>
        new(RoomObjectVisualOverrideReader.RunByRun(entries, "Shot-block", TryGetDrawListByVisualId, Calculated.Count()));

    /// <summary>The published artwork identity of a shot-block draw: its shape and breakup frame, or its restoration.</summary>
    internal static string VisualId(ShotBlockObjectDraws.Draw draw)
    {
        string shape = draw.Layout switch
        {
            RoomObjectShotBlockDrawDefinitions.Shape.Single => "single",
            RoomObjectShotBlockDrawDefinitions.Shape.Horizontal => "horizontal",
            RoomObjectShotBlockDrawDefinitions.Shape.Vertical => "vertical",
            RoomObjectShotBlockDrawDefinitions.Shape.Square => "square",
            _ => throw new InvalidOperationException($"Undefined shot-block shape {draw.Layout}."),
        };
        return draw.Restore ? $"{shape}-restore" : $"{shape}-breakup-{draw.Frame}";
    }

    /// <summary>The published artwork identity of a shot-block <paramref name="draw"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not a shot-block draw.</exception>
    internal static string VisualId(RoomObjectDraw draw) => VisualId(RoomObjectDraw.FrameOf<ShotBlockObjectDraws.Draw>(draw, "shot-block"));

    /// <summary>Finds the shot-block draw whose artwork identity is <paramref name="id"/>.</summary>
    internal static bool TryGetByVisualId(string? id, out ShotBlockObjectDraws.Draw draw) =>
        RoomObjectDrawListExport.TryFindByVisualId(Calculated, VisualId, id, out draw);

    /// <summary>Finds the draw list of the shot-block draw whose artwork identity is <paramref name="id"/>.</summary>
    private static bool TryGetDrawListByVisualId(string id, out RoomObjectShotBlockDrawDefinitions.DrawList list)
    {
        bool found = TryGetByVisualId(id, out ShotBlockObjectDraws.Draw draw);
        list = found ? Export(draw) : default;
        return found;
    }

    /// <summary>The draw lists of every breakup and restoration draw, built from the calculated draws.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> All => Calculated.Select(Export);

    /// <summary>Expands a calculated shot-block draw into ordered terrain runs and offsets.</summary>
    private static RoomObjectShotBlockDrawDefinitions.DrawList Export(ShotBlockObjectDraws.Draw draw) =>
        RoomObjectDrawListExport.Runs(RoomObjectDraw.Of(draw), draw.RunCount, _ => draw.BlocksPerRun, draw.BlockAt,
            _ => draw.Vertical ? RoomObjectRunDirection.Vertical : RoomObjectRunDirection.Horizontal,
            static _ => 0, run => run + 1 < draw.RunCount ? 1 : 0);

    /// <summary>Yields breakup frames interleaved by shape, followed by the three restoration draws.</summary>
    internal static IEnumerable<ShotBlockObjectDraws.Draw> Calculated
    {
        get
        {
            for (int frame = 0; frame < RoomObjectShotBlockDrawDefinitions.BreakupFrameCount; frame++)
            {
                yield return new(RoomObjectShotBlockDrawDefinitions.Shape.Single, frame, Restore: false);
                yield return new(RoomObjectShotBlockDrawDefinitions.Shape.Horizontal, frame, Restore: false);
                yield return new(RoomObjectShotBlockDrawDefinitions.Shape.Vertical, frame, Restore: false);
                yield return new(RoomObjectShotBlockDrawDefinitions.Shape.Square, frame, Restore: false);
            }
            yield return new(RoomObjectShotBlockDrawDefinitions.Shape.Horizontal, 0, Restore: true);
            yield return new(RoomObjectShotBlockDrawDefinitions.Shape.Vertical, 0, Restore: true);
            yield return new(RoomObjectShotBlockDrawDefinitions.Shape.Square, 0, Restore: true);
        }
    }

    /// <summary>Checks all bank 84 run headers, terrain, and offsets before exporting editable appearance.</summary>
    private static RoomObjectVisualRunEntry[] ReadAndVerifyNative(ISnesAddressSpace bus)
    {
        var result = new List<RoomObjectVisualRunEntry>();
        foreach (RoomObjectShotBlockDrawDefinitions.DrawList list in
                 All.OrderBy(item => RoomObjectDrawAddressCodec.Of(item.Draw)))
        {
            int cursor = RoomObjectDrawAddressCodec.Of(list.Draw);
            var visuals = new List<RoomBlockAppearance[]>();
            foreach (RoomObjectShotBlockDrawDefinitions.Run run in list.Runs.Span)
            {
                if (CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(CartridgeBankWindow.At(RoomObjectSourceDefinitions.RoomObjectBank, cursor)) != RoomObjectRunHeaderCodec.Encode(run))
                    throw new InvalidDataException(
                        $"Shot-block draw list {list.Draw} has a different shape in the source cartridge.");
                cursor = unchecked((ushort)(cursor + 2));
                var appearances = new RoomBlockAppearance[run.Count];
                for (int index = 0; index < appearances.Length; index++)
                {
                    RoomTerrainBlock source = RoomLevelWordCodec.Decode(CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(CartridgeBankWindow.At(RoomObjectSourceDefinitions.RoomObjectBank, cursor)));
                    if (source != run.Blocks.Span[index])
                        throw new InvalidDataException(
                            $"Shot-block draw list {list.Draw} differs at ${cursor:X4}.");
                    appearances[index] = source.Appearance;
                    cursor = unchecked((ushort)(cursor + 2));
                }

                if (bus.ReadCartridgeByte(CartridgeBankWindow.At(RoomObjectSourceDefinitions.RoomObjectBank, cursor)) != unchecked((byte)run.Next.X) ||
                    bus.ReadCartridgeByte(CartridgeBankWindow.At(RoomObjectSourceDefinitions.RoomObjectBank, unchecked((ushort)(cursor + 1)))) !=
                        unchecked((byte)run.Next.Y))
                    throw new InvalidDataException(
                        $"Shot-block draw list {list.Draw} has a different next-record offset.");
                cursor = unchecked((ushort)(cursor + 2));
                visuals.Add(appearances);
            }

            result.Add(new RoomObjectVisualRunEntry(VisualId(list.Draw), visuals.ToArray()));
        }

        return result.ToArray();
    }

    /// <summary>Checks the installed document's version and presence of draw entries.</summary>
    private static void ValidateDocument(VisualDocument document, string path)
    {
        if (document.Version != FormatVersion || document.Entries is null)
            throw new InvalidDataException($"Shot-block visuals {path} have an incompatible format.");
    }

    /// <summary>Builds the selected visual catalog and includes the source path in entry errors.</summary>
    private static InstalledRoomObjectShotBlockVisuals CreateCatalog(
        VisualDocument document, string path)
    {
        try
        {
            return Create(document.Entries);
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid shot-block visuals {path}: {error.Message}", error);
        }
    }

    /// <summary>Reads a shot-block manifest or visual document with path-aware diagnostics.</summary>
    private static T ReadJson<T>(string path) => ReadJson<T>(File.ReadAllBytes(path), path);

    /// <summary>Deserializes bytes while rejecting empty or malformed JSON at the named file path.</summary>
    private static T ReadJson<T>(byte[] bytes, string path)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(bytes, JsonOptions)
                ?? throw new InvalidDataException($"Shot-block JSON {path} is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid shot-block JSON {path}.", error);
        }
    }

    /// <summary>Captures stock schema, cartridge provenance, and visual JSON byte hash.</summary>
    private sealed record VisualManifest(int Version, string SourceCartridgeSha256);
    /// <summary>Contains the nineteen ordered visual draw entries and schema version.</summary>
    private sealed record VisualDocument(int Version, RoomObjectVisualRunEntry[] Entries);
}

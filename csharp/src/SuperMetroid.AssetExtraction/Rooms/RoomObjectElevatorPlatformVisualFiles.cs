using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Core;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Installs named elevator-platform appearances. Native draw geometry and decoded terrain
/// blocks are checked against the compiled cartridge definitions.
/// </summary>
public static class RoomObjectElevatorPlatformVisualFiles
{
    /// <summary>Family-relative installed JSON filename for the elevator platform's three six-cell artwork frames.</summary>
    public const string VisualFileName = "elevator-platforms.json";
    /// <summary>Stock manifest filename containing version-one format, cartridge provenance, and the visual JSON's SHA-256.</summary>
    public const string ManifestFileName = "manifest.json";
    /// <summary>Version of the three-frame elevator platform visual document and manifest.</summary>
    private const int FormatVersion = 1;

    /// <summary>Uses camel-case JSON, with block mirroring named, while accepting historical member casing on load.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>Verifies the three elevator-platform draws at $84:AA97–AADE and exports their block appearances, not their solid collision.</summary>
    /// <param name="bus">Cartridge source whose three-run frame shapes, decoded terrain blocks, and continuation offsets must match compiled definitions.</param>
    /// <param name="directory">Family directory, created before native checks; receives new visual JSON and manifest files.</param>
    /// <param name="sourceCartridgeSha256">Nonblank importer-supplied provenance hash, recorded without recomputation and checked against the supported identity on stock load.</param>
    /// <remarks>Each frame retains native upper-edge/upper-edge/lower-row run order. Existing outputs are refused; failure of the later manifest write does not remove the earlier JSON.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory or source hash is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Native draw geometry or decoded terrain blocks differ from compiled data.</exception>
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

    /// <summary>Validates the installed elevator-platform manifest's provenance and loads the installed elevator-platform JSON as the player left it, without cartridge access.</summary>
    /// <param name="directory">Installed family directory containing the version-one elevator-platform JSON and its manifest.</param>
    /// <returns>A ROM-independent catalog with copied authored appearances; solid geometry and the four-update first/second/third/second loop stay compiled.</returns>
    /// <exception cref="ArgumentException"><paramref name="directory"/> is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Manifest provenance, schema, identities, coverage, shapes, or appearances are invalid.</exception>
    /// <exception cref="IOException">The installed manifest or elevator-platform JSON cannot be read.</exception>
    public static RoomObjectElevatorPlatformVisualCatalog Load(
        string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string manifestPath = Path.Combine(directory, ManifestFileName);
        VisualManifest manifest = ReadJson<VisualManifest>(manifestPath);
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Elevator-platform visual manifest {manifestPath} is incompatible.");
        string path = Path.Combine(directory, VisualFileName);
        return CreateCatalog(ReadJson<VisualDocument>(path), path);
    }

    /// <summary>Validates all three elevator-platform draw layouts and builds the visuals from the run appearances that differ from the compiled frames, retaining run geometry, terrain blocks, and animation cadence.</summary>
    /// <remarks>The four-step instruction loop selects first, second, third, then second layout; the repeated second layout has only one entry.</remarks>
    /// <param name="entries">One visual-only entry per compiled frame, with its exact run and cell counts.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves frame coverage incomplete.</exception>
    internal static RoomObjectElevatorPlatformVisualCatalog Create(IEnumerable<RoomObjectVisualRunEntry> entries) =>
        new(RoomObjectVisualOverrideReader.RunByRun(entries, "Elevator-platform", TryGetByVisualId, DrawLists.Count()));

    /// <summary>The draw lists of the three platform frames in order.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> DrawLists
    {
        get
        {
            foreach (ElevatorPlatformDraw platform in Enum.GetValues<ElevatorPlatformDraw>())
            {
                TryGetDraw(RoomObjectDraw.Of(platform), out var list);
                yield return list;
            }
        }
    }

    /// <summary>Maps a platform draw to its stable artwork identifier.</summary>
    internal static string VisualId(RoomObjectDraw request) =>
        RoomObjectVisualIdDefinitions.ElevatorPlatform.TryGetValue(
            RoomObjectDraw.FrameOf<ElevatorPlatformDraw>(request, "elevator-platform draw list"), out string? id)
            ? id
            : throw new InvalidOperationException($"Undefined elevator-platform draw list {request}.");

    /// <summary>Resolves a stable frame identifier to its draw list.</summary>
    internal static bool TryGetByVisualId(string? id, out RoomObjectShotBlockDrawDefinitions.DrawList list) =>
        RoomObjectDrawListExport.FindByVisualId(Enum.GetValues<ElevatorPlatformDraw>().Select(draw => (RoomObjectDraw)RoomObjectDraw.Of(draw)),
            VisualId, TryGetDraw, id, out list);

    /// <summary>Builds the draw list of a platform frame from its strips.</summary>
    internal static bool TryGetDraw(RoomObjectDraw request, out RoomObjectShotBlockDrawDefinitions.DrawList list)
    {
        bool found = ElevatorPlatformDraws.TryDescribe(request, out ElevatorPlatformDraws.Draw draw);
        list = found
            ? RoomObjectDrawListExport.Strips(request, draw.RunCount, draw.BlockCount, draw.BlockAt, draw.Start)
            : default;
        return found;
    }

    /// <summary>Checks bank 84 run headers, decoded terrain blocks, and offsets before extracting editable appearances.</summary>
    private static RoomObjectVisualRunEntry[] ReadAndVerifyNative(ISnesAddressSpace bus)
    {
        var entries = new List<RoomObjectVisualRunEntry>();
        foreach (RoomObjectShotBlockDrawDefinitions.DrawList list in
                 DrawLists.OrderBy(item => RoomObjectDrawAddressCodec.Of(item.Draw)))
        {
            int cursor = RoomObjectDrawAddressCodec.Of(list.Draw);
            var visuals = new List<RoomBlockAppearance[]>();
            for (int runIndex = 0; runIndex < list.Runs.Length; runIndex++)
            {
                RoomObjectShotBlockDrawDefinitions.Run run = list.Runs.Span[runIndex];
                if (CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(CartridgeBankWindow.At(RoomObjectSourceDefinitions.RoomObjectBank, cursor)) != RoomObjectRunHeaderCodec.Encode(run))
                    throw new InvalidDataException(
                        $"Elevator-platform draw list {list.Draw} differs in source cartridge shape.");
                cursor = unchecked((ushort)(cursor + 2));
                var appearances = new RoomBlockAppearance[run.Blocks.Length];
                for (int index = 0; index < appearances.Length; index++)
                {
                    RoomTerrainBlock source = RoomLevelWordCodec.Decode(CartridgeImportSource.Require(bus)
                        .ReadCartridgeWordFixedBank(CartridgeBankWindow.At(RoomObjectSourceDefinitions.RoomObjectBank, cursor)));
                    if (source != run.Blocks.Span[index])
                        throw new InvalidDataException(
                            $"Elevator-platform draw list {list.Draw} differs at ${cursor:X4}.");
                    appearances[index] = source.Appearance;
                    cursor = unchecked((ushort)(cursor + 2));
                }

                if (bus.ReadCartridgeByte(CartridgeBankWindow.At(RoomObjectSourceDefinitions.RoomObjectBank, cursor)) != unchecked((byte)RoomObjectRunContinuationCodec.X(list, runIndex)) ||
                    bus.ReadCartridgeByte(CartridgeBankWindow.At(RoomObjectSourceDefinitions.RoomObjectBank, unchecked((ushort)(cursor + 1)))) !=
                        unchecked((byte)RoomObjectRunContinuationCodec.Y(list, runIndex)))
                    throw new InvalidDataException(
                        $"Elevator-platform draw list {list.Draw} differs in next-record offset.");
                cursor = unchecked((ushort)(cursor + 2));
                visuals.Add(appearances);
            }

            entries.Add(new RoomObjectVisualRunEntry(
                VisualId(list.Draw), visuals.ToArray()));
        }

        return entries.ToArray();
    }

    /// <summary>Checks document version and entries while including the installed file path in validation errors.</summary>
    private static RoomObjectElevatorPlatformVisualCatalog CreateCatalog(
        VisualDocument document, string path)
    {
        if (document.Version != FormatVersion || document.Entries is null)
            throw new InvalidDataException($"Elevator-platform visuals {path} have an incompatible format.");
        try { return Create(document.Entries); }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid elevator-platform visuals {path}: {error.Message}", error);
        }
    }

    /// <summary>Reads a manifest or visual document and carries its source path into parse diagnostics.</summary>
    private static T ReadJson<T>(string path) => ReadJson<T>(File.ReadAllBytes(path), path);
    /// <summary>Deserializes JSON bytes and identifies the installed file when content is empty or malformed.</summary>
    private static T ReadJson<T>(byte[] bytes, string path)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(bytes, JsonOptions)
                ?? throw new InvalidDataException($"Elevator-platform JSON {path} is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid elevator-platform JSON {path}.", error);
        }
    }

    /// <summary>Captures schema version, supported cartridge provenance, and the stock visual JSON hash.</summary>
    private sealed record VisualManifest(int Version, string SourceCartridgeSha256);
    /// <summary>Contains the three editable elevator frame entries and their schema version.</summary>
    private sealed record VisualDocument(int Version, RoomObjectVisualRunEntry[] Entries);
}

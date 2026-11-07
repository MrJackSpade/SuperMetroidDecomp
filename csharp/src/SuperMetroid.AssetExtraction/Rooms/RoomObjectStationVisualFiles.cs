using System.Text.Json.Serialization;
using SuperMetroid.Core;
using System.Text.Json;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Installs named, editable station visual block references. Native draw geometry and
/// complete terrain are checked against the compiled cartridge catalog.
/// </summary>
public static class RoomObjectStationVisualFiles
{
    /// <summary>Family-relative installed JSON filename for twenty map, refill, save, and station-access draw layouts.</summary>
    public const string VisualFileName = "stations.json";
    /// <summary>Stock manifest filename recording version-one format, cartridge provenance, and the visual JSON's SHA-256.</summary>
    public const string ManifestFileName = "manifest.json";
    /// <summary>Supported station-visual JSON and stock-manifest revision.</summary>
    private const int FormatVersion = 1;

    /// <summary>Serializes readable camel-case station records and accepts case-insensitive field names.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>Checks every compiled bank-$84 station draw shape, terrain, and offset before exporting named metatile/flip run arrays.</summary>
    /// <param name="bus">Cartridge source for map, energy, missile, save, and access layouts in their original draw-run order.</param>
    /// <param name="directory">Family directory, created before native checks; receives new visual JSON and manifest files.</param>
    /// <param name="sourceCartridgeSha256">Nonblank importer-supplied provenance hash, recorded without recomputation and required to match the supported identity on stock load.</param>
    /// <remarks>Only appearance 0..11 are serialized; save-floor cap runs retain native order rather than a top-to-bottom rearrangement. Existing files are refused and separate writes can leave a partial installation.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory or source hash is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Native station draws differ from compiled shapes, terrain, or offsets.</exception>
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

    /// <summary>Validates the installed station manifest's provenance and loads the installed station JSON as the player left it, without cartridge access.</summary>
    /// <param name="directory">Installed family directory containing the version-one station JSON and its manifest.</param>
    /// <returns>A ROM-independent catalog with copied authored runs and compiled station mechanics and draw geometry unchanged.</returns>
    /// <exception cref="ArgumentException"><paramref name="directory"/> is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Manifest provenance, schema, identities, coverage, run shape, or appearance are invalid.</exception>
    /// <exception cref="IOException">The installed manifest or station JSON cannot be read.</exception>
    public static InstalledRoomObjectStationVisuals Load(
        string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string manifestPath = Path.Combine(directory, ManifestFileName);
        VisualManifest manifest = ReadJson<VisualManifest>(manifestPath);
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Station visual manifest {manifestPath} is incompatible.");
        string path = Path.Combine(directory, VisualFileName);
        return CreateCatalog(ReadJson<VisualDocument>(path), path);
    }

    /// <summary>Validates and captures the complete twenty-frame station artwork selection, building the visuals from the run appearances that differ from the compiled layouts.</summary>
    /// <param name="entries">Every compiled map, energy, missile, save, and access frame exactly once, with its original run/cell counts and valid visual-only appearances.</param>
    /// <remarks>Changed frames are deep-copied; stock-identical frames resolve from compiled definitions. Later caller-array edits do not affect either path.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="entries"/> is null.</exception>
    /// <exception cref="InvalidDataException">Coverage, uniqueness, frame identity, or array shape are invalid.</exception>
    internal static InstalledRoomObjectStationVisuals Create(IEnumerable<RoomObjectVisualRunEntry> entries) =>
        new(RoomObjectVisualOverrideReader.RunByRun(entries, "Station", TryGetByVisualId, All.Count()));

    /// <summary>The layout of <paramref name="request"/>, which must be one of the twenty station draw lists.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="request"/> is not a station draw list.</exception>
    internal static StationObjectDraws.Draw Describe(RoomObjectDraw request) =>
        StationObjectDraws.TryDescribe(request, out StationObjectDraws.Draw? draw) ? draw : throw new InvalidOperationException($"{request} is not a station draw list.");

    /// <summary>Builds the draw list of a station draw from its calculated cells.</summary>
    internal static bool TryGet(RoomObjectDraw request, out RoomObjectShotBlockDrawDefinitions.DrawList list)
    {
        if (!StationObjectDraws.TryDescribe(request, out StationObjectDraws.Draw? draw))
        {
            list = default;
            return false;
        }
        list = RoomObjectDrawListExport.Runs(request, draw.RunCount, draw.BlockCount, draw.BlockAt,
            RoomObjectDrawListExport.Horizontal, draw.NextX, draw.NextY);
        return true;
    }

    /// <summary>
    /// Published ordinal artwork IDs: three numbered frames per map/resource
    /// station, and named save/access states. These are identity selections,
    /// independent of the terrain. Only complete lists have IDs.
    /// </summary>
    internal static string VisualId(RoomObjectDraw request) =>
        RoomObjectDraw.FrameOf<StationDraw>(request, "station draw list") switch
        {
            StationDraw.MapFrame0 => "map-frame-0",
            StationDraw.MapFrame1 => "map-frame-1",
            StationDraw.MapFrame2 => "map-frame-2",
            StationDraw.EnergyFrame0 => "energy-frame-0",
            StationDraw.EnergyFrame1 => "energy-frame-1",
            StationDraw.EnergyFrame2 => "energy-frame-2",
            StationDraw.MissileFrame0 => "missile-frame-0",
            StationDraw.MissileFrame1 => "missile-frame-1",
            StationDraw.MissileFrame2 => "missile-frame-2",
            StationDraw.SaveIdle => "save-idle",
            StationDraw.SaveActive => "save-active-a",
            StationDraw.SaveAlternate => "save-active-b",
            StationDraw.MapRightRetracted => "map-right-retracted",
            StationDraw.MapRightExtended => "map-right-extended",
            StationDraw.MapLeftRetracted => "map-left-retracted",
            StationDraw.MapLeftExtended => "map-left-extended",
            StationDraw.ResourceRightRetracted => "resource-right-retracted",
            StationDraw.ResourceRightExtended => "resource-right-extended",
            StationDraw.ResourceLeftRetracted => "resource-left-retracted",
            StationDraw.ResourceLeftExtended => "resource-left-extended",
            _ => throw new InvalidOperationException($"Undefined station draw list {request}."),
        };

    /// <summary>Finds a station draw list by its stable artwork identity rather than pointer.</summary>
    internal static bool TryGetByVisualId(string id, out RoomObjectShotBlockDrawDefinitions.DrawList list) =>
        RoomObjectDrawListExport.FindByVisualId(All, VisualId, id, out list);

    /// <summary>Calculates the twenty complete station lists in published enumeration order.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (StationDraw frame in RoomObjectVisualIdDefinitions.StationPublishedOrder)
            {
                TryGet(RoomObjectDraw.Of(frame), out var list);
                yield return list;
            }
        }
    }

    /// <summary>Checks each native station draw run's shape, words, and continuation offset before extracting its twelve-bit visuals.</summary>
    private static RoomObjectVisualRunEntry[] ReadAndVerifyNative(ISnesAddressSpace bus)
    {
        var entries = new List<RoomObjectVisualRunEntry>();
        foreach (RoomObjectShotBlockDrawDefinitions.DrawList list in
                 All.OrderBy(item => RoomObjectDrawAddressCodec.Of(item.Draw)))
        {
            int cursor = RoomObjectDrawAddressCodec.Of(list.Draw);
            var visuals = new List<RoomBlockAppearance[]>();
            foreach (RoomObjectShotBlockDrawDefinitions.Run run in list.Runs.Span)
            {
                if (CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(CartridgeBankWindow.At(RoomObjectSourceDefinitions.RoomObjectBank, cursor)) != RoomObjectRunHeaderCodec.Encode(run))
                    throw new InvalidDataException(
                        $"Station draw list {list.Draw} differs in source cartridge shape.");
                cursor = unchecked((ushort)(cursor + 2));
                var appearances = new RoomBlockAppearance[run.Count];
                for (int index = 0; index < appearances.Length; index++)
                {
                    RoomTerrainBlock source = RoomLevelWordCodec.Decode(CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(CartridgeBankWindow.At(RoomObjectSourceDefinitions.RoomObjectBank, cursor)));
                    if (source != run.Blocks.Span[index])
                        throw new InvalidDataException(
                            $"Station draw list {list.Draw} differs at ${cursor:X4}.");
                    appearances[index] = source.Appearance;
                    cursor = unchecked((ushort)(cursor + 2));
                }

                if (bus.ReadCartridgeByte(CartridgeBankWindow.At(RoomObjectSourceDefinitions.RoomObjectBank, cursor)) != unchecked((byte)run.Next.X) ||
                    bus.ReadCartridgeByte(CartridgeBankWindow.At(RoomObjectSourceDefinitions.RoomObjectBank, unchecked((ushort)(cursor + 1)))) !=
                        unchecked((byte)run.Next.Y))
                    throw new InvalidDataException(
                        $"Station draw list {list.Draw} differs in next-record offset.");
                cursor = unchecked((ushort)(cursor + 2));
                visuals.Add(appearances);
            }

            entries.Add(new RoomObjectVisualRunEntry(
                VisualId(list.Draw), visuals.ToArray()));
        }

        return entries.ToArray();
    }

    /// <summary>Checks the visual document revision and wraps catalog validation with its source path.</summary>
    private static InstalledRoomObjectStationVisuals CreateCatalog(
        VisualDocument document, string path)
    {
        if (document.Version != FormatVersion || document.Entries is null)
            throw new InvalidDataException($"Station visuals {path} have an incompatible format.");
        try { return Create(document.Entries); }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid station visuals {path}: {error.Message}", error);
        }
    }

    /// <summary>Reads a station document or manifest from disk with path-aware validation errors.</summary>
    private static T ReadJson<T>(string path) => ReadJson<T>(File.ReadAllBytes(path), path);

    /// <summary>Deserializes station JSON bytes and reports malformed or empty content against the supplied path.</summary>
    private static T ReadJson<T>(byte[] bytes, string path)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(bytes, JsonOptions)
                ?? throw new InvalidDataException($"Station JSON {path} is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid station JSON {path}.", error);
        }
    }

    /// <summary>Stock revision, cartridge provenance, and SHA-256 of the exact station visual JSON bytes.</summary>
    private sealed record VisualManifest(int Version, string SourceCartridgeSha256);
    /// <summary>Versioned set of named station draw layouts exported from the source cartridge.</summary>
    private sealed record VisualDocument(int Version, RoomObjectVisualRunEntry[] Entries);
}

using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Installs named, editable station visual block references. Native draw geometry and
/// complete physical level words are checked against the compiled cartridge catalog.
/// </summary>
public static class RoomPlmStationVisualFiles
{
    /// <summary>Family-relative stock/override JSON filename for twenty map, refill, save, and station-access draw layouts.</summary>
    public const string VisualFileName = "stations.json";
    /// <summary>Stock manifest filename recording version-one format, cartridge provenance, and the visual JSON's SHA-256.</summary>
    public const string ManifestFileName = "manifest.json";
    /// <summary>Current manifest and station visual-document schema version.</summary>
    private const int FormatVersion = 1;

    /// <summary>Camel-case JSON settings shared by station provenance and editable visual entries.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    /// <summary>Checks every compiled bank-$84 station draw shape, physical word, and offset before exporting named metatile/flip run arrays.</summary>
    /// <param name="bus">Cartridge source for map, energy, missile, save, and access layouts in their original draw-run order.</param>
    /// <param name="directory">Family directory, created before native checks; receives new visual JSON and manifest files.</param>
    /// <param name="sourceCartridgeSha256">Nonblank importer-supplied provenance hash, recorded without recomputation and required to match the supported identity on stock load.</param>
    /// <remarks>Only visual bits 0..11 are serialized; save-floor cap runs retain native order rather than a top-to-bottom rearrangement. Existing files are refused and separate writes can leave a partial installation.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory or source hash is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Native station draws differ from compiled shapes, physical words, or offsets.</exception>
    /// <exception cref="IOException">An output exists or filesystem access fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        RoomPlmStationVisualEntry[] entries = ReadAndVerifyNative(bus);
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(
            new VisualDocument(FormatVersion, entries), JsonOptions);
        using (var output = new FileStream(Path.Combine(directory, VisualFileName),
                   FileMode.CreateNew, FileAccess.Write))
            output.Write(json);
        using var manifest = new FileStream(Path.Combine(directory, ManifestFileName),
            FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(manifest,
            new VisualManifest(FormatVersion, sourceCartridgeSha256,
                Convert.ToHexString(SHA256.HashData(json))), JsonOptions);
    }

    /// <summary>Validates stock station artwork and selects an optional complete twenty-layout replacement without changing station effects or collision.</summary>
    /// <param name="stockDirectory">Family directory containing version-one stock JSON and its required manifest.</param>
    /// <param name="overrideDirectory">Optional family directory; null or absent visual JSON uses validated stock.</param>
    /// <returns>A ROM-independent catalog with copied authored runs and compiled station mechanics and draw geometry unchanged.</returns>
    /// <remarks>Stock provenance, byte hash, and compiled visual equality are checked before an override. Replacement JSON must contain all named layouts with original run counts/lengths and twelve-bit visual words; no override manifest is needed.</remarks>
    /// <exception cref="ArgumentException"><paramref name="stockDirectory"/> is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Provenance, integrity, schema, identities, coverage, run shape, or visual bits are invalid.</exception>
    /// <exception cref="IOException">Required stock or selected override files cannot be read.</exception>
    public static RoomPlmStationVisualCatalog Load(
        string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, ManifestFileName);
        VisualManifest manifest = ReadJson<VisualManifest>(manifestPath);
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Station visual manifest {manifestPath} is incompatible.");
        string stockPath = Path.Combine(stockDirectory, VisualFileName);
        byte[] stockBytes = File.ReadAllBytes(stockPath);
        if (!string.Equals(Convert.ToHexString(SHA256.HashData(stockBytes)),
                manifest.VisualSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Stock station visuals {stockPath} failed their manifest hash.");
        RoomPlmStationVisualCatalog stock = CreateCatalog(
            ReadJson<VisualDocument>(stockBytes, stockPath), stockPath);
        VerifyStockMatchesCompiled(stock, stockPath);

        string? overridePath = overrideDirectory is null ? null :
            Path.Combine(overrideDirectory, VisualFileName);
        return overridePath is null || !File.Exists(overridePath)
            ? stock
            : CreateCatalog(ReadJson<VisualDocument>(overridePath), overridePath);
    }

    /// <summary>Performs the stock checks from <see cref="Load"/> without overrides, cartridge reads, or file writes.</summary>
    /// <param name="directory">Station family directory containing stock JSON and manifest.</param>
    /// <exception cref="InvalidDataException">Stock provenance, integrity, schema, or compiled appearance checks fail.</exception>
    public static void ValidateStock(string directory) => _ = Load(directory, null);

    /// <summary>Verifies every native station run header, physical level word, and continuation offset while extracting visual bits.</summary>
    /// <param name="bus">Import address space containing the bank-$84 station draw lists.</param>
    /// <returns>All named station entries ordered by native draw-list pointer.</returns>
    private static RoomPlmStationVisualEntry[] ReadAndVerifyNative(ISnesAddressSpace bus)
    {
        var entries = new List<RoomPlmStationVisualEntry>();
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in
                 RoomPlmStationDrawDefinitions.All.OrderBy(item => item.Pointer))
        {
            ushort cursor = list.Pointer;
            var visuals = new List<ushort[]>();
            foreach (RoomPlmShotBlockDrawDefinitions.Run run in list.Runs.Span)
            {
                if (ReadWord(bus, cursor) != run.DirectionAndCount)
                    throw new InvalidDataException(
                        $"Station draw list ${list.Pointer:X4} differs in source cartridge shape.");
                cursor = unchecked((ushort)(cursor + 2));
                var visualWords = new ushort[run.LevelWords.Length];
                for (int index = 0; index < visualWords.Length; index++)
                {
                    ushort source = ReadWord(bus, cursor);
                    if (source != run.LevelWords.Span[index])
                        throw new InvalidDataException(
                            $"Station draw list ${list.Pointer:X4} differs at ${cursor:X4}.");
                    visualWords[index] = new RoomLevelWord(source).VisualWord;
                    cursor = unchecked((ushort)(cursor + 2));
                }

                if (bus.ReadCartridgeByte(0x840000 | cursor) != unchecked((byte)run.NextX) ||
                    bus.ReadCartridgeByte(0x840000 | unchecked((ushort)(cursor + 1))) !=
                        unchecked((byte)run.NextY))
                    throw new InvalidDataException(
                        $"Station draw list ${list.Pointer:X4} differs in next-record offset.");
                cursor = unchecked((ushort)(cursor + 2));
                visuals.Add(visualWords);
            }

            entries.Add(new RoomPlmStationVisualEntry(
                RoomPlmStationDrawDefinitions.VisualId(list.Pointer), visuals.ToArray()));
        }

        return entries.ToArray();
    }

    /// <summary>Ensures each validated stock run word equals the visual portion of its compiled physical station word.</summary>
    /// <param name="catalog">Compiled stock station catalog.</param>
    /// <param name="path">Stock visual filename included in failures.</param>
    private static void VerifyStockMatchesCompiled(RoomPlmStationVisualCatalog catalog,
        string path)
    {
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in
                 RoomPlmStationDrawDefinitions.All)
        for (int run = 0; run < list.Runs.Length; run++)
        for (int word = 0; word < list.Runs.Span[run].LevelWords.Length; word++)
        {
            ushort expected = new RoomLevelWord(
                list.Runs.Span[run].LevelWords.Span[word]).VisualWord;
            if (catalog.GetWord(list.Pointer, run, word) != expected)
                throw new InvalidDataException(
                    $"Stock station visuals {path} differ from compiled frame " +
                    RoomPlmStationDrawDefinitions.VisualId(list.Pointer) + ".");
        }
    }

    /// <summary>Checks document compatibility, compiles complete station coverage, and adds the filename to admission errors.</summary>
    /// <param name="document">Deserialized station visual document.</param>
    /// <param name="path">Source filename included in failures.</param>
    /// <returns>The immutable station visual catalog.</returns>
    private static RoomPlmStationVisualCatalog CreateCatalog(
        VisualDocument document, string path)
    {
        if (document.Version != FormatVersion || document.Entries is null)
            throw new InvalidDataException($"Station visuals {path} have an incompatible format.");
        try { return new RoomPlmStationVisualCatalog(document.Entries); }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid station visuals {path}: {error.Message}", error);
        }
    }

    /// <summary>Reads and deserializes a required station JSON file.</summary>
    /// <typeparam name="T">Expected manifest or visual-document type.</typeparam>
    /// <param name="path">File to read.</param>
    /// <returns>The non-null deserialized document.</returns>
    private static T ReadJson<T>(string path) => ReadJson<T>(File.ReadAllBytes(path), path);

    /// <summary>Deserializes station JSON bytes and normalizes syntax/null failures as invalid asset data.</summary>
    /// <typeparam name="T">Expected manifest or visual-document type.</typeparam>
    /// <param name="bytes">Complete UTF-8 JSON bytes.</param>
    /// <param name="path">Logical source filename included in failures.</param>
    /// <returns>The non-null deserialized document.</returns>
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

    /// <summary>Reads one little-endian word from a bank-$84 station draw-list pointer.</summary>
    /// <param name="bus">Import address space containing the draw list.</param>
    /// <param name="pointer">Sixteen-bit bank-relative word pointer.</param>
    /// <returns>The native word.</returns>
    private static ushort ReadWord(ISnesAddressSpace bus, ushort pointer) =>
        unchecked((ushort)(bus.ReadCartridgeByte(0x840000 | pointer) |
            bus.ReadCartridgeByte(0x840000 | unchecked((ushort)(pointer + 1))) << 8));

    /// <summary>Stock provenance and exact station visual-document byte identity.</summary>
    /// <param name="Version">Manifest schema version.</param>
    /// <param name="SourceCartridgeSha256">Cartridge identity used for extraction.</param>
    /// <param name="VisualSha256">Uppercase SHA-256 of the stock visual JSON bytes.</param>
    private sealed record VisualManifest(int Version, string SourceCartridgeSha256,
        string VisualSha256);
    /// <summary>Versioned complete editable station visual entry collection.</summary>
    /// <param name="Version">Visual-document schema version.</param>
    /// <param name="Entries">Map, refill, save, and access layouts in stable identity order.</param>
    private sealed record VisualDocument(int Version, RoomPlmStationVisualEntry[] Entries);
}

using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Installs named gate-block appearances. Native draw geometry and physical level
/// words are checked against the compiled cartridge definitions.
/// </summary>
public static class RoomPlmDownwardGateVisualFiles
{
    /// <summary>Family-relative stock/override JSON filename for six downward-gate column frames and eight left/right trigger layouts.</summary>
    public const string VisualFileName = "downward-gates.json";
    /// <summary>Stock provenance manifest filename; records format version one, cartridge SHA-256, and the visual JSON's byte hash.</summary>
    public const string ManifestFileName = "manifest.json";
    /// <summary>Schema version accepted for downward-gate visual documents and manifests.</summary>
    private const int FormatVersion = 1;

    /// <summary>Shared camel-case JSON settings used for downward-gate visual files.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    /// <summary>Checks bank-$84 downward-gate draw shapes, physical words, and continuation offsets before exporting metatile/flip words in native run order.</summary>
    /// <param name="bus">Cartridge source for column draws $84:A517–A56A and trigger draws $84:A5D7–A626; upward gates are not included.</param>
    /// <param name="directory">Family directory, created before native validation; receives new visual JSON and manifest files.</param>
    /// <param name="sourceCartridgeSha256">Nonblank caller-supplied provenance hash, recorded without recomputation; stock loading requires the supported cartridge identity.</param>
    /// <remarks>Words contain only bits 0..11, not collision types. Create-new outputs are separate writes and are not rolled back together on failure.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory or source hash is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Native draw data differs from the compiled gate definitions.</exception>
    /// <exception cref="IOException">An output exists or filesystem access fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        RoomPlmDownwardGateVisualEntry[] entries = ReadAndVerifyNative(bus);
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

    /// <summary>Validates stock downward-gate artwork and selects an optional complete fourteen-layout visual replacement.</summary>
    /// <param name="stockDirectory">Family directory containing version-one stock JSON and its required manifest.</param>
    /// <param name="overrideDirectory">Optional family directory; null or an absent visual JSON selects validated stock.</param>
    /// <returns>A ROM-independent catalog with copied authored words and compiled collision, draw placement, and gate timing unchanged.</returns>
    /// <remarks>Stock provenance, file hash, and compiled visual equality are checked before overrides. Overrides require every published identity and original run shape with twelve-bit visual words, but no manifest; legacy trigger color labels remain unchanged.</remarks>
    /// <exception cref="ArgumentException"><paramref name="stockDirectory"/> is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Provenance, integrity, schema, identities, frame coverage, shapes, or visual bits are invalid.</exception>
    /// <exception cref="IOException">Required stock or selected override files cannot be read.</exception>
    public static RoomPlmDownwardGateVisualCatalog Load(
        string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, ManifestFileName);
        VisualManifest manifest = ReadJson<VisualManifest>(manifestPath);
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Downward gate visual manifest {manifestPath} is incompatible.");
        string stockPath = Path.Combine(stockDirectory, VisualFileName);
        byte[] stockBytes = File.ReadAllBytes(stockPath);
        if (!string.Equals(Convert.ToHexString(SHA256.HashData(stockBytes)),
                manifest.VisualSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Stock downward gate visuals {stockPath} failed their manifest hash.");
        RoomPlmDownwardGateVisualCatalog stock = CreateCatalog(
            ReadJson<VisualDocument>(stockBytes, stockPath), stockPath);
        VerifyStockMatchesCompiled(stock, stockPath);

        string? overridePath = overrideDirectory is null ? null :
            Path.Combine(overrideDirectory, VisualFileName);
        return overridePath is null || !File.Exists(overridePath)
            ? stock
            : CreateCatalog(ReadJson<VisualDocument>(overridePath), overridePath);
    }

    /// <summary>Runs stock manifest, schema, integrity, and compiled-appearance checks through <see cref="Load"/> without overrides, cartridge reads, or writes.</summary>
    /// <param name="directory">Downward-gate family directory containing both required stock files.</param>
    /// <exception cref="InvalidDataException">Stock validation fails.</exception>
    public static void ValidateStock(string directory) => _ = Load(directory, null);

    /// <summary>Verifies every compiled downward-gate draw record and extracts its editable visual words.</summary>
    /// <param name="bus">Cartridge address space containing the bank-$84 draw lists.</param>
    /// <returns>Named entries preserving every compiled run and word position.</returns>
    private static RoomPlmDownwardGateVisualEntry[] ReadAndVerifyNative(ISnesAddressSpace bus)
    {
        var entries = new List<RoomPlmDownwardGateVisualEntry>();
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in
                 DownwardGatePlmDrawDefinitions.All.OrderBy(item => item.Pointer))
        {
            ushort cursor = list.Pointer;
            var visuals = new List<ushort[]>();
            foreach (RoomPlmShotBlockDrawDefinitions.Run run in list.Runs.Span)
            {
                if (ReadWord(bus, cursor) != run.DirectionAndCount)
                    throw new InvalidDataException(
                        $"Downward gate draw list ${list.Pointer:X4} differs in source cartridge shape.");
                cursor = unchecked((ushort)(cursor + 2));
                var visualWords = new ushort[run.LevelWords.Length];
                for (int index = 0; index < visualWords.Length; index++)
                {
                    ushort source = ReadWord(bus, cursor);
                    if (source != run.LevelWords.Span[index])
                        throw new InvalidDataException(
                            $"Downward gate draw list ${list.Pointer:X4} differs at ${cursor:X4}.");
                    visualWords[index] = new RoomLevelWord(source).VisualWord;
                    cursor = unchecked((ushort)(cursor + 2));
                }

                if (bus.ReadCartridgeByte(0x840000 | cursor) != unchecked((byte)run.NextX) ||
                    bus.ReadCartridgeByte(0x840000 | unchecked((ushort)(cursor + 1))) !=
                        unchecked((byte)run.NextY))
                    throw new InvalidDataException(
                        $"Downward gate draw list ${list.Pointer:X4} differs in next-record offset.");
                cursor = unchecked((ushort)(cursor + 2));
                visuals.Add(visualWords);
            }

            entries.Add(new RoomPlmDownwardGateVisualEntry(
                DownwardGatePlmDrawDefinitions.VisualId(list.Pointer), visuals.ToArray()));
        }

        return entries.ToArray();
    }

    /// <summary>Requires stock visual words to equal the visual portion of every compiled gate word.</summary>
    /// <param name="catalog">Validated stock catalog to compare.</param>
    /// <param name="path">Stock filename included in mismatch errors.</param>
    private static void VerifyStockMatchesCompiled(RoomPlmDownwardGateVisualCatalog catalog,
        string path)
    {
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in
                 DownwardGatePlmDrawDefinitions.All)
        for (int run = 0; run < list.Runs.Length; run++)
        for (int word = 0; word < list.Runs.Span[run].LevelWords.Length; word++)
        {
            ushort expected = new RoomLevelWord(
                list.Runs.Span[run].LevelWords.Span[word]).VisualWord;
            if (catalog.GetWord(list.Pointer, run, word) != expected)
                throw new InvalidDataException(
                    $"Stock downward gate visuals {path} differ from compiled frame " +
                    DownwardGatePlmDrawDefinitions.VisualId(list.Pointer) + ".");
        }
    }

    /// <summary>Validates a decoded downward-gate document and constructs its immutable catalog.</summary>
    /// <param name="document">Decoded document with versioned entries.</param>
    /// <param name="path">Source filename included in validation errors.</param>
    /// <returns>The validated catalog.</returns>
    private static RoomPlmDownwardGateVisualCatalog CreateCatalog(
        VisualDocument document, string path)
    {
        if (document.Version != FormatVersion || document.Entries is null)
            throw new InvalidDataException($"Downward gate visuals {path} have an incompatible format.");
        try { return new RoomPlmDownwardGateVisualCatalog(document.Entries); }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid downward gate visuals {path}: {error.Message}", error);
        }
    }

    /// <summary>Reads and decodes a required downward-gate JSON file.</summary>
    /// <typeparam name="T">Document type to deserialize.</typeparam>
    /// <param name="path">File to read and identify in errors.</param>
    /// <returns>The decoded non-null document.</returns>
    private static T ReadJson<T>(string path) => ReadJson<T>(File.ReadAllBytes(path), path);
    /// <summary>Decodes downward-gate JSON bytes and translates malformed or empty content into data errors.</summary>
    /// <typeparam name="T">Document type to deserialize.</typeparam>
    /// <param name="bytes">UTF-8 JSON payload.</param>
    /// <param name="path">Logical source filename included in errors.</param>
    /// <returns>The decoded non-null document.</returns>
    private static T ReadJson<T>(byte[] bytes, string path)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(bytes, JsonOptions)
                ?? throw new InvalidDataException($"Downward gate JSON {path} is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid downward gate JSON {path}.", error);
        }
    }

    /// <summary>Reads a little-endian word from a bank-$84 draw-list offset.</summary>
    /// <param name="bus">Cartridge address space to read.</param>
    /// <param name="pointer">Sixteen-bit bank-relative address.</param>
    /// <returns>The decoded word.</returns>
    private static ushort ReadWord(ISnesAddressSpace bus, ushort pointer) =>
        unchecked((ushort)(bus.ReadCartridgeByte(0x840000 | pointer) |
            bus.ReadCartridgeByte(0x840000 | unchecked((ushort)(pointer + 1))) << 8));

    /// <summary>Records the schema, source cartridge identity, and stock visual payload hash.</summary>
    /// <param name="Version">Manifest schema version.</param>
    /// <param name="SourceCartridgeSha256">SHA-256 identity of the extraction cartridge.</param>
    /// <param name="VisualSha256">SHA-256 of the exact stock visual JSON bytes.</param>
    private sealed record VisualManifest(int Version, string SourceCartridgeSha256,
        string VisualSha256);
    /// <summary>Represents the complete editable downward-gate visual document.</summary>
    /// <param name="Version">Visual document schema version.</param>
    /// <param name="Entries">Named entries for all compiled column and trigger layouts.</param>
    private sealed record VisualDocument(int Version, RoomPlmDownwardGateVisualEntry[] Entries);
}

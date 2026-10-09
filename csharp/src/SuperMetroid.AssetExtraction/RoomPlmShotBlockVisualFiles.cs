using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Extracts the visual block references of ordinary shot-block PLM draw lists.
/// The native draw shapes and complete collision-changing level words stay compiled.
/// </summary>
public static class RoomPlmShotBlockVisualFiles
{
    /// <summary>Family-relative stock/override JSON filename for all nineteen ordinary shot-block breakup and restoration draws.</summary>
    public const string VisualFileName = "shot-blocks.json";
    /// <summary>Stock manifest filename recording format version one, cartridge provenance, and the visual JSON's SHA-256.</summary>
    public const string ManifestFileName = "manifest.json";
    /// <summary>Current manifest and shot-block visual-document schema version.</summary>
    private const int FormatVersion = 1;

    /// <summary>Camel-case JSON settings shared by the manifest and editable visual document.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    /// <summary>Verifies ordinary shot-block bank-$84 draw geometry, complete physical words, and continuation offsets before exporting visual-only run arrays.</summary>
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
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        RoomPlmShotBlockVisualEntry[] entries = ReadAndVerifyNative(bus);
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

    /// <summary>Validates all stock shot-block draws before selecting an optional complete visual replacement.</summary>
    /// <param name="stockDirectory">Family directory containing version-one stock JSON and its required provenance/hash manifest.</param>
    /// <param name="overrideDirectory">Optional family directory; null or absent visual JSON uses validated stock.</param>
    /// <returns>A ROM-independent catalog that copies edited run arrays and preserves native collision words, draw placement, timing, and slot effects.</returns>
    /// <remarks>Stock provenance, byte hash, and compiled visual equality are checked first. Override JSON needs all nineteen draw pointers exactly once, original run counts/lengths, and twelve-bit visual words, but no manifest.</remarks>
    /// <exception cref="ArgumentException"><paramref name="stockDirectory"/> is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Provenance, integrity, schema, identities, coverage, run shape, or visual bits are invalid.</exception>
    /// <exception cref="IOException">Required stock or selected override files cannot be read.</exception>
    public static RoomPlmShotBlockVisualCatalog Load(
        string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, ManifestFileName);
        VisualManifest manifest = ReadJson<VisualManifest>(manifestPath);
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Shot-block visual manifest {manifestPath} is incompatible.");
        string stockPath = Path.Combine(stockDirectory, VisualFileName);
        byte[] stockBytes = File.ReadAllBytes(stockPath);
        if (!string.Equals(Convert.ToHexString(SHA256.HashData(stockBytes)),
                manifest.VisualSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Stock shot-block visuals {stockPath} failed their manifest hash.");
        VisualDocument stock = ReadJson<VisualDocument>(stockBytes, stockPath);
        ValidateDocument(stock, stockPath);
        RoomPlmShotBlockVisualCatalog stockCatalog = CreateCatalog(stock, stockPath);
        VerifyStockMatchesCompiled(stockCatalog, stockPath);

        string? overridePath = overrideDirectory is null ? null :
            Path.Combine(overrideDirectory, VisualFileName);
        if (overridePath is null || !File.Exists(overridePath))
            return stockCatalog;
        VisualDocument selected = ReadJson<VisualDocument>(overridePath);
        ValidateDocument(selected, overridePath);
        return CreateCatalog(selected, overridePath);
    }

    /// <summary>Runs stock provenance, hash, schema, and compiled-appearance checks through <see cref="Load"/> without overrides, cartridge access, or writes.</summary>
    /// <param name="directory">Shot-block family directory containing both required stock files.</param>
    /// <exception cref="InvalidDataException">Stock validation fails.</exception>
    public static void ValidateStock(string directory) => _ = Load(directory, null);

    /// <summary>Verifies every native shot-block run header, physical level word, and continuation offset while extracting visual bits.</summary>
    /// <param name="bus">Import address space containing the bank-$84 draw lists.</param>
    /// <returns>Entries ordered by native draw-list pointer.</returns>
    private static RoomPlmShotBlockVisualEntry[] ReadAndVerifyNative(ISnesAddressSpace bus)
    {
        var result = new List<RoomPlmShotBlockVisualEntry>();
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in
                 RoomPlmShotBlockDrawDefinitions.All.OrderBy(item => item.Pointer))
        {
            ushort cursor = list.Pointer;
            var visuals = new List<ushort[]>();
            foreach (RoomPlmShotBlockDrawDefinitions.Run run in list.Runs.Span)
            {
                if (ReadWord(bus, cursor) != run.DirectionAndCount)
                    throw new InvalidDataException(
                        $"Shot-block draw list ${list.Pointer:X4} has a different shape in the source cartridge.");
                cursor = unchecked((ushort)(cursor + 2));
                var visualWords = new ushort[run.LevelWords.Length];
                for (int index = 0; index < visualWords.Length; index++)
                {
                    ushort source = ReadWord(bus, cursor);
                    if (source != run.LevelWords.Span[index])
                        throw new InvalidDataException(
                            $"Shot-block draw list ${list.Pointer:X4} differs at ${cursor:X4}.");
                    visualWords[index] = new RoomLevelWord(source).VisualWord;
                    cursor = unchecked((ushort)(cursor + 2));
                }

                if (bus.ReadCartridgeByte(0x840000 | cursor) != unchecked((byte)run.NextX) ||
                    bus.ReadCartridgeByte(0x840000 | unchecked((ushort)(cursor + 1))) !=
                        unchecked((byte)run.NextY))
                    throw new InvalidDataException(
                        $"Shot-block draw list ${list.Pointer:X4} has a different next-record offset.");
                cursor = unchecked((ushort)(cursor + 2));
                visuals.Add(visualWords);
            }

            result.Add(new RoomPlmShotBlockVisualEntry(list.Pointer, visuals.ToArray()));
        }

        return result.ToArray();
    }

    /// <summary>Ensures validated stock visuals still equal the visual portion of every compiled physical draw-list word.</summary>
    /// <param name="catalog">Compiled stock shot-block catalog.</param>
    /// <param name="path">Stock visual filename included in failures.</param>
    private static void VerifyStockMatchesCompiled(
        RoomPlmShotBlockVisualCatalog catalog, string path)
    {
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in RoomPlmShotBlockDrawDefinitions.All)
        for (int run = 0; run < list.Runs.Length; run++)
        for (int word = 0; word < list.Runs.Span[run].LevelWords.Length; word++)
        {
            ushort expected = new RoomLevelWord(
                list.Runs.Span[run].LevelWords.Span[word]).VisualWord;
            if (catalog.GetWord(list.Pointer, run, word) != expected)
                throw new InvalidDataException(
                    $"Stock shot-block visuals {path} differ from compiled list ${list.Pointer:X4}.");
        }
    }

    /// <summary>Checks the visual document's schema version and required entry collection.</summary>
    /// <param name="document">Deserialized shot-block document.</param>
    /// <param name="path">Source filename included in failures.</param>
    private static void ValidateDocument(VisualDocument document, string path)
    {
        if (document.Version != FormatVersion || document.Entries is null)
            throw new InvalidDataException($"Shot-block visuals {path} have an incompatible format.");
    }

    /// <summary>Compiles a validated document and adds its filename to structural admission errors.</summary>
    /// <param name="document">Validated visual document.</param>
    /// <param name="path">Source filename included in failures.</param>
    /// <returns>The immutable shot-block visual catalog.</returns>
    private static RoomPlmShotBlockVisualCatalog CreateCatalog(
        VisualDocument document, string path)
    {
        try
        {
            return new RoomPlmShotBlockVisualCatalog(document.Entries);
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid shot-block visuals {path}: {error.Message}", error);
        }
    }

    /// <summary>Reads and deserializes a required shot-block JSON file.</summary>
    /// <typeparam name="T">Expected manifest or visual-document type.</typeparam>
    /// <param name="path">File to read.</param>
    /// <returns>The non-null deserialized document.</returns>
    private static T ReadJson<T>(string path) => ReadJson<T>(File.ReadAllBytes(path), path);

    /// <summary>Deserializes already-read shot-block JSON and normalizes syntax/null failures as invalid asset data.</summary>
    /// <typeparam name="T">Expected manifest or visual-document type.</typeparam>
    /// <param name="bytes">Complete UTF-8 JSON bytes.</param>
    /// <param name="path">Logical source filename included in failures.</param>
    /// <returns>The non-null deserialized document.</returns>
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

    /// <summary>Reads one little-endian word from a bank-$84 shot-block draw-list pointer.</summary>
    /// <param name="bus">Import address space containing the draw list.</param>
    /// <param name="pointer">Sixteen-bit bank-relative word pointer.</param>
    /// <returns>The native word.</returns>
    private static ushort ReadWord(ISnesAddressSpace bus, ushort pointer) =>
        unchecked((ushort)(bus.ReadCartridgeByte(0x840000 | pointer) |
            (bus.ReadCartridgeByte(0x840000 | unchecked((ushort)(pointer + 1))) << 8)));

    /// <summary>Stock provenance and exact visual-document byte identity.</summary>
    /// <param name="Version">Manifest schema version.</param>
    /// <param name="SourceCartridgeSha256">Cartridge identity used for extraction.</param>
    /// <param name="VisualSha256">Uppercase SHA-256 of the stock visual JSON bytes.</param>
    private sealed record VisualManifest(int Version, string SourceCartridgeSha256,
        string VisualSha256);
    /// <summary>Versioned complete editable shot-block visual entry collection.</summary>
    /// <param name="Version">Visual-document schema version.</param>
    /// <param name="Entries">All ordinary shot-block draw lists and their visual run words.</param>
    private sealed record VisualDocument(int Version, RoomPlmShotBlockVisualEntry[] Entries);
}

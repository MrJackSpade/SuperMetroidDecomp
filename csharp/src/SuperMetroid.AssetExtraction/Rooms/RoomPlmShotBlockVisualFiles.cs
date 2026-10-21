using SuperMetroid.Core;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Extracts the visual block references of ordinary shot-block PLM draw lists.
/// The native draw shapes and complete collision-changing level words stay compiled.
/// </summary>
[CpuBusBoundary("codec: ROM bank-$84 shot-block PLM draw lists → shot-block visual files")]
public static class RoomPlmShotBlockVisualFiles
{
    /// <summary>Family-relative stock/override JSON filename for all nineteen ordinary shot-block breakup and restoration draws.</summary>
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
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        RoomPlmVisualRunEntry[] entries = ReadAndVerifyNative(bus);
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

    /// <summary>Checks all bank 84 run headers, physical words, and offsets before exporting editable visual bits.</summary>
    private static RoomPlmVisualRunEntry[] ReadAndVerifyNative(ISnesAddressSpace bus)
    {
        var result = new List<RoomPlmVisualRunEntry>();
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in
                 RoomPlmShotBlockDrawDefinitions.All.OrderBy(item => PlmDrawAddressCodec.Of(item.Draw)))
        {
            ushort cursor = PlmDrawAddressCodec.Of(list.Draw);
            var visuals = new List<ushort[]>();
            foreach (RoomPlmShotBlockDrawDefinitions.Run run in list.Runs.Span)
            {
                if (CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(CartridgeBankWindow.At(RoomPlmCartridgeTables.PlmBank, cursor)) != run.DirectionAndCount)
                    throw new InvalidDataException(
                        $"Shot-block draw list {list.Draw} has a different shape in the source cartridge.");
                cursor = unchecked((ushort)(cursor + 2));
                var visualWords = new ushort[run.LevelWords.Length];
                for (int index = 0; index < visualWords.Length; index++)
                {
                    ushort source = CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(CartridgeBankWindow.At(RoomPlmCartridgeTables.PlmBank, cursor));
                    if (source != run.LevelWords.Span[index])
                        throw new InvalidDataException(
                            $"Shot-block draw list {list.Draw} differs at ${cursor:X4}.");
                    visualWords[index] = new RoomLevelWord(source).VisualWord;
                    cursor = unchecked((ushort)(cursor + 2));
                }

                if (bus.ReadCartridgeByte(CartridgeBankWindow.At(RoomPlmCartridgeTables.PlmBank, cursor)) != unchecked((byte)run.Next.X) ||
                    bus.ReadCartridgeByte(CartridgeBankWindow.At(RoomPlmCartridgeTables.PlmBank, unchecked((ushort)(cursor + 1)))) !=
                        unchecked((byte)run.Next.Y))
                    throw new InvalidDataException(
                        $"Shot-block draw list {list.Draw} has a different next-record offset.");
                cursor = unchecked((ushort)(cursor + 2));
                visuals.Add(visualWords);
            }

            result.Add(new RoomPlmVisualRunEntry(RoomPlmShotBlockDrawDefinitions.VisualId(list.Draw), visuals.ToArray()));
        }

        return result.ToArray();
    }

    /// <summary>Rejects stock visual words that differ from the nineteen compiled draw lists.</summary>
    private static void VerifyStockMatchesCompiled(
        RoomPlmShotBlockVisualCatalog catalog, string path)
    {
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in RoomPlmShotBlockDrawDefinitions.All)
        for (int run = 0; run < list.Runs.Length; run++)
        for (int word = 0; word < list.Runs.Span[run].LevelWords.Length; word++)
        {
            ushort expected = new RoomLevelWord(
                list.Runs.Span[run].LevelWords.Span[word]).VisualWord;
            if (catalog.GetWord(list.Draw, run, word) != expected)
                throw new InvalidDataException(
                    $"Stock shot-block visuals {path} differ from compiled list {list.Draw}.");
        }
    }

    /// <summary>Checks the selected document's version and presence of draw entries.</summary>
    private static void ValidateDocument(VisualDocument document, string path)
    {
        if (document.Version != FormatVersion || document.Entries is null)
            throw new InvalidDataException($"Shot-block visuals {path} have an incompatible format.");
    }

    /// <summary>Builds the selected visual catalog and includes the source path in entry errors.</summary>
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
    private sealed record VisualManifest(int Version, string SourceCartridgeSha256,
        string VisualSha256);
    /// <summary>Contains the nineteen ordered visual draw entries and schema version.</summary>
    private sealed record VisualDocument(int Version, RoomPlmVisualRunEntry[] Entries);
}

using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Shared installed-file contract for bounded PLM block-draw layouts. Visual
/// blocks are stored in native run order, while the compiled definitions retain
/// run directions, signed offsets and physical level words.
/// </summary>
internal static class RoomPlmDoorVisualFileCodec
{
    /// <summary>Filename of the stock provenance and visual-file hash manifest within each PLM family directory.</summary>
    internal const string ManifestFileName = "manifest.json";
    /// <summary>Current manifest and visual-document schema version.</summary>
    private const int FormatVersion = 1;

    /// <summary>Strict camel-case JSON settings shared by manifests and editable visual documents.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    /// <summary>One named PLM draw list flattened into native run-order visual block words.</summary>
    /// <param name="Id">Stable visual identity derived from the native draw-list pointer.</param>
    /// <param name="Blocks">Twelve-bit visual words with physical block type and BTS data removed.</param>
    internal sealed record Entry(string Id, ushort[] Blocks);

    /// <summary>Exports a bounded PLM visual family after proving its compiled run structure against the cartridge.</summary>
    /// <param name="bus">Import address space containing the bank-$84 draw lists.</param>
    /// <param name="directory">New stock family directory receiving the visual document and manifest.</param>
    /// <param name="sourceCartridgeSha256">Supported cartridge identity recorded as provenance.</param>
    /// <param name="visualFileName">Family-specific editable visual filename.</param>
    /// <param name="family">Human-readable family name used in diagnostics.</param>
    /// <param name="definitions">Compiled draw lists whose shape and physical words remain authoritative.</param>
    /// <param name="visualId">Maps each native draw-list pointer to its stable document key.</param>
    internal static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256, string visualFileName, string family,
        IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> definitions,
        Func<ushort, string> visualId)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Entry[] entries = ReadAndVerifyNative(bus, family, definitions, visualId);
        Directory.CreateDirectory(directory);
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(
            new VisualDocument(FormatVersion, entries), JsonOptions);
        using (var output = new FileStream(Path.Combine(directory, visualFileName),
                   FileMode.CreateNew, FileAccess.Write))
            output.Write(json);
        using var manifest = new FileStream(Path.Combine(directory, ManifestFileName),
            FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(manifest,
            new VisualManifest(FormatVersion, sourceCartridgeSha256,
                Convert.ToHexString(SHA256.HashData(json))), JsonOptions);
    }

    /// <summary>Validates stock provenance/content, optionally selects an override document, and compiles one PLM visual catalog.</summary>
    /// <typeparam name="TCatalog">Family-specific immutable catalog type.</typeparam>
    /// <param name="stockDirectory">Directory containing the required stock manifest and visual document.</param>
    /// <param name="overrideDirectory">Optional directory containing a same-named complete replacement document.</param>
    /// <param name="visualFileName">Family-specific visual filename.</param>
    /// <param name="family">Human-readable family name used in diagnostics.</param>
    /// <param name="definitions">Compiled draw lists used to verify stock visual words.</param>
    /// <param name="visualId">Maps native draw pointers to stable document keys.</param>
    /// <param name="createCatalog">Compiles validated document entries into the family catalog.</param>
    /// <param name="getWord">Reads one compiled visual word for stock-to-definition comparison.</param>
    /// <returns>The validated stock catalog or selected override catalog.</returns>
    internal static TCatalog Load<TCatalog>(string stockDirectory,
        string? overrideDirectory, string visualFileName, string family,
        IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> definitions,
        Func<ushort, string> visualId,
        Func<Entry[], TCatalog> createCatalog,
        Func<TCatalog, ushort, int, int, ushort> getWord)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, ManifestFileName);
        VisualManifest manifest = ReadJson<VisualManifest>(manifestPath, family);
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(
                $"{family} visual manifest {manifestPath} is incompatible.");
        string stockPath = Path.Combine(stockDirectory, visualFileName);
        byte[] stockBytes = File.ReadAllBytes(stockPath);
        if (!string.Equals(Convert.ToHexString(SHA256.HashData(stockBytes)),
                manifest.VisualSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(
                $"Stock {family} visuals {stockPath} failed their manifest hash.");
        TCatalog stock = CreateCatalog(
            ReadJson<VisualDocument>(stockBytes, stockPath, family), stockPath,
            family, createCatalog);
        VerifyStockMatchesCompiled(stock, stockPath, family, definitions,
            visualId, getWord);

        string? overridePath = overrideDirectory is null ? null :
            Path.Combine(overrideDirectory, visualFileName);
        return overridePath is null || !File.Exists(overridePath)
            ? stock
            : CreateCatalog(ReadJson<VisualDocument>(overridePath, family),
                overridePath, family, createCatalog);
    }

    /// <summary>Checks native run headers, block words, and signed inter-run offsets against compiled definitions while extracting visual bits.</summary>
    /// <param name="bus">Import address space containing the native draw lists.</param>
    /// <param name="family">Family name used in validation failures.</param>
    /// <param name="definitions">Compiled bounded draw-list definitions.</param>
    /// <param name="visualId">Maps each draw pointer to its stable exported identity.</param>
    /// <returns>Serializable entries in ascending native pointer order.</returns>
    private static Entry[] ReadAndVerifyNative(ISnesAddressSpace bus,
        string family,
        IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> definitions,
        Func<ushort, string> visualId)
    {
        var entries = new List<Entry>();
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList draw in
                 definitions.OrderBy(item => item.Pointer))
        {
            ushort pointer = draw.Pointer;
            // Multirow statue and hand layouts need up to five native runs.
            // Keep a bounded structural guard without rejecting those layouts.
            if (draw.Runs.Length is < 1 or > 8)
                throw new InvalidDataException(
                    $"Compiled {family} draw ${pointer:X4} has an invalid run count.");
            var blocks = new List<ushort>();
            ushort cursor = pointer;
            for (int runIndex = 0; runIndex < draw.Runs.Length; runIndex++)
            {
                RoomPlmShotBlockDrawDefinitions.Run run = draw.Runs.Span[runIndex];
                // Kraid's native spike-clear list is one 22-block run. A bounded
                // 32-block allowance admits it without exposing arbitrary layouts.
                if (run.LevelWords.Length is < 1 or > 32 ||
                    (run.DirectionAndCount & 0x7fff) != run.LevelWords.Length ||
                    ReadWord(bus, cursor) != run.DirectionAndCount)
                    throw new InvalidDataException(
                        $"{family} draw ${pointer:X4} differs in run {runIndex} shape.");
                for (int block = 0; block < run.LevelWords.Length; block++)
                {
                    ushort source = ReadWord(bus,
                        checked((ushort)(cursor + 2 + block * 2)));
                    if (source != run.LevelWords.Span[block])
                        throw new InvalidDataException(
                            $"{family} draw ${pointer:X4} differs at run {runIndex}, block {block}.");
                    blocks.Add(new RoomLevelWord(source).VisualWord);
                }
                ushort nativeOffset = ReadWord(bus,
                    checked((ushort)(cursor + 2 + run.LevelWords.Length * 2)));
                ushort compiledOffset = (ushort)(
                    unchecked((byte)run.NextX) |
                    unchecked((byte)run.NextY) << 8);
                if (nativeOffset != compiledOffset ||
                    (runIndex == draw.Runs.Length - 1) != (nativeOffset == 0))
                    throw new InvalidDataException(
                        $"{family} draw ${pointer:X4} differs at run {runIndex} offset.");
                cursor = checked((ushort)(cursor + 4 + run.LevelWords.Length * 2));
            }
            entries.Add(new Entry(visualId(pointer), blocks.ToArray()));
        }
        return entries.ToArray();
    }

    /// <summary>Ensures every stock visual word equals the visual portion of its compiled physical level word.</summary>
    /// <typeparam name="TCatalog">Family-specific catalog type.</typeparam>
    /// <param name="catalog">Compiled stock catalog to inspect.</param>
    /// <param name="path">Stock filename included in failures.</param>
    /// <param name="family">Family name included in failures.</param>
    /// <param name="definitions">Authoritative compiled draw structures.</param>
    /// <param name="visualId">Maps draw pointers to diagnostic identities.</param>
    /// <param name="getWord">Reads one run/block visual word from <paramref name="catalog"/>.</param>
    private static void VerifyStockMatchesCompiled<TCatalog>(TCatalog catalog,
        string path, string family,
        IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> definitions,
        Func<ushort, string> visualId,
        Func<TCatalog, ushort, int, int, ushort> getWord)
    {
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList draw in definitions)
        {
            for (int runIndex = 0; runIndex < draw.Runs.Length; runIndex++)
            {
                RoomPlmShotBlockDrawDefinitions.Run run = draw.Runs.Span[runIndex];
                for (int block = 0; block < run.LevelWords.Length; block++)
                {
                    ushort expected = new RoomLevelWord(
                        run.LevelWords.Span[block]).VisualWord;
                    if (getWord(catalog, draw.Pointer, runIndex, block) != expected)
                        throw new InvalidDataException(
                            $"Stock {family} visuals {path} differ from compiled frame " +
                            visualId(draw.Pointer) + $" run {runIndex}, block {block}.");
                }
            }
        }
    }

    /// <summary>Checks document version/coverage and wraps family-specific admission errors with the source filename.</summary>
    /// <typeparam name="TCatalog">Family-specific catalog type.</typeparam>
    /// <param name="document">Deserialized visual document.</param>
    /// <param name="path">Document filename used in errors.</param>
    /// <param name="family">Family name used in errors.</param>
    /// <param name="createCatalog">Family compiler that validates entry identities, shapes, and words.</param>
    /// <returns>The admitted immutable catalog.</returns>
    private static TCatalog CreateCatalog<TCatalog>(VisualDocument document,
        string path, string family, Func<Entry[], TCatalog> createCatalog)
    {
        if (document.Version != FormatVersion || document.Entries is null)
            throw new InvalidDataException(
                $"{family} visuals {path} have an incompatible format.");
        try { return createCatalog(document.Entries); }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid {family} visuals {path}: {error.Message}", error);
        }
    }

    /// <summary>Reads and strictly deserializes a required family JSON file.</summary>
    /// <typeparam name="T">Expected manifest or visual-document type.</typeparam>
    /// <param name="path">File to read.</param>
    /// <param name="family">Family name used in errors.</param>
    /// <returns>The non-null deserialized document.</returns>
    private static T ReadJson<T>(string path, string family) =>
        ReadJson<T>(File.ReadAllBytes(path), path, family);

    /// <summary>Strictly deserializes already-read family JSON bytes and normalizes JSON errors as invalid asset data.</summary>
    /// <typeparam name="T">Expected manifest or visual-document type.</typeparam>
    /// <param name="bytes">Complete UTF-8 JSON bytes.</param>
    /// <param name="path">Logical source filename used in errors.</param>
    /// <param name="family">Family name used in errors.</param>
    /// <returns>The non-null deserialized document.</returns>
    private static T ReadJson<T>(byte[] bytes, string path, string family)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(bytes, JsonOptions)
                ?? throw new InvalidDataException($"{family} JSON {path} is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid {family} JSON {path}.", error);
        }
    }

    /// <summary>Reads one little-endian word from a bank-$84 PLM draw-list pointer.</summary>
    /// <param name="bus">Import address space containing the draw list.</param>
    /// <param name="pointer">Sixteen-bit bank-relative word pointer.</param>
    /// <returns>The native word.</returns>
    private static ushort ReadWord(ISnesAddressSpace bus, ushort pointer) =>
        (ushort)(bus.ReadCartridgeByte(0x840000 | pointer) |
            bus.ReadCartridgeByte(0x840000 | checked((ushort)(pointer + 1))) << 8);

    /// <summary>Stock provenance and byte identity for one PLM family visual document.</summary>
    /// <param name="Version">Manifest schema version.</param>
    /// <param name="SourceCartridgeSha256">Cartridge identity from which stock content was extracted.</param>
    /// <param name="VisualSha256">Uppercase SHA-256 of the exact stock visual JSON bytes.</param>
    private sealed record VisualManifest(int Version, string SourceCartridgeSha256,
        string VisualSha256);
    /// <summary>Versioned editable collection of every visual entry in one PLM family.</summary>
    /// <param name="Version">Visual-document schema version.</param>
    /// <param name="Entries">Complete family entries keyed by stable visual identity.</param>
    private sealed record VisualDocument(int Version, Entry[] Entries);
}

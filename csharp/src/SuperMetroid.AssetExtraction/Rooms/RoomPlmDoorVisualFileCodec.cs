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
/// Shared installed-file contract for bounded PLM block-draw layouts. Visual
/// blocks are stored in native run order, while the compiled definitions retain
/// run directions, signed offsets and physical level words.
/// </summary>
[CpuBusBoundary("codec: ROM bank-$84 PLM block-draw lists → installed PLM door visual files")]
internal static class RoomPlmDoorVisualFileCodec
{
    /// <summary>Stock manifest name containing cartridge and extracted visual hashes.</summary>
    internal const string ManifestFileName = "manifest.json";
    /// <summary>Schema version shared by the stock manifest and editable visual document.</summary>
    private const int FormatVersion = 1;

    /// <summary>Uses camel-case field names and readable output for installed PLM visual JSON.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    /// <summary>One draw-list identity and its editable block visuals in native run order.</summary>
    internal sealed record Entry(string Id, ushort[] Blocks);

    /// <summary>Verifies native PLM run geometry and level words before writing a fresh stock visual file and hash manifest.</summary>
    internal static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256, string visualFileName, string family,
        IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> definitions,
        Func<PlmDraw, string> visualId)
    {
        Ensure.NotNull(bus);
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

    /// <summary>Validates stock provenance and compiled parity, then selects an optional edited visual catalog.</summary>
    internal static TCatalog Load<TCatalog>(string stockDirectory, string? overrideDirectory,
        RoomPlmVisualFamily<TCatalog> visuals)
    {
        Ensure.NotNull(visuals);
        string visualFileName = visuals.FileName;
        string family = visuals.Name;
        IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> definitions = visuals.Definitions;
        Func<PlmDraw, string> visualId = visuals.VisualId;
        Func<Entry[], TCatalog> createCatalog = visuals.CreateCatalog;
        Func<TCatalog, PlmDraw, int, int, ushort> getWord = visuals.GetWord;
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

    /// <summary>Checks bounded bank-$84 run counts, directions, offsets, and level words while extracting only visual bits.</summary>
    private static Entry[] ReadAndVerifyNative(ISnesAddressSpace bus,
        string family,
        IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> definitions,
        Func<PlmDraw, string> visualId)
    {
        var entries = new List<Entry>();
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList draw in
                 definitions.OrderBy(item => PlmDrawAddressCodec.Of(item.Draw)))
        {
            ushort pointer = PlmDrawAddressCodec.Of(draw.Draw);
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
                    (run.DirectionAndCount & 0x7fff) != run.LevelWords.Length || // cartridge-encoding: allow(Field) - the run word's low fifteen bits count its blocks; bit 15 is the direction
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
                ushort compiledOffset =
                    SnesLittleEndian.Word(unchecked((byte)run.Next.X), unchecked((byte)run.Next.Y));
                if (nativeOffset != compiledOffset ||
                    (runIndex == draw.Runs.Length - 1) != (nativeOffset == 0))
                    throw new InvalidDataException(
                        $"{family} draw ${pointer:X4} differs at run {runIndex} offset.");
                cursor = checked((ushort)(cursor + 4 + run.LevelWords.Length * 2));
            }
            entries.Add(new Entry(visualId(draw.Draw), blocks.ToArray()));
        }
        return entries.ToArray();
    }

    /// <summary>Requires every stock visual block to match the compiled draw-list source word.</summary>
    private static void VerifyStockMatchesCompiled<TCatalog>(TCatalog catalog,
        string path, string family,
        IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> definitions,
        Func<PlmDraw, string> visualId,
        Func<TCatalog, PlmDraw, int, int, ushort> getWord)
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
                    if (getWord(catalog, draw.Draw, runIndex, block) != expected)
                        throw new InvalidDataException(
                            $"Stock {family} visuals {path} differ from compiled frame " +
                            visualId(draw.Draw) + $" run {runIndex}, block {block}.");
                }
            }
        }
    }

    /// <summary>Rejects incompatible JSON and wraps catalog validation with the selected file path.</summary>
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

    /// <summary>Reads a selected PLM visual file before applying the shared JSON error boundary.</summary>
    private static T ReadJson<T>(string path, string family) =>
        ReadJson<T>(File.ReadAllBytes(path), path, family);

    /// <summary>Parses PLM visual JSON and reports malformed or empty documents with their file path.</summary>
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

    /// <summary>Reads a bank-$84 PLM word while rejecting a pointer that straddles the bank end.</summary>
    private static ushort ReadWord(ISnesAddressSpace bus, ushort pointer) =>
        pointer == ushort.MaxValue
            ? throw new InvalidDataException("A PLM visual word cannot start at the PLM bank's last byte.")
            : CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(CartridgeBankWindow.At(RoomPlmCartridgeTables.PlmBank, pointer));

    /// <summary>Stock provenance binding a visual JSON hash to the supported cartridge.</summary>
    private sealed record VisualManifest(int Version, string SourceCartridgeSha256,
        string VisualSha256);
    /// <summary>Versioned collection of editable block visuals extracted from compiled draw lists.</summary>
    private sealed record VisualDocument(int Version, Entry[] Entries);
}

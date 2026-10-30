using SuperMetroid.Core;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Shared installed-file contract for bounded PLM block-draw layouts. Block appearances (block number and mirroring)
/// are stored in run order, while the compiled definitions retain run directions, signed offsets and terrain. The
/// cartridge's block are decoded once, here, by <see cref="RoomLevelWordCodec"/>.
/// </summary>
internal static class RoomPlmDoorVisualFileCodec
{
    /// <summary>Stock manifest name containing cartridge and extracted visual hashes.</summary>
    internal const string ManifestFileName = "manifest.json";
    /// <summary>Schema version shared by the stock manifest and editable visual document.</summary>
    private const int FormatVersion = 1;

    /// <summary>Uses camel-case field names, mirroring names and readable output for installed PLM visual JSON.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>One draw-list identity and its editable block appearances in run order.</summary>
    internal sealed record Entry(string Id, RoomBlockAppearance[] Blocks);

    /// <summary>Verifies the cartridge's PLM run geometry and blocks before writing a fresh stock visual file and hash manifest.</summary>
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
        Func<TCatalog, PlmDraw, int, int, RoomBlockAppearance?> getAppearance = visuals.GetAppearance;
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
            visualId, getAppearance);

        string? overridePath = overrideDirectory is null ? null :
            Path.Combine(overrideDirectory, visualFileName);
        return overridePath is null || !File.Exists(overridePath)
            ? stock
            : CreateCatalog(ReadJson<VisualDocument>(overridePath, family),
                overridePath, family, createCatalog);
    }

    /// <summary>Checks bounded bank-$84 run counts, directions, offsets, and decoded blocks while extracting only their appearances.</summary>
    private static Entry[] ReadAndVerifyNative(ISnesAddressSpace bus,
        string family,
        IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> definitions,
        Func<PlmDraw, string> visualId)
    {
        var entries = new List<Entry>();
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList draw in
                 definitions.OrderBy(item => PlmDrawAddressCodec.Of(item.Draw)))
        {
            int pointer = PlmDrawAddressCodec.Of(draw.Draw);
            // Multirow statue and hand layouts need up to five native runs.
            // Keep a bounded structural guard without rejecting those layouts.
            if (draw.Runs.Length is < 1 or > 8)
                throw new InvalidDataException(
                    $"Compiled {family} draw ${pointer:X4} has an invalid run count.");
            var blocks = new List<RoomBlockAppearance>();
            int cursor = pointer;
            for (int runIndex = 0; runIndex < draw.Runs.Length; runIndex++)
            {
                RoomPlmShotBlockDrawDefinitions.Run run = draw.Runs.Span[runIndex];
                // Kraid's native spike-clear list is one 22-block run. A bounded
                // 32-block allowance admits it without exposing arbitrary layouts.
                if (run.Count is < 1 or > 32 ||
                    ReadWord(bus, cursor) != PlmRunHeaderCodec.Encode(run))
                    throw new InvalidDataException(
                        $"{family} draw ${pointer:X4} differs in run {runIndex} shape.");
                for (int block = 0; block < run.Count; block++)
                {
                    RoomTerrainBlock source = RoomLevelWordCodec.Decode(ReadWord(bus,
                        checked((ushort)(cursor + 2 + block * 2))));
                    if (source != run.Blocks.Span[block])
                        throw new InvalidDataException(
                            $"{family} draw ${pointer:X4} differs at run {runIndex}, block {block}.");
                    blocks.Add(source.Appearance);
                }
                int nativeOffset = ReadWord(bus,
                    checked((ushort)(cursor + 2 + run.Count * 2)));
                int compiledOffset =
                    LittleEndianBytes.Word(unchecked((byte)run.Next.X), unchecked((byte)run.Next.Y));
                if (nativeOffset != compiledOffset ||
                    (runIndex == draw.Runs.Length - 1) != (nativeOffset == 0))
                    throw new InvalidDataException(
                        $"{family} draw ${pointer:X4} differs at run {runIndex} offset.");
                cursor = checked((ushort)(cursor + 4 + run.Count * 2));
            }
            entries.Add(new Entry(visualId(draw.Draw), blocks.ToArray()));
        }
        return entries.ToArray();
    }

    /// <summary>Requires every stock appearance to match the compiled draw's block.</summary>
    private static void VerifyStockMatchesCompiled<TCatalog>(TCatalog catalog,
        string path, string family,
        IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> definitions,
        Func<PlmDraw, string> visualId,
        Func<TCatalog, PlmDraw, int, int, RoomBlockAppearance?> getAppearance)
    {
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList draw in definitions)
        {
            for (int runIndex = 0; runIndex < draw.Runs.Length; runIndex++)
            {
                RoomPlmShotBlockDrawDefinitions.Run run = draw.Runs.Span[runIndex];
                for (int block = 0; block < run.Count; block++)
                {
                    RoomBlockAppearance expected = run.Blocks.Span[block].Appearance;
                    if (getAppearance(catalog, draw.Draw, runIndex, block) != expected)
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
    private static ushort ReadWord(ISnesAddressSpace bus, int pointer) =>
        pointer == ushort.MaxValue
            ? throw new InvalidDataException("A PLM visual word cannot start at the PLM bank's last byte.")
            : CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(CartridgeBankWindow.At(RoomPlmSourceDefinitions.PlmBank, pointer));

    /// <summary>Stock provenance binding a visual JSON hash to the supported cartridge.</summary>
    private sealed record VisualManifest(int Version, string SourceCartridgeSha256,
        string VisualSha256);
    /// <summary>Versioned collection of editable block appearances extracted from compiled draw lists.</summary>
    private sealed record VisualDocument(int Version, Entry[] Entries);
}

using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Shared installed-file contract for door PLMs with one bounded draw run.
/// The family catalogs validate complete IDs and each family's block shape.
/// </summary>
internal static class RoomPlmDoorVisualFileCodec
{
    internal const string ManifestFileName = "manifest.json";
    private const int FormatVersion = 1;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    internal sealed record Entry(string Id, ushort[] Blocks);

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

    internal static TCatalog Load<TCatalog>(string stockDirectory,
        string? overrideDirectory, string visualFileName, string family,
        IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> definitions,
        Func<ushort, string> visualId,
        Func<Entry[], TCatalog> createCatalog,
        Func<TCatalog, ushort, int, ushort> getWord)
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
            if (draw.Runs.Length != 1 ||
                draw.Runs.Span[0].LevelWords.Length is < 1 or > 4 ||
                (draw.Runs.Span[0].DirectionAndCount & 0x7fff) !=
                draw.Runs.Span[0].LevelWords.Length)
                throw new InvalidDataException(
                    $"Compiled {family} draw ${pointer:X4} has an invalid block shape.");
            RoomPlmShotBlockDrawDefinitions.Run run = draw.Runs.Span[0];
            if (ReadWord(bus, pointer) != run.DirectionAndCount)
                throw new InvalidDataException(
                    $"{family} draw ${pointer:X4} differs in source cartridge shape.");
            var blocks = new ushort[run.LevelWords.Length];
            for (int block = 0; block < blocks.Length; block++)
            {
                ushort source = ReadWord(bus, checked((ushort)(pointer + 2 + block * 2)));
                if (source != run.LevelWords.Span[block])
                    throw new InvalidDataException(
                        $"{family} draw ${pointer:X4} differs at block {block}.");
                blocks[block] = new RoomLevelWord(source).VisualWord;
            }
            if (ReadWord(bus, checked((ushort)(pointer + 2 + blocks.Length * 2))) != 0)
                throw new InvalidDataException(
                    $"{family} draw ${pointer:X4} lacks its zero terminator.");
            entries.Add(new Entry(visualId(pointer), blocks));
        }
        return entries.ToArray();
    }

    private static void VerifyStockMatchesCompiled<TCatalog>(TCatalog catalog,
        string path, string family,
        IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> definitions,
        Func<ushort, string> visualId,
        Func<TCatalog, ushort, int, ushort> getWord)
    {
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList draw in definitions)
        for (int block = 0; block < draw.Runs.Span[0].LevelWords.Length; block++)
        {
            ushort expected = new RoomLevelWord(
                draw.Runs.Span[0].LevelWords.Span[block]).VisualWord;
            if (getWord(catalog, draw.Pointer, block) != expected)
                throw new InvalidDataException(
                    $"Stock {family} visuals {path} differ from compiled frame " +
                    visualId(draw.Pointer) + ".");
        }
    }

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

    private static T ReadJson<T>(string path, string family) =>
        ReadJson<T>(File.ReadAllBytes(path), path, family);

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

    private static ushort ReadWord(ISnesAddressSpace bus, ushort pointer) =>
        (ushort)(bus.ReadByte(0x840000 | pointer) |
            bus.ReadByte(0x840000 | checked((ushort)(pointer + 1))) << 8);

    private sealed record VisualManifest(int Version, string SourceCartridgeSha256,
        string VisualSha256);
    private sealed record VisualDocument(int Version, Entry[] Entries);
}

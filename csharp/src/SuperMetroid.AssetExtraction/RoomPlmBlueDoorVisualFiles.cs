using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Extracts replaceable blue-door visual block references after proving the sixteen
/// native physical draw lists agree with the compiled cartridge definitions.
/// </summary>
public static class RoomPlmBlueDoorVisualFiles
{
    public const string VisualFileName = "blue-doors.json";
    public const string ManifestFileName = "manifest.json";
    private const int FormatVersion = 1;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        RoomPlmBlueDoorVisualEntry[] entries = ReadAndVerifyNative(bus);
        Directory.CreateDirectory(directory);
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

    public static RoomPlmBlueDoorVisualCatalog Load(
        string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, ManifestFileName);
        VisualManifest manifest = ReadJson<VisualManifest>(manifestPath);
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(
                $"Blue-door visual manifest {manifestPath} is incompatible.");
        string stockPath = Path.Combine(stockDirectory, VisualFileName);
        byte[] stockBytes = File.ReadAllBytes(stockPath);
        if (!string.Equals(Convert.ToHexString(SHA256.HashData(stockBytes)),
                manifest.VisualSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(
                $"Stock blue-door visuals {stockPath} failed their manifest hash.");
        RoomPlmBlueDoorVisualCatalog stock = CreateCatalog(
            ReadJson<VisualDocument>(stockBytes, stockPath), stockPath);
        VerifyStockMatchesCompiled(stock, stockPath);

        string? overridePath = overrideDirectory is null ? null :
            Path.Combine(overrideDirectory, VisualFileName);
        return overridePath is null || !File.Exists(overridePath)
            ? stock
            : CreateCatalog(ReadJson<VisualDocument>(overridePath), overridePath);
    }

    public static void ValidateStock(string directory) => _ = Load(directory, null);

    private static RoomPlmBlueDoorVisualEntry[] ReadAndVerifyNative(ISnesAddressSpace bus)
    {
        var entries = new List<RoomPlmBlueDoorVisualEntry>();
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList draw in
                 BlueDoorPlmDrawDefinitions.All.OrderBy(item => item.Pointer))
        {
            ushort pointer = draw.Pointer;
            RoomPlmShotBlockDrawDefinitions.Run run = draw.Runs.Span[0];
            if (ReadWord(bus, pointer) != run.DirectionAndCount)
                throw new InvalidDataException(
                    $"Blue-door draw ${pointer:X4} differs in source cartridge shape.");
            var blocks = new ushort[run.LevelWords.Length];
            for (int block = 0; block < blocks.Length; block++)
            {
                ushort source = ReadWord(bus, checked((ushort)(pointer + 2 + block * 2)));
                if (source != run.LevelWords.Span[block])
                    throw new InvalidDataException(
                        $"Blue-door draw ${pointer:X4} differs at block {block}.");
                blocks[block] = new RoomLevelWord(source).VisualWord;
            }
            if (ReadWord(bus, checked((ushort)(pointer + 10))) != 0)
                throw new InvalidDataException(
                    $"Blue-door draw ${pointer:X4} lacks its zero terminator.");
            entries.Add(new RoomPlmBlueDoorVisualEntry(
                BlueDoorPlmDrawDefinitions.VisualId(pointer), blocks));
        }
        return entries.ToArray();
    }

    private static void VerifyStockMatchesCompiled(RoomPlmBlueDoorVisualCatalog catalog,
        string path)
    {
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList draw in
                 BlueDoorPlmDrawDefinitions.All)
        for (int block = 0; block < draw.Runs.Span[0].LevelWords.Length; block++)
        {
            ushort expected = new RoomLevelWord(
                draw.Runs.Span[0].LevelWords.Span[block]).VisualWord;
            if (catalog.GetWord(draw.Pointer, block) != expected)
                throw new InvalidDataException(
                    $"Stock blue-door visuals {path} differ from compiled frame " +
                    BlueDoorPlmDrawDefinitions.VisualId(draw.Pointer) + ".");
        }
    }

    private static RoomPlmBlueDoorVisualCatalog CreateCatalog(
        VisualDocument document, string path)
    {
        if (document.Version != FormatVersion || document.Entries is null)
            throw new InvalidDataException(
                $"Blue-door visuals {path} have an incompatible format.");
        try { return new RoomPlmBlueDoorVisualCatalog(document.Entries); }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid blue-door visuals {path}: {error.Message}", error);
        }
    }

    private static T ReadJson<T>(string path) => ReadJson<T>(File.ReadAllBytes(path), path);

    private static T ReadJson<T>(byte[] bytes, string path)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(bytes, JsonOptions)
                ?? throw new InvalidDataException($"Blue-door JSON {path} is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid blue-door JSON {path}.", error);
        }
    }

    private static ushort ReadWord(ISnesAddressSpace bus, ushort pointer) =>
        (ushort)(bus.ReadByte(0x840000 | pointer) |
            bus.ReadByte(0x840000 | checked((ushort)(pointer + 1))) << 8);

    private sealed record VisualManifest(int Version, string SourceCartridgeSha256,
        string VisualSha256);
    private sealed record VisualDocument(int Version, RoomPlmBlueDoorVisualEntry[] Entries);
}

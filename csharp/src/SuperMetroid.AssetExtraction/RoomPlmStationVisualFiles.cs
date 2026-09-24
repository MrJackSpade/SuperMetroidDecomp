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
    public const string VisualFileName = "stations.json";
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

    public static void ValidateStock(string directory) => _ = Load(directory, null);

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

                if (bus.ReadByte(0x840000 | cursor) != unchecked((byte)run.NextX) ||
                    bus.ReadByte(0x840000 | unchecked((ushort)(cursor + 1))) !=
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

    private static T ReadJson<T>(string path) => ReadJson<T>(File.ReadAllBytes(path), path);

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

    private static ushort ReadWord(ISnesAddressSpace bus, ushort pointer) =>
        unchecked((ushort)(bus.ReadByte(0x840000 | pointer) |
            bus.ReadByte(0x840000 | unchecked((ushort)(pointer + 1))) << 8));

    private sealed record VisualManifest(int Version, string SourceCartridgeSha256,
        string VisualSha256);
    private sealed record VisualDocument(int Version, RoomPlmStationVisualEntry[] Entries);
}

using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Installs named elevator-platform appearances. Native draw geometry and physical level
/// words are checked against the compiled cartridge definitions.
/// </summary>
public static class RoomPlmElevatorPlatformVisualFiles
{
    public const string VisualFileName = "elevator-platforms.json";
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
        RoomPlmElevatorPlatformVisualEntry[] entries = ReadAndVerifyNative(bus);
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

    public static RoomPlmElevatorPlatformVisualCatalog Load(
        string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, ManifestFileName);
        VisualManifest manifest = ReadJson<VisualManifest>(manifestPath);
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Elevator-platform visual manifest {manifestPath} is incompatible.");
        string stockPath = Path.Combine(stockDirectory, VisualFileName);
        byte[] stockBytes = File.ReadAllBytes(stockPath);
        if (!string.Equals(Convert.ToHexString(SHA256.HashData(stockBytes)),
                manifest.VisualSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Stock elevator-platform visuals {stockPath} failed their manifest hash.");
        RoomPlmElevatorPlatformVisualCatalog stock = CreateCatalog(
            ReadJson<VisualDocument>(stockBytes, stockPath), stockPath);
        VerifyStockMatchesCompiled(stock, stockPath);

        string? overridePath = overrideDirectory is null ? null :
            Path.Combine(overrideDirectory, VisualFileName);
        return overridePath is null || !File.Exists(overridePath)
            ? stock
            : CreateCatalog(ReadJson<VisualDocument>(overridePath), overridePath);
    }

    public static void ValidateStock(string directory) => _ = Load(directory, null);

    private static RoomPlmElevatorPlatformVisualEntry[] ReadAndVerifyNative(ISnesAddressSpace bus)
    {
        var entries = new List<RoomPlmElevatorPlatformVisualEntry>();
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in
                 ElevatorPlatformPlmDefinitions.DrawLists.OrderBy(item => item.Pointer))
        {
            ushort cursor = list.Pointer;
            var visuals = new List<ushort[]>();
            foreach (RoomPlmShotBlockDrawDefinitions.Run run in list.Runs.Span)
            {
                if (ReadWord(bus, cursor) != run.DirectionAndCount)
                    throw new InvalidDataException(
                        $"Elevator-platform draw list ${list.Pointer:X4} differs in source cartridge shape.");
                cursor = unchecked((ushort)(cursor + 2));
                var visualWords = new ushort[run.LevelWords.Length];
                for (int index = 0; index < visualWords.Length; index++)
                {
                    ushort source = ReadWord(bus, cursor);
                    if (source != run.LevelWords.Span[index])
                        throw new InvalidDataException(
                            $"Elevator-platform draw list ${list.Pointer:X4} differs at ${cursor:X4}.");
                    visualWords[index] = new RoomLevelWord(source).VisualWord;
                    cursor = unchecked((ushort)(cursor + 2));
                }

                if (bus.ReadByte(0x840000 | cursor) != unchecked((byte)run.NextX) ||
                    bus.ReadByte(0x840000 | unchecked((ushort)(cursor + 1))) !=
                        unchecked((byte)run.NextY))
                    throw new InvalidDataException(
                        $"Elevator-platform draw list ${list.Pointer:X4} differs in next-record offset.");
                cursor = unchecked((ushort)(cursor + 2));
                visuals.Add(visualWords);
            }

            entries.Add(new RoomPlmElevatorPlatformVisualEntry(
                ElevatorPlatformPlmDefinitions.VisualId(list.Pointer), visuals.ToArray()));
        }

        return entries.ToArray();
    }

    private static void VerifyStockMatchesCompiled(RoomPlmElevatorPlatformVisualCatalog catalog,
        string path)
    {
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in
                 ElevatorPlatformPlmDefinitions.DrawLists)
        for (int run = 0; run < list.Runs.Length; run++)
        for (int word = 0; word < list.Runs.Span[run].LevelWords.Length; word++)
        {
            ushort expected = new RoomLevelWord(
                list.Runs.Span[run].LevelWords.Span[word]).VisualWord;
            if (catalog.GetWord(list.Pointer, run, word) != expected)
                throw new InvalidDataException(
                    $"Stock elevator-platform visuals {path} differ from compiled frame " +
                    ElevatorPlatformPlmDefinitions.VisualId(list.Pointer) + ".");
        }
    }

    private static RoomPlmElevatorPlatformVisualCatalog CreateCatalog(
        VisualDocument document, string path)
    {
        if (document.Version != FormatVersion || document.Entries is null)
            throw new InvalidDataException($"Elevator-platform visuals {path} have an incompatible format.");
        try { return new RoomPlmElevatorPlatformVisualCatalog(document.Entries); }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid elevator-platform visuals {path}: {error.Message}", error);
        }
    }

    private static T ReadJson<T>(string path) => ReadJson<T>(File.ReadAllBytes(path), path);
    private static T ReadJson<T>(byte[] bytes, string path)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(bytes, JsonOptions)
                ?? throw new InvalidDataException($"Elevator-platform JSON {path} is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid elevator-platform JSON {path}.", error);
        }
    }

    private static ushort ReadWord(ISnesAddressSpace bus, ushort pointer) =>
        unchecked((ushort)(bus.ReadByte(0x840000 | pointer) |
            bus.ReadByte(0x840000 | unchecked((ushort)(pointer + 1))) << 8));

    private sealed record VisualManifest(int Version, string SourceCartridgeSha256,
        string VisualSha256);
    private sealed record VisualDocument(int Version, RoomPlmElevatorPlatformVisualEntry[] Entries);
}

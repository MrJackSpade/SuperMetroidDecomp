using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Installs named collectible block art while checking all 24 native draw records
/// and the eight dynamic-slot selectors against compiled physical definitions.
/// </summary>
public static class RoomPlmCollectibleVisualFiles
{
    public const string VisualFileName = "collectibles.json";
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
        RoomPlmCollectibleVisualEntry[] entries = ReadAndVerifyNative(bus);
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

    public static RoomPlmCollectibleVisualCatalog Load(
        string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, ManifestFileName);
        VisualManifest manifest = ReadJson<VisualManifest>(manifestPath);
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(
                $"Collectible visual manifest {manifestPath} is incompatible.");
        string stockPath = Path.Combine(stockDirectory, VisualFileName);
        byte[] stockBytes = File.ReadAllBytes(stockPath);
        if (!string.Equals(Convert.ToHexString(SHA256.HashData(stockBytes)),
                manifest.VisualSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(
                $"Stock collectible visuals {stockPath} failed their manifest hash.");
        RoomPlmCollectibleVisualCatalog stock = CreateCatalog(
            ReadJson<VisualDocument>(stockBytes, stockPath), stockPath);
        VerifyStockMatchesCompiled(stock, stockPath);

        string? overridePath = overrideDirectory is null ? null :
            Path.Combine(overrideDirectory, VisualFileName);
        return overridePath is null || !File.Exists(overridePath)
            ? stock
            : CreateCatalog(ReadJson<VisualDocument>(overridePath), overridePath);
    }

    public static void ValidateStock(string directory) => _ = Load(directory, null);

    private static RoomPlmCollectibleVisualEntry[] ReadAndVerifyNative(ISnesAddressSpace bus)
    {
        for (int slot = 0; slot < 4; slot++)
        for (int animation = 0; animation < 2; animation++)
        {
            ushort table = animation == 0
                ? RoomPlmCollectibleDrawDefinitions.DynamicFrame0Table
                : RoomPlmCollectibleDrawDefinitions.DynamicFrame1Table;
            ushort native = ReadWord(bus, checked((ushort)(table + slot * 2)));
            ushort compiled = RoomPlmCollectibleDrawDefinitions.VisibleFrame(
                InWorldCollectibleKind.Bombs, animation, slot);
            if (native != compiled)
                throw new InvalidDataException(
                    $"Dynamic collectible selector $84:{table + slot * 2:X4} differs from the cartridge.");
        }

        var entries = new List<RoomPlmCollectibleVisualEntry>();
        foreach (RoomPlmCollectibleDrawFrame frame in RoomPlmCollectibleDrawDefinitions.All)
        {
            if (ReadWord(bus, frame.Pointer) != 1 ||
                ReadWord(bus, checked((ushort)(frame.Pointer + 2))) != frame.LevelWord ||
                ReadWord(bus, checked((ushort)(frame.Pointer + 4))) != 0)
                throw new InvalidDataException(
                    $"Collectible draw list $84:{frame.Pointer:X4} differs from compiled geometry or level word.");
            entries.Add(new RoomPlmCollectibleVisualEntry(frame.Id,
                new RoomLevelWord(frame.LevelWord).VisualWord));
        }
        return entries.ToArray();
    }

    private static void VerifyStockMatchesCompiled(RoomPlmCollectibleVisualCatalog catalog,
        string path)
    {
        foreach (RoomPlmCollectibleDrawFrame frame in RoomPlmCollectibleDrawDefinitions.All)
            if (catalog.GetWord(frame.Pointer) !=
                new RoomLevelWord(frame.LevelWord).VisualWord)
                throw new InvalidDataException(
                    $"Stock collectible visuals {path} differ from compiled frame {frame.Id}.");
    }

    private static RoomPlmCollectibleVisualCatalog CreateCatalog(
        VisualDocument document, string path)
    {
        if (document.Version != FormatVersion || document.Entries is null)
            throw new InvalidDataException(
                $"Collectible visuals {path} have an incompatible format.");
        try { return new RoomPlmCollectibleVisualCatalog(document.Entries); }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid collectible visuals {path}: {error.Message}", error);
        }
    }

    private static T ReadJson<T>(string path) => ReadJson<T>(File.ReadAllBytes(path), path);

    private static T ReadJson<T>(byte[] bytes, string path)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(bytes, JsonOptions)
                ?? throw new InvalidDataException($"Collectible JSON {path} is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid collectible JSON {path}.", error);
        }
    }

    private static ushort ReadWord(ISnesAddressSpace bus, ushort pointer) =>
        unchecked((ushort)(bus.ReadByte(0x840000 | pointer) |
            bus.ReadByte(0x840000 | unchecked((ushort)(pointer + 1))) << 8));

    private sealed record VisualManifest(int Version, string SourceCartridgeSha256,
        string VisualSha256);
    private sealed record VisualDocument(int Version,
        RoomPlmCollectibleVisualEntry[] Entries);
}

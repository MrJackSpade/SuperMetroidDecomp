using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Installs editable Grapple-block visual references while verifying every native
/// source draw record against the immutable collision/shape catalog.
/// </summary>
public static class RoomPlmGrappleBlockVisualFiles
{
    public const string VisualFileName = "grapple-blocks.json";
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
        RoomPlmGrappleBlockVisualEntry[] entries = ReadAndVerifyNative(bus);
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

    public static RoomPlmGrappleBlockVisualCatalog Load(
        string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, ManifestFileName);
        VisualManifest manifest = ReadJson<VisualManifest>(manifestPath);
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(
                $"Grapple-block visual manifest {manifestPath} is incompatible.");
        string stockPath = Path.Combine(stockDirectory, VisualFileName);
        byte[] stockBytes = File.ReadAllBytes(stockPath);
        if (!string.Equals(Convert.ToHexString(SHA256.HashData(stockBytes)),
                manifest.VisualSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(
                $"Stock Grapple-block visuals {stockPath} failed their manifest hash.");
        RoomPlmGrappleBlockVisualCatalog stock = CreateCatalog(
            ReadJson<VisualDocument>(stockBytes, stockPath), stockPath);
        foreach (RoomPlmGrappleBlockDrawDefinitions.DrawList draw in
                 RoomPlmGrappleBlockDrawDefinitions.All)
        {
            ushort expected = new RoomLevelWord(draw.LevelWord).VisualWord;
            if (stock.GetWord(draw.Pointer) != expected)
                throw new InvalidDataException(
                    $"Stock Grapple-block visuals {stockPath} differ from ${draw.Pointer:X4}.");
        }

        string? overridePath = overrideDirectory is null ? null :
            Path.Combine(overrideDirectory, VisualFileName);
        return overridePath is null || !File.Exists(overridePath)
            ? stock
            : CreateCatalog(ReadJson<VisualDocument>(overridePath), overridePath);
    }

    public static void ValidateStock(string directory) => _ = Load(directory, null);

    private static RoomPlmGrappleBlockVisualEntry[] ReadAndVerifyNative(ISnesAddressSpace bus)
    {
        var entries = new List<RoomPlmGrappleBlockVisualEntry>();
        foreach (RoomPlmGrappleBlockDrawDefinitions.DrawList draw in
                 RoomPlmGrappleBlockDrawDefinitions.All.OrderBy(item => item.Pointer))
        {
            ushort pointer = draw.Pointer;
            if (ReadWord(bus, pointer) !=
                    RoomPlmGrappleBlockDrawDefinitions.DrawList.DirectionAndCount ||
                ReadWord(bus, unchecked((ushort)(pointer + 2))) != draw.LevelWord ||
                bus.ReadByte(0x840000 | unchecked((ushort)(pointer + 4))) !=
                    RoomPlmGrappleBlockDrawDefinitions.DrawList.NextX ||
                bus.ReadByte(0x840000 | unchecked((ushort)(pointer + 5))) !=
                    RoomPlmGrappleBlockDrawDefinitions.DrawList.NextY)
                throw new InvalidDataException(
                    $"Grapple-block draw list ${pointer:X4} differs from compiled cartridge data.");
            entries.Add(new RoomPlmGrappleBlockVisualEntry(pointer,
                new RoomLevelWord(draw.LevelWord).VisualWord));
        }

        return entries.ToArray();
    }

    private static RoomPlmGrappleBlockVisualCatalog CreateCatalog(
        VisualDocument document, string path)
    {
        if (document.Version != FormatVersion || document.Entries is null)
            throw new InvalidDataException(
                $"Grapple-block visuals {path} have an incompatible format.");
        try { return new RoomPlmGrappleBlockVisualCatalog(document.Entries); }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException(
                $"Invalid Grapple-block visuals {path}: {error.Message}", error);
        }
    }

    private static T ReadJson<T>(string path) => ReadJson<T>(File.ReadAllBytes(path), path);

    private static T ReadJson<T>(byte[] bytes, string path)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(bytes, JsonOptions)
                ?? throw new InvalidDataException($"Grapple-block JSON {path} is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid Grapple-block JSON {path}.", error);
        }
    }

    private static ushort ReadWord(ISnesAddressSpace bus, ushort pointer) =>
        unchecked((ushort)(bus.ReadByte(0x840000 | pointer) |
            bus.ReadByte(0x840000 | unchecked((ushort)(pointer + 1))) << 8));

    private sealed record VisualManifest(int Version, string SourceCartridgeSha256,
        string VisualSha256);
    private sealed record VisualDocument(int Version, RoomPlmGrappleBlockVisualEntry[] Entries);
}

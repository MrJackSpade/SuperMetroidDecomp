using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts the two authored direct small-OBJ atmospheric lists as editable JSON.</summary>
public static class SamusAtmosphericArtworkFiles
{
    public const string ArtworkFileName = "samus-atmosphere.json";
    public const string ManifestFileName = "samus-atmosphere-manifest.json";
    private const int FormatVersion = 1;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Directory.CreateDirectory(directory);
        int table = SamusMovementRomData.Environment.AtmosphericSpriteAttributeListPointers;
        // Only nonzero, authored pointers become extracted assets. Type two's zero
        // pointer is an intentional mutable-memory read, not a missing art record.
        AssertPointer(bus, table, 1, SamusMovementRomData.Environment.TypeOneAtmosphericAttributes);
        AssertPointer(bus, table, 2, 0);
        foreach (int type in new[] { 4, 6, 7 })
            AssertPointer(bus, table, type,
                SamusMovementRomData.Environment.SharedAtmosphericAttributes);
        var document = new ArtworkDocument(FormatVersion,
            ReadList(bus, SamusMovementRomData.Environment.TypeOneAtmosphericAttributes),
            ReadList(bus, SamusMovementRomData.Environment.SharedAtmosphericAttributes));
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        File.WriteAllBytes(Path.Combine(directory, ArtworkFileName), bytes);
        File.WriteAllBytes(Path.Combine(directory, ManifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(new ArtworkManifest(FormatVersion,
                sourceCartridgeSha256, Convert.ToHexString(SHA256.HashData(bytes))), JsonOptions));
        _ = Load(directory, null);
    }

    public static SamusAtmosphericArtworkCatalog Load(string stockDirectory, string? overrideDirectory)
    {
        ArtworkManifest manifest = Read<ArtworkManifest>(Path.Combine(stockDirectory, ManifestFileName));
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Samus atmospheric artwork manifest does not match the pinned cartridge.");
        byte[] stock = File.ReadAllBytes(Path.Combine(stockDirectory, ArtworkFileName));
        if (!string.Equals(Convert.ToHexString(SHA256.HashData(stock)), manifest.ArtworkSha256,
            StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Stock Samus atmospheric artwork failed its manifest hash.");
        _ = Build(Read<ArtworkDocument>(stock), "stock");
        string? overridePath = overrideDirectory is null ? null :
            Path.Combine(overrideDirectory, ArtworkFileName);
        return overridePath is not null && File.Exists(overridePath)
            ? Build(Read<ArtworkDocument>(File.ReadAllBytes(overridePath)), overridePath)
            : Build(Read<ArtworkDocument>(stock), "stock");
    }

    private static SamusAtmosphericArtworkCatalog Build(ArtworkDocument document, string source)
    {
        if (document.Version != FormatVersion || document.TypeOne is null ||
            document.SharedTypeFour is null)
            throw new InvalidDataException($"Samus atmospheric artwork {source} has invalid structure.");
        return new SamusAtmosphericArtworkCatalog(document.TypeOne, document.SharedTypeFour);
    }

    private static ushort[] ReadList(ISnesAddressSpace bus, ushort pointer)
    {
        var words = new ushort[SamusMovementRomData.Environment.DirectAtmosphericFrameCount];
        for (int frame = 0; frame < words.Length; frame++)
            words[frame] = ReadWord(bus, SamusMovementRomData.Banks.Movement | pointer + frame * 2);
        return words;
    }

    private static void AssertPointer(ISnesAddressSpace bus, int table, int type, ushort expected)
    {
        ushort actual = ReadWord(bus, table + type * 2);
        if (actual != expected)
            throw new InvalidDataException(
                $"Atmospheric type {type} points to ${actual:X4}, expected ${expected:X4}.");
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private static T Read<T>(string path) => Read<T>(File.ReadAllBytes(path));

    private static T Read<T>(byte[] bytes)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(bytes, JsonOptions) ??
                throw new InvalidDataException($"Samus atmospheric {typeof(T).Name} is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid Samus atmospheric {typeof(T).Name}.", error);
        }
    }

    private sealed record ArtworkDocument(int Version, ushort[] TypeOne, ushort[] SharedTypeFour);
    private sealed record ArtworkManifest(int Version, string SourceCartridgeSha256, string ArtworkSha256);
}

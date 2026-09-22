using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Hash-checked stock RGB5 room palettes plus persistent user-selected replacements.</summary>
public static class RoomStaticPaletteArtworkFiles
{
    public const string ManifestFileName = "room-palettes.json";
    private const int FormatVersion = 1;

    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        IReadOnlyDictionary<string, byte[]> files = RoomStaticPaletteExtractor.Extract(bus);
        var hashes = new Dictionary<string, string>();
        foreach ((string name, byte[] bytes) in files)
        {
            using (var output = new FileStream(Path.Combine(directory, name), FileMode.CreateNew, FileAccess.Write))
                output.Write(bytes);
            hashes.Add(name, Convert.ToHexString(SHA256.HashData(bytes)));
        }
        using var manifest = new FileStream(Path.Combine(directory, ManifestFileName),
            FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(manifest,
            new RoomPaletteFileManifest(FormatVersion, sourceCartridgeSha256, hashes), JsonOptions);
    }

    public static RoomStaticPaletteCatalog Load(string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, ManifestFileName);
        RoomPaletteFileManifest manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<RoomPaletteFileManifest>(
                File.ReadAllBytes(manifestPath), JsonOptions)
                ?? throw new InvalidDataException("Room palette manifest is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid room palette manifest {manifestPath}.", error);
        }
        Dictionary<string, int> sources = ExpectedSources();
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase) ||
            manifest.Sha256 is null || manifest.Sha256.Count != sources.Count ||
            sources.Keys.Any(name => !manifest.Sha256.ContainsKey(name)))
            throw new InvalidDataException($"Room palette manifest {manifestPath} does not describe this installation.");

        var selected = new Dictionary<int, RoomStaticPalette>();
        foreach ((string name, int address) in sources)
        {
            string stockPath = Path.Combine(stockDirectory, name);
            byte[] stock = File.ReadAllBytes(stockPath);
            string stockHash = Convert.ToHexString(SHA256.HashData(stock));
            if (!string.Equals(stockHash, manifest.Sha256[name], StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Stock room palette {stockPath} failed its manifest hash.");

            string? overridePath = overrideDirectory is null ? null : Path.Combine(overrideDirectory, name);
            string selectedPath = overridePath is not null && File.Exists(overridePath)
                ? overridePath : stockPath;
            byte[] bytes = selectedPath == stockPath ? stock : File.ReadAllBytes(selectedPath);
            try
            {
                selected.Add(address, RoomStaticPalette.Load(new MemoryStream(bytes, writable: false)));
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid room palette {selectedPath}: {error.Message}", error);
            }
        }
        return new RoomStaticPaletteCatalog(selected);
    }

    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);

    private static Dictionary<string, int> ExpectedSources()
    {
        var sources = new Dictionary<string, int>();
        for (byte graphicsSet = 0; graphicsSet < RoomTilesetDefinitions.Count; graphicsSet++)
        {
            int address = RoomTilesetDefinitions.Get(graphicsSet).PaletteAddress;
            sources[RoomStaticPaletteFormat.SourceFileName(address)] = address;
        }
        return sources;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    private sealed record RoomPaletteFileManifest(
        int Version, string SourceCartridgeSha256, Dictionary<string, string> Sha256);
}

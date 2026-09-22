using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Hash-checked stock visual block definitions and persistent editable JSON overrides.</summary>
public static class RoomMetatileArtworkFiles
{
    public const string ManifestFileName = "room-blocks.json";
    private const int FormatVersion = 1;

    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        IReadOnlyDictionary<string, byte[]> files = RoomMetatileExtractor.Extract(bus);
        var entries = new Dictionary<string, RoomMetatileFileEntry>();
        foreach ((string name, int address) in ExpectedSources())
        {
            byte[] json = files[name];
            int nativeLength = RomDataReader.Decompress(bus, address).Length;
            RoomMetatileFormat.ValidateBlockCount(nativeLength);
            using (var output = new FileStream(Path.Combine(directory, name), FileMode.CreateNew, FileAccess.Write))
                output.Write(json);
            entries.Add(name, new RoomMetatileFileEntry(nativeLength,
                Convert.ToHexString(SHA256.HashData(json))));
        }
        using var manifest = new FileStream(Path.Combine(directory, ManifestFileName),
            FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(manifest,
            new RoomMetatileFileManifest(FormatVersion, sourceCartridgeSha256, entries), JsonOptions);
    }

    /// <summary>Validates stock provenance and selected complete visual block resources without a ROM.</summary>
    public static RoomMetatileCatalog Load(string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, ManifestFileName);
        RoomMetatileFileManifest manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<RoomMetatileFileManifest>(
                File.ReadAllBytes(manifestPath), JsonOptions)
                ?? throw new InvalidDataException("Room metatile manifest is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid room metatile manifest {manifestPath}.", error);
        }
        Dictionary<string, int> sources = ExpectedSources();
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase) ||
            manifest.Entries is null || manifest.Entries.Count != sources.Count ||
            sources.Keys.Any(name => !manifest.Entries.ContainsKey(name)))
            throw new InvalidDataException($"Room metatile manifest {manifestPath} does not describe this installation.");

        RoomMetatileAtlas? cre = null;
        var selected = new Dictionary<int, RoomMetatileAtlas>();
        foreach ((string name, int address) in sources)
        {
            RoomMetatileFileEntry entry = manifest.Entries[name];
            RoomMetatileFormat.ValidateBlockCount(entry.NativeByteCount);
            string stockPath = Path.Combine(stockDirectory, name);
            byte[] stock = File.ReadAllBytes(stockPath);
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(stock)), entry.Sha256,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Stock room metatile JSON {stockPath} failed its manifest hash.");
            string? overridePath = overrideDirectory is null ? null : Path.Combine(overrideDirectory, name);
            string selectedPath = overridePath is not null && File.Exists(overridePath)
                ? overridePath : stockPath;
            byte[] json = selectedPath == stockPath ? stock : File.ReadAllBytes(selectedPath);
            RoomMetatileAtlas atlas;
            try
            {
                atlas = RoomMetatileAtlas.Load(new MemoryStream(json, writable: false),
                    entry.NativeByteCount);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid room metatile JSON {selectedPath}: {error.Message}", error);
            }
            if (name == RoomMetatileFormat.CreFileName) cre = atlas;
            else selected.Add(address, atlas);
        }
        return new RoomMetatileCatalog(
            cre ?? throw new InvalidDataException("Room metatile manifest has no CRE resource."), selected);
    }

    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);

    private static Dictionary<string, int> ExpectedSources()
    {
        var sources = new Dictionary<string, int>
        {
            [RoomMetatileFormat.CreFileName] = RoomAssetRomData.Tilesets.CreBlockDefinitionsAddress,
        };
        for (byte graphicsSet = 0; graphicsSet < RoomTilesetDefinitions.Count; graphicsSet++)
        {
            int address = RoomTilesetDefinitions.Get(graphicsSet).BlockDefinitionsAddress;
            sources[RoomMetatileFormat.SourceFileName(address)] = address;
        }
        return sources;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    private sealed record RoomMetatileFileEntry(int NativeByteCount, string Sha256);
    private sealed record RoomMetatileFileManifest(int Version, string SourceCartridgeSha256,
        Dictionary<string, RoomMetatileFileEntry> Entries);
}

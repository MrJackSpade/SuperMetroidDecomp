using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Hash-checked stock visual block definitions and persistent editable JSON overrides.</summary>
public static class RoomMetatileArtworkFiles
{
    /// <summary>Stock visual-block manifest filename recording cartridge provenance, metatile JSON hashes, and native transfer lengths.</summary>
    public const string ManifestFileName = "room-blocks.json";
    private const int FormatVersion = 1;

    /// <summary>Creates stock visual metatile JSON for CRE and each distinct room block-table source, with a provenance manifest.</summary>
    /// <param name="bus">Non-null cartridge import address space containing the compressed visual block-definition tables.</param>
    /// <param name="directory">Output directory, created if needed; resource and manifest filenames must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Nonempty cartridge SHA-256 stored with each file's hash and native block-table byte count.</param>
    /// <remarks>Checks exact native-byte roundtrips for four-quadrant visual compositions. Collision and block behavior remain outside these resources; player overrides are not accessed.</remarks>
    /// <exception cref="IOException">An output file already exists or filesystem output fails.</exception>
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
            int nativeLength = RomDataReader.Decompress(CartridgeImportSource.Require(bus), address).Length;
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
        using var manifestStream = File.OpenRead(manifestPath);
        RoomMetatileFileManifest manifest = JsonAssetDocument.Read<RoomMetatileFileManifest>(
            manifestStream, JsonOptions, $"room metatile manifest {manifestPath}");
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
            using PooledFileBytes stock = PooledFileBytes.Read(stockPath);
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(stock.Span)), entry.Sha256,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Stock room metatile JSON {stockPath} failed its manifest hash.");
            string? overridePath = overrideDirectory is null ? null : Path.Combine(overrideDirectory, name);
            string selectedPath = overridePath is not null && File.Exists(overridePath)
                ? overridePath : stockPath;
            using PooledFileBytes? edited = selectedPath == stockPath ? null : PooledFileBytes.Read(selectedPath);
            PooledFileBytes json = edited ?? stock;
            RoomMetatileAtlas atlas;
            try
            {
                using MemoryStream jsonStream = json.OpenRead();
                atlas = RoomMetatileAtlas.Load(jsonStream, entry.NativeByteCount);
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

    /// <summary>Validates the complete stock CRE and room metatile resources with player overrides disabled.</summary>
    /// <param name="stockDirectory">Directory containing the visual-block manifest and stock JSON resources.</param>
    /// <remarks>Checks supported cartridge provenance, expected source coverage, file hashes, block counts, and BG-cell fields without cartridge access or file writes.</remarks>
    /// <exception cref="InvalidDataException">Stock provenance, coverage, dimensions, hashes, or visual composition data are invalid.</exception>
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
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectRequiredConstructorParameters = true,
        WriteIndented = true,
    };

    private sealed record RoomMetatileFileEntry(int NativeByteCount, string Sha256);
    private sealed record RoomMetatileFileManifest(int Version, string SourceCartridgeSha256,
        Dictionary<string, RoomMetatileFileEntry> Entries);
}

using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Hash-checked stock BG tilemaps and persistent editable tile-reference overrides.</summary>
public static class RoomBackgroundTilemapArtworkFiles
{
    /// <summary>Stock manifest filename recording every library-background source address, native transfer length, and JSON hash.</summary>
    public const string ManifestFileName = "room-backgrounds.json";
    /// <summary>Schema version accepted for room-background stock manifests.</summary>
    private const int FormatVersion = 1;

    /// <summary>Creates one editable tilemap JSON file per retail compressed library-background source and records its provenance.</summary>
    /// <param name="bus">Non-null cartridge import address space supplying the compiled background sources.</param>
    /// <param name="directory">Output directory, created if needed; resource and manifest filenames must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Nonempty cartridge SHA-256 stored with source addresses, one- or two-page transfer lengths, and file hashes.</param>
    /// <remarks>Preserves ordered 32x32 BG pages with exact native-word roundtrips; does not read or write player overrides.</remarks>
    /// <exception cref="IOException">An output file already exists or filesystem output fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        IReadOnlyDictionary<string, byte[]> files = RoomBackgroundTilemapExtractor.Extract(bus);
        var entries = new Dictionary<string, RoomBackgroundFileEntry>();
        foreach (int address in RoomBackgroundTilemapSources.All)
        {
            string name = RoomBackgroundTilemapFormat.SourceFileName(address);
            byte[] json = files[name];
            int nativeLength = RomDataReader.Decompress(CartridgeImportSource.Require(bus), address).Length;
            RoomBackgroundTilemapFormat.ValidatePageCount(nativeLength);
            using (var output = new FileStream(Path.Combine(directory, name), FileMode.CreateNew, FileAccess.Write))
                output.Write(json);
            entries.Add(name, new RoomBackgroundFileEntry(address, nativeLength,
                Convert.ToHexString(SHA256.HashData(json))));
        }
        using var manifest = new FileStream(Path.Combine(directory, ManifestFileName),
            FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(manifest,
            new RoomBackgroundFileManifest(FormatVersion, sourceCartridgeSha256, entries), JsonOptions);
    }

    /// <summary>Checks stock background provenance and compiles each selected tilemap resource without cartridge access.</summary>
    /// <param name="stockDirectory">Directory containing the supported-cartridge manifest and all hash-checked stock JSON files.</param>
    /// <param name="overrideDirectory">Optional directory of same-named complete JSON replacements; null or a missing replacement selects that stock file.</param>
    /// <returns>A catalog owning the selected ordered BG transfer bytes, keyed by immutable native source address.</returns>
    /// <remarks>Always checks each stock hash before selection, then validates the chosen document against its manifest transfer length. An override cannot alter source identities or page count.</remarks>
    /// <exception cref="InvalidDataException">The manifest, source coverage, stock hash, transfer length, or selected tilemap document is invalid.</exception>
    public static RoomBackgroundTilemapCatalog Load(string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, ManifestFileName);
        using var manifestStream = File.OpenRead(manifestPath);
        RoomBackgroundFileManifest manifest = JsonAssetDocument.Read<RoomBackgroundFileManifest>(
            manifestStream, JsonOptions, $"room background manifest {manifestPath}");
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase) ||
            manifest.Entries is null ||
            manifest.Entries.Count != RoomBackgroundTilemapFormat.RetailCompressedSourceCount)
            throw new InvalidDataException(
                $"Room background manifest {manifestPath} does not describe this installation.");

        var selected = new Dictionary<int, RoomBackgroundTilemapAtlas>();
        foreach ((string name, RoomBackgroundFileEntry entry) in manifest.Entries)
        {
            if (entry is null || name != RoomBackgroundTilemapFormat.SourceFileName(entry.SourceAddress) ||
                !RoomBackgroundTilemapSources.Contains(entry.SourceAddress))
                throw new InvalidDataException($"Room background manifest {manifestPath} has an invalid source entry.");
            RoomBackgroundTilemapFormat.ValidatePageCount(entry.NativeByteCount);
            string stockPath = Path.Combine(stockDirectory, name);
            using PooledFileBytes stock = PooledFileBytes.Read(stockPath);
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(stock.Span)), entry.Sha256,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Stock room background {stockPath} failed its manifest hash.");
            string? overridePath = overrideDirectory is null ? null : Path.Combine(overrideDirectory, name);
            string selectedPath = overridePath is not null && File.Exists(overridePath)
                ? overridePath : stockPath;
            using PooledFileBytes? edited = selectedPath == stockPath ? null : PooledFileBytes.Read(selectedPath);
            PooledFileBytes json = edited ?? stock;
            RoomBackgroundTilemapAtlas atlas;
            try
            {
                using MemoryStream jsonStream = json.OpenRead();
                atlas = RoomBackgroundTilemapAtlas.Load(jsonStream, entry.NativeByteCount);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid room background {selectedPath}: {error.Message}", error);
            }
            if (!selected.TryAdd(entry.SourceAddress, atlas))
                throw new InvalidDataException($"Room background manifest {manifestPath} repeats a source.");
        }
        return new RoomBackgroundTilemapCatalog(selected);
    }

    /// <summary>Checks the background manifest, hashes, and all stock tilemap documents with overrides disabled.</summary>
    /// <param name="stockDirectory">Installed background-art directory to validate without cartridge reads or file writes.</param>
    /// <exception cref="InvalidDataException">Stock provenance, coverage, transfer sizes, hashes, or tilemap content are invalid.</exception>
    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);

    /// <summary>Strict camel-case JSON settings shared by manifest serialization and parsing.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectRequiredConstructorParameters = true,
        WriteIndented = true,
    };

    /// <summary>Records one background tilemap source and the integrity metadata for its stock JSON.</summary>
    /// <param name="SourceAddress">Immutable cartridge source address used as the runtime identity.</param>
    /// <param name="NativeByteCount">Expected decoded transfer size, which determines the page count.</param>
    /// <param name="Sha256">SHA-256 of the exact stock JSON bytes.</param>
    private sealed record RoomBackgroundFileEntry(int SourceAddress, int NativeByteCount, string Sha256);
    /// <summary>Describes the complete installed set of stock room-background tilemaps.</summary>
    /// <param name="Version">Manifest schema version.</param>
    /// <param name="SourceCartridgeSha256">SHA-256 identity of the extraction cartridge.</param>
    /// <param name="Entries">Stock file metadata keyed by source-derived filename.</param>
    private sealed record RoomBackgroundFileManifest(int Version, string SourceCartridgeSha256,
        Dictionary<string, RoomBackgroundFileEntry> Entries);
}

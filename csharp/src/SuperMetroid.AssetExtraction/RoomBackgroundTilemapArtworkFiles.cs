using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Hash-checked stock BG tilemaps and persistent editable tile-reference overrides.</summary>
public static class RoomBackgroundTilemapArtworkFiles
{
    public const string ManifestFileName = "room-backgrounds.json";
    private const int FormatVersion = 1;

    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        IReadOnlyDictionary<string, byte[]> files = RoomBackgroundTilemapExtractor.Extract(bus);
        int[] sources = LibraryBackgroundSourceInventory.Scan(bus)
            .Where(source => source.Command == LibraryBackgroundCommand.DecompressToWorkRam)
            .Select(source => source.SourceAddress).Distinct().Order().ToArray();
        var entries = new Dictionary<string, RoomBackgroundFileEntry>();
        foreach (int address in sources)
        {
            string name = RoomBackgroundTilemapFormat.SourceFileName(address);
            byte[] json = files[name];
            int nativeLength = RomDataReader.Decompress(bus, address).Length;
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

    public static RoomBackgroundTilemapCatalog Load(string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, ManifestFileName);
        RoomBackgroundFileManifest manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<RoomBackgroundFileManifest>(
                File.ReadAllBytes(manifestPath), JsonOptions)
                ?? throw new InvalidDataException("Room background manifest is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid room background manifest {manifestPath}.", error);
        }
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
                entry.SourceAddress < RoomAssetRomData.LibraryBackground.RomSourceAddressFloor)
                throw new InvalidDataException($"Room background manifest {manifestPath} has an invalid source entry.");
            RoomBackgroundTilemapFormat.ValidatePageCount(entry.NativeByteCount);
            string stockPath = Path.Combine(stockDirectory, name);
            byte[] stock = File.ReadAllBytes(stockPath);
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(stock)), entry.Sha256,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Stock room background {stockPath} failed its manifest hash.");
            string? overridePath = overrideDirectory is null ? null : Path.Combine(overrideDirectory, name);
            string selectedPath = overridePath is not null && File.Exists(overridePath)
                ? overridePath : stockPath;
            byte[] json = selectedPath == stockPath ? stock : File.ReadAllBytes(selectedPath);
            RoomBackgroundTilemapAtlas atlas;
            try
            {
                atlas = RoomBackgroundTilemapAtlas.Load(
                    new MemoryStream(json, writable: false), entry.NativeByteCount);
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

    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    private sealed record RoomBackgroundFileEntry(int SourceAddress, int NativeByteCount, string Sha256);
    private sealed record RoomBackgroundFileManifest(int Version, string SourceCartridgeSha256,
        Dictionary<string, RoomBackgroundFileEntry> Entries);
}

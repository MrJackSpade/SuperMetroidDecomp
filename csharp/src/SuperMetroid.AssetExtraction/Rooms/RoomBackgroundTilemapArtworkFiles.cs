using SuperMetroid.AssetExtraction.Assets;
using SuperMetroid.Core;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Installation;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The library backgrounds' tilemaps as import data (#1638): each cell's placed character is the import's input to the
/// background's picture, and the game reads none of them, so the tilemaps and their manifest go to the component's
/// import-data directory (<see cref="ImportDataDirectory.Of"/>), which the installer deletes before it publishes. A room's
/// background changes through its pictures in the installed block sheets.
/// </summary>
public static class RoomBackgroundTilemapArtworkFiles
{
    /// <summary>Import-data manifest filename recording every background tilemap, its length, and its JSON hash.</summary>
    public const string ManifestFileName = "room-backgrounds.json";
    /// <summary>
    /// Manifest revision; 2 names each tilemap for its background rather than its cartridge address (#1414), and 3 writes the
    /// tilemaps as import data, not installed (#1638).
    /// </summary>
    private const int FormatVersion = 3;

    /// <summary>The import-data JSON file name of <paramref name="tilemap"/>, named for the background it pictures (#1414).</summary>
    public static string FileName(RoomBackgroundTilemap tilemap) =>
        $"room-background-{InstalledArtworkNames.Hyphenated(tilemap.ToString())}.json";

    /// <summary>Creates one tilemap JSON file per background tilemap in the component's import data and records its provenance.</summary>
    /// <param name="bus">Non-null cartridge import address space supplying the compressed background tilemaps.</param>
    /// <param name="directory">The component's directory; the tilemaps and manifest go to its import-data directory, created if needed, where their filenames must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Nonempty cartridge SHA-256 stored with the tilemaps, their one- or two-page lengths, and file hashes.</param>
    /// <remarks>Preserves ordered 32x32 BG pages; reads no earlier tilemap.</remarks>
    /// <exception cref="IOException">An output file already exists or filesystem output fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        string importData = ImportDataDirectory.Of(directory);
        Directory.CreateDirectory(importData);
        IReadOnlyDictionary<string, byte[]> files = RoomBackgroundTilemapExtractor.Extract(bus);
        var entries = new Dictionary<string, RoomBackgroundFileEntry>();
        foreach ((RoomBackgroundTilemap tilemap, int address) in RoomBackgroundSourceDefinitions.Tilemaps.OrderBy(pair => pair.Key))
        {
            string name = FileName(tilemap);
            byte[] json = files[name];
            int byteCount = RomDataReader.Decompress(CartridgeImportSource.Require(bus), address).Length;
            RoomBackgroundTilemapFormat.ValidatePageCount(byteCount);
            using (var output = new FileStream(Path.Combine(importData, name), FileMode.CreateNew, FileAccess.Write))
                output.Write(json);
            entries.Add(name, new RoomBackgroundFileEntry(tilemap.ToString(), byteCount,
                Convert.ToHexString(SHA256.HashData(json))));
        }
        using var manifest = new FileStream(Path.Combine(importData, ManifestFileName),
            FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(manifest,
            new RoomBackgroundFileManifest(FormatVersion, sourceCartridgeSha256, entries), JsonOptions);
    }

    /// <summary>Checks the backgrounds' provenance and compiles each tilemap document of the component's import data, without cartridge access.</summary>
    /// <param name="importData">The component's import-data directory, holding the supported-cartridge manifest and every tilemap JSON file.</param>
    /// <returns>A catalog owning the ordered BG tilemap bytes, keyed by the background each pictures.</returns>
    /// <remarks>Validates each document against its manifest length, which or how many pages a file holds being fixed.</remarks>
    /// <exception cref="InvalidDataException">The manifest, tilemap coverage, length, or a tilemap document is invalid.</exception>
    public static RoomBackgroundTilemaps LoadForImport(string importData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(importData);
        string manifestPath = Path.Combine(importData, ManifestFileName);
        using var manifestStream = File.OpenRead(manifestPath);
        RoomBackgroundFileManifest manifest = JsonAssetDocument.Read<RoomBackgroundFileManifest>(
            manifestStream, JsonOptions, $"room background manifest {manifestPath}");
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase) ||
            manifest.Entries is null ||
            manifest.Entries.Count != RoomBackgroundSourceDefinitions.Tilemaps.Count)
            throw new InvalidDataException(
                $"Room background manifest {manifestPath} does not describe this import.");

        var tilemaps = new Dictionary<RoomBackgroundTilemap, RoomBackgroundTilemapAtlas>();
        foreach ((string name, RoomBackgroundFileEntry entry) in manifest.Entries)
        {
            if (entry is null || !Enum.TryParse(entry.Tilemap, ignoreCase: false, out RoomBackgroundTilemap tilemap) ||
                !Enum.IsDefined(tilemap) || name != FileName(tilemap))
                throw new InvalidDataException($"Room background manifest {manifestPath} has an invalid tilemap entry.");
            RoomBackgroundTilemapFormat.ValidatePageCount(entry.ByteCount);
            string path = Path.Combine(importData, name);
            using PooledFileBytes json = PooledFileBytes.Read(path);
            RoomBackgroundTilemapAtlas atlas;
            try
            {
                using MemoryStream jsonStream = json.OpenRead();
                atlas = RoomBackgroundTilemapAtlas.Load(jsonStream, entry.ByteCount);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid room background {path}: {error.Message}", error);
            }
            if (!tilemaps.TryAdd(tilemap, atlas))
                throw new InvalidDataException($"Room background manifest {manifestPath} repeats a tilemap.");
        }
        return new RoomBackgroundTilemaps(tilemaps);
    }

    /// <summary>Strict, required-property JSON settings shared by background manifest load and extraction.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectRequiredConstructorParameters = true,
        WriteIndented = true,
    };

    /// <summary>One stock tilemap's name, decompressed length in bytes, and JSON digest.</summary>
    private sealed record RoomBackgroundFileEntry(string Tilemap, int ByteCount, string Sha256);
    /// <summary>Records cartridge provenance and complete stock background tilemap coverage.</summary>
    private sealed record RoomBackgroundFileManifest(int Version, string SourceCartridgeSha256,
        Dictionary<string, RoomBackgroundFileEntry> Entries);
}

using SuperMetroid.AssetExtraction.Assets;
using SuperMetroid.Core;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installed BG tilemaps, whose tile references a player edits in place.</summary>
public static class RoomBackgroundTilemapArtworkFiles
{
    /// <summary>Stock manifest filename recording every background tilemap, its length, and its JSON hash.</summary>
    public const string ManifestFileName = "room-backgrounds.json";
    /// <summary>Manifest revision; 2 names each tilemap for its background rather than its cartridge address (#1414).</summary>
    private const int FormatVersion = 2;

    /// <summary>The editable JSON file name of <paramref name="tilemap"/>, named for the background it pictures (#1414).</summary>
    public static string FileName(RoomBackgroundTilemap tilemap) =>
        $"room-background-{InstalledArtworkNames.Hyphenated(tilemap.ToString())}.json";

    /// <summary>Creates one editable tilemap JSON file per background tilemap and records its provenance.</summary>
    /// <param name="bus">Non-null cartridge import address space supplying the compressed background tilemaps.</param>
    /// <param name="directory">Output directory, created if needed; resource and manifest filenames must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Nonempty cartridge SHA-256 stored with the tilemaps, their one- or two-page lengths, and file hashes.</param>
    /// <remarks>Preserves ordered 32x32 BG pages; reads no installed tilemap.</remarks>
    /// <exception cref="IOException">An output file already exists or filesystem output fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        IReadOnlyDictionary<string, byte[]> files = RoomBackgroundTilemapExtractor.Extract(bus);
        var entries = new Dictionary<string, RoomBackgroundFileEntry>();
        foreach ((RoomBackgroundTilemap tilemap, int address) in RoomBackgroundSourceDefinitions.Tilemaps.OrderBy(pair => pair.Key))
        {
            string name = FileName(tilemap);
            byte[] json = files[name];
            int byteCount = RomDataReader.Decompress(CartridgeImportSource.Require(bus), address).Length;
            RoomBackgroundTilemapFormat.ValidatePageCount(byteCount);
            using (var output = new FileStream(Path.Combine(directory, name), FileMode.CreateNew, FileAccess.Write))
                output.Write(json);
            entries.Add(name, new RoomBackgroundFileEntry(tilemap.ToString(), byteCount,
                Convert.ToHexString(SHA256.HashData(json))));
        }
        using var manifest = new FileStream(Path.Combine(directory, ManifestFileName),
            FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(manifest,
            new RoomBackgroundFileManifest(FormatVersion, sourceCartridgeSha256, entries), JsonOptions);
    }

    /// <summary>Checks installed background provenance and compiles each installed tilemap resource as the player left it, without cartridge access.</summary>
    /// <param name="directory">Directory containing the supported-cartridge manifest and every installed tilemap JSON file.</param>
    /// <returns>A catalog owning the installed ordered BG tilemap bytes, keyed by the background each pictures.</returns>
    /// <remarks>Validates each installed document against its manifest length. An edit cannot alter which tilemap a file is or its page count.</remarks>
    /// <exception cref="InvalidDataException">The manifest, tilemap coverage, length, or an installed tilemap document is invalid.</exception>
    public static RoomBackgroundTilemaps Load(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string manifestPath = Path.Combine(directory, ManifestFileName);
        using var manifestStream = File.OpenRead(manifestPath);
        RoomBackgroundFileManifest manifest = JsonAssetDocument.Read<RoomBackgroundFileManifest>(
            manifestStream, JsonOptions, $"room background manifest {manifestPath}");
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase) ||
            manifest.Entries is null ||
            manifest.Entries.Count != RoomBackgroundSourceDefinitions.Tilemaps.Count)
            throw new InvalidDataException(
                $"Room background manifest {manifestPath} does not describe this installation.");

        var tilemaps = new Dictionary<RoomBackgroundTilemap, RoomBackgroundTilemapAtlas>();
        foreach ((string name, RoomBackgroundFileEntry entry) in manifest.Entries)
        {
            if (entry is null || !Enum.TryParse(entry.Tilemap, ignoreCase: false, out RoomBackgroundTilemap tilemap) ||
                !Enum.IsDefined(tilemap) || name != FileName(tilemap))
                throw new InvalidDataException($"Room background manifest {manifestPath} has an invalid tilemap entry.");
            RoomBackgroundTilemapFormat.ValidatePageCount(entry.ByteCount);
            string path = Path.Combine(directory, name);
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

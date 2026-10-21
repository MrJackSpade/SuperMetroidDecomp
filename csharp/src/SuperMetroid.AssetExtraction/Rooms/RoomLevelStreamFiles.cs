using SuperMetroid.Core;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Extracts the complete native room-level allocation for every retail level source: the
/// collision/BTS and streaming baseline that room loads use without decompressing the ROM.
/// The family is stock-only. It has no override directory, so artwork edits cannot alter
/// collision, BTS, slopes, hazards, or the native overread tail.
/// </summary>
[CpuBusBoundary("codec: ROM compressed room level data → native level-stream files")]
public static class RoomLevelStreamFiles
{
    /// <summary>Stock manifest filename recording version-one provenance and each retail source's address, byte count, and SHA-256.</summary>
    public const string ManifestFileName = "room-level-streams.json";
    /// <summary>Manifest schema version this loader accepts.</summary>
    private const int FormatVersion = 1;

    /// <summary>Serializer settings shared by manifest writes and reads.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectRequiredConstructorParameters = true,
        WriteIndented = true,
    };

    /// <summary>Formats the stable per-source stream filename without reading or validating the source.</summary>
    /// <param name="sourceAddress">Native 24-bit compressed level-data identity, shared by all room states using that source.</param>
    /// <returns><c>level-</c> followed by the uppercase hexadecimal address padded to at least six digits and <c>.bin</c>.</returns>
    public static string SourceFileName(int sourceAddress) => $"level-{sourceAddress:X6}.bin";

    /// <summary>Decompresses every distinct retail level source and writes each complete decompressor output unchanged.</summary>
    /// <param name="bus">Non-null import-capable cartridge source for the compressed level streams identified by <see cref="RoomVisualLayoutFiles.RetailSources"/>.</param>
    /// <param name="directory">Family directory, created if absent; receives new per-source files and the required manifest.</param>
    /// <param name="sourceCartridgeSha256">Nonblank caller-supplied provenance hash, recorded without recomputation and checked against the supported identity on load.</param>
    /// <remarks>Streams are neither trimmed, padded nor re-encoded. Create-new writes refuse existing files and do not roll back earlier outputs.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory/source hash is blank or the address space lacks cartridge import access.</exception>
    /// <exception cref="InvalidDataException">A compressed stream is invalid or lacks its layer-size word.</exception>
    /// <exception cref="IOException">An output exists or filesystem access fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        Directory.CreateDirectory(directory);
        var entries = new Dictionary<string, StreamFileEntry>();
        foreach (int sourceAddress in RoomVisualLayoutFiles.RetailSources.Keys.Order())
        {
            byte[] stream = RomDataReader.Decompress(cartridge, sourceAddress);
            if (stream.Length < RoomLevelStreamCatalog.MinimumStreamByteCount)
                throw new InvalidDataException($"Room level ${sourceAddress:X6} has no size word.");
            string name = SourceFileName(sourceAddress);
            using (var file = new FileStream(Path.Combine(directory, name), FileMode.CreateNew,
                       FileAccess.Write))
                file.Write(stream);
            entries.Add(name, new StreamFileEntry(sourceAddress, stream.Length,
                Convert.ToHexString(SHA256.HashData(stream))));
        }
        using var manifest = new FileStream(Path.Combine(directory, ManifestFileName),
            FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(manifest,
            new StreamFileManifest(FormatVersion, sourceCartridgeSha256, entries), JsonOptions);
    }

    /// <summary>Checks the complete retail-source manifest and every stream's length and hash, then builds the stock catalog.</summary>
    /// <param name="directory">Family directory containing all per-source stream files and the required provenance manifest.</param>
    /// <returns>A ROM-independent catalog of complete, unmodified native level allocations.</returns>
    /// <exception cref="ArgumentException"><paramref name="directory"/> is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">The version, provenance, coverage, a file's identity, length, or hash is invalid; the message names the file.</exception>
    /// <exception cref="IOException">The manifest or a stream file cannot be read.</exception>
    public static RoomLevelStreamCatalog Load(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string manifestPath = Path.Combine(directory, ManifestFileName);
        if (!File.Exists(manifestPath))
            throw new InvalidDataException($"Room-level stream manifest {manifestPath} is missing.");
        StreamFileManifest manifest;
        using (FileStream manifestStream = File.OpenRead(manifestPath))
            manifest = JsonAssetDocument.Read<StreamFileManifest>(
                manifestStream, JsonOptions, $"room-level stream manifest {manifestPath}");
        if (manifest.Version != FormatVersion)
            throw new InvalidDataException(
                $"Room-level stream manifest {manifestPath} has unsupported version {manifest.Version}.");
        if (!string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(
                $"Room-level stream manifest {manifestPath} was not extracted from the supported cartridge.");
        if (manifest.Entries is null)
            throw new InvalidDataException($"Room-level stream manifest {manifestPath} has no entries.");

        var streams = new Dictionary<int, byte[]>(manifest.Entries.Count);
        foreach ((string name, StreamFileEntry entry) in manifest.Entries)
        {
            string path = Path.Combine(directory, name);
            if (entry is null || name != SourceFileName(entry.SourceAddress) ||
                !RoomVisualLayoutFiles.RetailSources.ContainsKey(entry.SourceAddress))
                throw new InvalidDataException(
                    $"Room-level stream manifest {manifestPath} names unknown source file {path}.");
            if (!File.Exists(path))
                throw new InvalidDataException($"Room-level stream {path} is missing.");
            byte[] stream = File.ReadAllBytes(path);
            if (stream.Length != entry.ByteCount || stream.Length < RoomLevelStreamCatalog.MinimumStreamByteCount)
                throw new InvalidDataException(
                    $"Room-level stream {path} has {stream.Length} bytes; the manifest records {entry.ByteCount}.");
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(stream)), entry.Sha256,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Room-level stream {path} failed its manifest hash.");
            if (!streams.TryAdd(entry.SourceAddress, stream))
                throw new InvalidDataException($"Room-level stream manifest {manifestPath} repeats {path}.");
        }
        foreach (int source in RoomVisualLayoutFiles.RetailSources.Keys.Order())
            if (!streams.ContainsKey(source))
                throw new InvalidDataException(
                    $"Room-level stream manifest {manifestPath} lacks {Path.Combine(directory, SourceFileName(source))}.");
        return new RoomLevelStreamCatalog(streams, Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(manifestPath))),
            RoomVisualLayoutSourceDefinitions.All);
    }

    /// <summary>Loads and validates every stock stream through <see cref="Load"/> with no cartridge reads or file writes.</summary>
    /// <param name="directory">Room-level stream family directory containing the manifest and all referenced files.</param>
    /// <exception cref="InvalidDataException">Provenance, hashes, lengths, schema, or coverage are invalid.</exception>
    public static void ValidateStock(string directory) => _ = Load(directory);

    /// <summary>One retail level source: its 24-bit address, decompressed byte count and SHA-256 digest.</summary>
    private sealed record StreamFileEntry(int SourceAddress, int ByteCount, string Sha256);
    /// <summary>Records schema and source-ROM identity plus every installed level-stream file entry.</summary>
    private sealed record StreamFileManifest(int Version, string SourceCartridgeSha256,
        Dictionary<string, StreamFileEntry> Entries);
}

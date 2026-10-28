using SuperMetroid.Core;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Hash-checked stock block compositions, which extraction composes the block sheets from (#1431). They are not a player
/// override surface: a room's blocks change only through their pictures in the block sheet.
/// </summary>
public static class RoomMetatileArtworkFiles
{
    /// <summary>Stock visual-block manifest filename recording cartridge provenance, metatile JSON hashes, and native transfer lengths.</summary>
    public const string ManifestFileName = "room-blocks.json";
    /// <summary>Schema revision required by the stock visual-block provenance manifest.</summary>
    private const int FormatVersion = 2;

    /// <summary>Creates stock visual metatile JSON for CRE and each distinct room block-table source, with a provenance manifest.</summary>
    /// <param name="bus">Non-null cartridge import address space containing the compressed visual block-definition tables.</param>
    /// <param name="directory">Output directory, created if needed; resource and manifest filenames must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Nonempty cartridge SHA-256 stored with each file's hash and native block-table byte count.</param>
    /// <remarks>Checks exact native-byte roundtrips for four-quadrant visual compositions. Collision and block behavior remain outside these resources.</remarks>
    /// <exception cref="IOException">An output file already exists or filesystem output fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        IReadOnlyDictionary<string, byte[]> files = RoomMetatileExtractor.Extract(bus);
        RoomTilesetSourceMap tilesets = RoomTilesetSources.Read(CartridgeImportSource.Require(bus));
        var entries = new Dictionary<string, RoomMetatileFileEntry>();
        foreach ((string name, RoomBlockSource source) in ExpectedSources())
        {
            SnesAddress address = source.Set is { } set ? tilesets.Blocks[set]
                : RoomTilesetSourceDefinitions.CreBlocks;
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

    /// <summary>Validates stock provenance and the complete stock block compositions without a ROM.</summary>
    public static RoomMetatileCatalog Load(string stockDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, ManifestFileName);
        using var manifestStream = File.OpenRead(manifestPath);
        RoomMetatileFileManifest manifest = JsonAssetDocument.Read<RoomMetatileFileManifest>(
            manifestStream, JsonOptions, $"room metatile manifest {manifestPath}");
        Dictionary<string, RoomBlockSource> sources = ExpectedSources();
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase) ||
            manifest.Entries is null || manifest.Entries.Count != sources.Count ||
            sources.Keys.Any(name => !manifest.Entries.ContainsKey(name)))
            throw new InvalidDataException($"Room metatile manifest {manifestPath} does not describe this installation.");

        RoomMetatileAtlas? cre = null;
        var selected = new Dictionary<RoomBlockSet, RoomMetatileAtlas>();
        foreach ((string name, RoomBlockSource source) in sources)
        {
            RoomMetatileFileEntry entry = manifest.Entries[name];
            RoomMetatileFormat.ValidateBlockCount(entry.NativeByteCount);
            string stockPath = Path.Combine(stockDirectory, name);
            using PooledFileBytes stock = PooledFileBytes.Read(stockPath);
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(stock.Span)), entry.Sha256,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Stock room metatile JSON {stockPath} failed its manifest hash.");
            RoomMetatileAtlas atlas;
            try
            {
                using MemoryStream jsonStream = stock.OpenRead();
                atlas = RoomMetatileAtlas.Load(jsonStream, entry.NativeByteCount);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid room metatile JSON {stockPath}: {error.Message}", error);
            }
            if (source.Set is { } set) selected.Add(set, atlas);
            else cre = atlas;
        }
        return new RoomMetatileCatalog(
            cre ?? throw new InvalidDataException("Room metatile manifest has no CRE resource."), selected);
    }

    /// <summary>Validates the complete stock CRE and room metatile resources.</summary>
    /// <param name="stockDirectory">Directory containing the visual-block manifest and stock JSON resources.</param>
    /// <remarks>Checks supported cartridge provenance, expected source coverage, file hashes, block counts, and BG-cell fields without cartridge access or file writes.</remarks>
    /// <exception cref="InvalidDataException">Stock provenance, coverage, dimensions, hashes, or visual composition data are invalid.</exception>
    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory);

    /// <summary>Collects the CRE source and each distinct room tileset block-definition source by asset filename.</summary>
    private static Dictionary<string, RoomBlockSource> ExpectedSources()
    {
        var sources = new Dictionary<string, RoomBlockSource>
        {
            [RoomMetatileFormat.CreFileName] = new(null),
        };
        for (int graphicsSet = 0; graphicsSet < RoomTilesetDefinitions.Count; graphicsSet++)
        {
            RoomBlockSet set = RoomTilesetDefinitions.Get(graphicsSet).Blocks;
            sources[RoomMetatileFormat.FileName(set)] = new(set);
        }
        return sources;
    }

    /// <summary>A block file's table: none for the common room elements.</summary>
    /// <param name="Set">The table, or null for the common room elements.</param>
    private readonly record struct RoomBlockSource(RoomBlockSet? Set);

    /// <summary>Strict camel-case serialization settings for the metatile provenance manifest.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectRequiredConstructorParameters = true,
        WriteIndented = true,
    };

    /// <summary>Native decompressed length and SHA-256 for one stock visual-block JSON file.</summary>
    private sealed record RoomMetatileFileEntry(int NativeByteCount, string Sha256);
    /// <summary>Versioned cartridge provenance and all expected visual-block file entries.</summary>
    private sealed record RoomMetatileFileManifest(int Version, string SourceCartridgeSha256,
        Dictionary<string, RoomMetatileFileEntry> Entries);
}

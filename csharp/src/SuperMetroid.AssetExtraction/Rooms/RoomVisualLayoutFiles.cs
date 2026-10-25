using SuperMetroid.Core;
using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Extracts initial room visual-block references without exporting collision types or BTS.
/// A user's JSON override may alter only bank-$80's initial tilemap source; gameplay keeps
/// the original decompressed room allocation and all later PLM writes.
/// </summary>
[CpuBusBoundary("codec: ROM compressed room level data → room visual-block layouts")]
public static class RoomVisualLayoutFiles
{
    /// <summary>Stock manifest filename recording provenance and each retail level's name, block dimensions, filename, and JSON byte hash.</summary>
    public const string ManifestFileName = "room-layouts.json";
    /// <summary>Schema revision shared by the manifest and per-level layout documents; version 2 names each level instead of its cartridge source.</summary>
    private const int FormatVersion = 2;
    /// <summary>The schema revision before #1414, whose documents carried the level's cartridge source address.</summary>
    private const int LegacyFormatVersion = 1;

    /// <summary>Strict camel-case JSON encoding for provenance and visual-word documents.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectRequiredConstructorParameters = true,
        WriteIndented = true,
    };

    /// <summary>Distinct levels selected by retail room states and their row strides.</summary>
    public static IReadOnlyDictionary<RoomLevelId, int> RetailLevels { get; } = BuildRetailLevels();

    /// <summary>Formats the stable per-level stock/override JSON filename without validating the level.</summary>
    /// <param name="level">The level, shared by all room states using it; membership validation occurs during loading.</param>
    /// <returns><c>level-</c> followed by the level's name and <c>.json</c>.</returns>
    public static string LevelFileName(RoomLevelId level) => $"level-{level.Name}.json";

    /// <summary>Decompresses every distinct retail room level and exports row-major foreground/background metatile-and-flip words without collision types or BTS.</summary>
    /// <param name="bus">Non-null import-capable cartridge source for the compressed level streams identified by <see cref="RetailLevels"/>.</param>
    /// <param name="directory">Family directory, created if absent; receives new per-level JSON files and the required manifest.</param>
    /// <param name="sourceCartridgeSha256">Nonblank caller-supplied provenance hash, recorded without recomputation and checked against the supported identity on stock load.</param>
    /// <remarks>Widths and heights use 16-pixel blocks. Foreground allocation determines height; absent trailing background words become zero, and every exported word keeps only bits 0..11. Create-new writes refuse existing files and do not roll back earlier outputs.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory/source hash is blank or the address space lacks cartridge import access.</exception>
    /// <exception cref="InvalidDataException">A compressed stream, foreground/BTS allocation, row stride, or trailing background-word boundary is invalid.</exception>
    /// <exception cref="IOException">An output exists or filesystem access fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);
        var entries = new Dictionary<string, LayoutFileEntry>();
        foreach ((RoomLevelId level, int widthInBlocks) in RetailLevels.OrderBy(item => item.Key.Name, StringComparer.Ordinal))
        {
            RoomVisualLayoutDocument document = Decode(bus, level, widthInBlocks);
            byte[] json = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
            string name = LevelFileName(level);
            using (var file = new FileStream(Path.Combine(directory, name), FileMode.CreateNew,
                       FileAccess.Write))
                file.Write(json);
            entries.Add(name, new LayoutFileEntry(level.Name, widthInBlocks,
                document.HeightInBlocks, Convert.ToHexString(SHA256.HashData(json))));
        }
        using var manifest = new FileStream(Path.Combine(directory, ManifestFileName),
            FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(manifest,
            new LayoutFileManifest(FormatVersion, sourceCartridgeSha256, entries), JsonOptions);
    }

    /// <summary>Checks the complete retail-level manifest and stock hashes, then selects independently replaceable initial room visual layouts.</summary>
    /// <param name="stockDirectory">Family directory containing all per-level stock JSON files and the required provenance/dimension manifest.</param>
    /// <param name="overrideDirectory">Optional family directory; each absent level JSON independently falls back to stock.</param>
    /// <returns>A ROM-independent catalog of selected initial foreground/background visual words, preserving gameplay allocation, collision/BTS data, and later PLM writes.</returns>
    /// <remarks>Stock provenance and every byte hash remain mandatory with overrides. Strict selected JSON must retain the level, original block dimensions, array extents, and twelve-bit words; no override manifest is needed. An overridden stock document is hash-checked but not separately deserialized or compared with cartridge bytes.</remarks>
    /// <exception cref="ArgumentException"><paramref name="stockDirectory"/> is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Provenance, coverage, integrity, strict JSON, level identity, dimensions, or visual word arrays are invalid.</exception>
    /// <exception cref="IOException">A required stock or selected override file cannot be read.</exception>
    public static RoomVisualLayoutCatalog Load(string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, ManifestFileName);
        using var manifestStream = File.OpenRead(manifestPath);
        LayoutFileManifest manifest = JsonAssetDocument.Read<LayoutFileManifest>(
            manifestStream, JsonOptions, $"room-layout manifest {manifestPath}");
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase) ||
            manifest.Entries is null || manifest.Entries.Count != RetailLevels.Count)
            throw new InvalidDataException(
                $"Room-layout manifest {manifestPath} does not cover the pinned retail levels.");

        var selected = new Dictionary<RoomLevelId, RoomVisualLayout>();
        foreach ((string name, LayoutFileEntry entry) in manifest.Entries)
        {
            if (entry?.Level is null || name != LevelFileName(entry.Id) ||
                !RetailLevels.TryGetValue(entry.Id, out int width) ||
                entry.Size.X != width || entry.Size.Y <= 0)
                throw new InvalidDataException(
                    $"Room-layout manifest {manifestPath} has an invalid level entry.");
            string stockPath = Path.Combine(stockDirectory, name);
            using PooledFileBytes stock = PooledFileBytes.Read(stockPath);
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(stock.Span)), entry.Sha256,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Stock room layout {stockPath} failed its manifest hash.");
            string? overridePath = overrideDirectory is null ? null : Path.Combine(overrideDirectory, name);
            string selectedPath = overridePath is not null && File.Exists(overridePath)
                ? overridePath : stockPath;
            using PooledFileBytes? edited = selectedPath == stockPath ? null : PooledFileBytes.Read(selectedPath);
            PooledFileBytes json = edited ?? stock;
            using MemoryStream jsonStream = json.OpenRead();
            RoomVisualLayoutDocument document = JsonAssetDocument.Read<RoomVisualLayoutDocument>(
                jsonStream, JsonOptions, $"room layout {selectedPath}");
            if (document.FormatVersion != FormatVersion ||
                document.Level != entry.Level ||
                document.Size != entry.Size ||
                document.ForegroundVisualWords is null ||
                document.BackgroundVisualWords is null)
                throw new InvalidDataException($"Room layout {selectedPath} has incompatible dimensions or identity.");
            RoomVisualLayout layout;
            try
            {
                layout = new RoomVisualLayout(entry.Id, document.Size,
                    document.ForegroundVisualWords, document.BackgroundVisualWords);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid room layout {selectedPath}: {error.Message}", error);
            }
            if (!selected.TryAdd(entry.Id, layout))
                throw new InvalidDataException($"Room-layout manifest {manifestPath} repeats a level.");
        }
        return new RoomVisualLayoutCatalog(selected, RoomVisualLayoutSourceDefinitions.All);
    }

    /// <summary>Loads and validates every stock level through <see cref="Load"/> with no overrides, cartridge reads, or file writes.</summary>
    /// <param name="directory">Room-layout family directory containing the manifest and all referenced stock JSON files.</param>
    /// <exception cref="InvalidDataException">Stock provenance, hashes, schema, coverage, or layouts are invalid.</exception>
    public static void ValidateStock(string directory) => _ = Load(directory, null);

    /// <summary>
    /// Moves each layout override still under its name from before #1414, which carried its level's cartridge source, to
    /// its level's name, and rewrites the document to name its level; a player who holds both must choose.
    /// </summary>
    /// <param name="overrideDirectory">The room-layout override directory, which need not exist.</param>
    /// <exception cref="IOException">An override exists under both its old and its new name.</exception>
    /// <exception cref="InvalidDataException">An old override is not a version-one document of the source its name gives.</exception>
    internal static void MigrateLegacyOverrides(string overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(overrideDirectory);
        foreach ((RoomLevelId level, int source) in RoomLevelSourceDefinitions.Addresses)
        {
            string legacy = Path.Combine(overrideDirectory, $"level-{source:X6}.json");
            if (!File.Exists(legacy))
                continue;
            string current = Path.Combine(overrideDirectory, LevelFileName(level));
            if (File.Exists(current))
                throw new IOException($"Room layout override {LevelFileName(level)} also exists under its old name {Path.GetFileName(legacy)}; keep one of them.");
            LegacyRoomVisualLayoutDocument old;
            using (FileStream stream = File.OpenRead(legacy))
                old = JsonAssetDocument.Read<LegacyRoomVisualLayoutDocument>(stream, JsonOptions, $"room layout {legacy}");
            if (old.FormatVersion != LegacyFormatVersion || old.SourceAddress != source)
                throw new InvalidDataException($"Room layout override {legacy} is not the version-{LegacyFormatVersion} layout of the level its name gives.");
            byte[] json = JsonSerializer.SerializeToUtf8Bytes(new RoomVisualLayoutDocument(FormatVersion, level.Name,
                old.WidthInBlocks, old.HeightInBlocks, old.ForegroundVisualWords, old.BackgroundVisualWords), JsonOptions);
            using (var file = new FileStream(current, FileMode.CreateNew, FileAccess.Write))
                file.Write(json);
            File.Delete(legacy);
        }
    }

    /// <summary>Finds each distinct room level and confirms the states sharing it agree on its block row stride.</summary>
    private static ReadOnlyDictionary<RoomLevelId, int> BuildRetailLevels()
    {
        var levels = new Dictionary<RoomLevelId, int>();
        foreach (RoomHeaderDefinition header in RoomHeaderDefinitions.All)
        {
            int width = header.Size.ToRoomBlocks().X;
            foreach (RoomStateSettings state in RoomStateSelectionDefinitions.GetStates(header))
            {
                RoomLevelId level = state.Level;
                if (levels.TryGetValue(level, out int existing) && existing != width)
                    throw new InvalidDataException(
                        $"Room level {level} is shared by {existing}- and {width}-block row strides.");
                levels[level] = width;
            }
        }
        return new ReadOnlyDictionary<RoomLevelId, int>(levels);
    }

    /// <summary>Decompresses one room level, keeps twelve-bit BG1/BG2 visual words, and derives height from BG1 allocation.</summary>
    private static RoomVisualLayoutDocument Decode(ISnesAddressSpace bus,
        RoomLevelId level, int widthInBlocks)
    {
        byte[] stream = RomDataReader.Decompress(CartridgeImportSource.Require(bus), RoomLevelSourceDefinitions.Addresses[level]);
        if (stream.Length < 2)
            throw new InvalidDataException($"Room level {level} has no size word.");
        int layerBytes = BinaryPrimitives.ReadUInt16LittleEndian(stream);
        int blockCount = layerBytes / 2;
        if ((layerBytes & 1) != 0 || blockCount == 0 || blockCount % widthInBlocks != 0 ||
            stream.Length < 2 + layerBytes + blockCount)
            throw new InvalidDataException(
                $"Room level {level} has an invalid BG1/BTS allocation.");
        var foreground = new ushort[blockCount];
        var background = new ushort[blockCount];
        int backgroundOffset = 2 + layerBytes + blockCount;
        int availableBackgroundBytes = Math.Min(layerBytes, stream.Length - backgroundOffset);
        if ((availableBackgroundBytes & 1) != 0)
            throw new InvalidDataException($"Room level {level} ends inside a BG2 word.");
        for (int index = 0; index < blockCount; index++)
        {
            foreground[index] = new RoomLevelWord(
                BinaryPrimitives.ReadUInt16LittleEndian(stream.AsSpan(2 + index * 2))).VisualWord;
            int offset = backgroundOffset + index * 2;
            if (offset + 2 <= backgroundOffset + availableBackgroundBytes)
                background[index] = new RoomLevelWord(
                    BinaryPrimitives.ReadUInt16LittleEndian(stream.AsSpan(offset))).VisualWord;
        }
        return new RoomVisualLayoutDocument(FormatVersion, level.Name,
            widthInBlocks, blockCount / widthInBlocks, foreground, background);
    }

    /// <summary>One retail level's name, block dimensions, and expected stock JSON hash.</summary>
    [SpatialComponents("External format: room-layouts.json manifest stores widthInBlocks/heightInBlocks.")]
    private sealed record LayoutFileEntry(string Level, int WidthInBlocks,
        int HeightInBlocks, string Sha256)
    {
        /// <summary>The level the entry names.</summary>
        [JsonIgnore]
        public RoomLevelId Id => new(Level);

        /// <summary>Converts the manifest's block width and height into a checked spatial extent.</summary>
        [JsonIgnore]
        public Extent16<RoomBlocks> Size => new(checked((ushort)WidthInBlocks), checked((ushort)HeightInBlocks));
    }
    /// <summary>Provenance and complete per-level stock file inventory for extracted room layouts.</summary>
    private sealed record LayoutFileManifest(int Version, string SourceCartridgeSha256,
        Dictionary<string, LayoutFileEntry> Entries);
    /// <summary>Editable initial BG1/BG2 visual words with fixed level identity and block dimensions.</summary>
    [SpatialComponents("External format: per-level room-layout JSON stores widthInBlocks/heightInBlocks.")]
    private sealed record RoomVisualLayoutDocument(int FormatVersion, string Level,
        int WidthInBlocks, int HeightInBlocks, ushort[] ForegroundVisualWords,
        ushort[] BackgroundVisualWords)
    {
        /// <summary>Converts document dimensions into the room-block extent used for validation.</summary>
        [JsonIgnore]
        public Extent16<RoomBlocks> Size => new(checked((ushort)WidthInBlocks), checked((ushort)HeightInBlocks));
    }
    /// <summary>A version-one layout override, which named its level by the level's cartridge source.</summary>
    [SpatialComponents("External format: version-one room-layout JSON stores widthInBlocks/heightInBlocks.")]
    private sealed record LegacyRoomVisualLayoutDocument(int FormatVersion, int SourceAddress,
        int WidthInBlocks, int HeightInBlocks, ushort[] ForegroundVisualWords,
        ushort[] BackgroundVisualWords);
}

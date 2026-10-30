using SuperMetroid.Core;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Installs what each room level's blocks initially show (#1469), without terrain, collision or behaviours. A user's JSON
/// override may alter only those appearances; gameplay keeps the installed terrain and all later room-object writes.
/// </summary>
/// <remarks>
/// A level document holds each plane as its blocks' numbers, row by row, and its blocks' mirroring as one character per
/// block (<see cref="MirroringCodes"/>: unmirrored, horizontal, vertical, both).
/// </remarks>
public static class RoomVisualLayoutFiles
{
    /// <summary>Stock manifest filename recording provenance and each retail level's name, block dimensions, filename, and JSON byte hash.</summary>
    public const string ManifestFileName = "room-layouts.json";
    /// <summary>Schema revision shared by the manifest and per-level layout documents; version 3 holds block numbers and mirroring instead of level words.</summary>
    private const int FormatVersion = 3;
    /// <summary>The schema revision from #1414 to #1469, whose documents carried twelve-bit level words.</summary>
    private const int WordFormatVersion = 2;
    /// <summary>The schema revision before #1414, whose documents carried the level's cartridge source address.</summary>
    private const int LegacyFormatVersion = 1;

    /// <summary>The character a document writes for each mirroring.</summary>
    private static readonly IReadOnlyDictionary<BlockMirroring, char> MirroringCodes = new Dictionary<BlockMirroring, char>
    {
        [BlockMirroring.None] = '-',
        [BlockMirroring.Horizontal] = 'H',
        [BlockMirroring.Vertical] = 'V',
        [BlockMirroring.Horizontal | BlockMirroring.Vertical] = 'B',
    };

    /// <summary>The mirroring each document character stands for.</summary>
    private static readonly IReadOnlyDictionary<char, BlockMirroring> MirroringByCode =
        MirroringCodes.ToDictionary(code => code.Value, code => code.Key);

    /// <summary>Strict camel-case JSON encoding for provenance and layout documents.</summary>
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

    /// <summary>Decompresses every distinct retail room level and exports what its foreground and background blocks show, without terrain or behaviours.</summary>
    /// <param name="bus">Non-null import-capable cartridge source for the compressed level streams identified by <see cref="RetailLevels"/>.</param>
    /// <param name="directory">Family directory, created if absent; receives new per-level JSON files and the required manifest.</param>
    /// <param name="sourceCartridgeSha256">Nonblank caller-supplied provenance hash, recorded without recomputation and checked against the supported identity on stock load.</param>
    /// <remarks>Widths and heights use 16-pixel blocks. The foreground grid determines height. Create-new writes refuse existing files and do not roll back earlier outputs.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory/source hash is blank or the address space lacks cartridge import access.</exception>
    /// <exception cref="InvalidDataException">A compressed level, its foreground or behaviour layer, row stride, or background is invalid.</exception>
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
    /// <returns>A ROM-independent catalog of selected initial foreground and background appearances, leaving terrain, behaviours and later room-object writes alone.</returns>
    /// <remarks>Stock provenance and every byte hash remain mandatory with overrides. Strict selected JSON must retain the level, original block dimensions, array extents, valid block numbers and mirroring codes; no override manifest is needed. An overridden stock document is hash-checked but not separately deserialized or compared with cartridge bytes.</remarks>
    /// <exception cref="ArgumentException"><paramref name="stockDirectory"/> is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Provenance, coverage, integrity, strict JSON, level identity, dimensions, block numbers or mirroring codes are invalid.</exception>
    /// <exception cref="IOException">A required stock or selected override file cannot be read.</exception>
    public static RoomLevelArtworks Load(string stockDirectory, string? overrideDirectory)
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

        var selected = new Dictionary<RoomLevelId, RoomLevelArtwork>();
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
                document.Foreground is null ||
                document.Background is null)
                throw new InvalidDataException($"Room layout {selectedPath} has incompatible dimensions or identity.");
            RoomLevelArtwork layout;
            try
            {
                layout = new RoomLevelArtwork(entry.Id, document.Size,
                    ReadPlane(document.Foreground), ReadPlane(document.Background));
            }
            catch (Exception error) when (error is InvalidDataException or ArgumentException)
            {
                throw new InvalidDataException($"Invalid room layout {selectedPath}: {error.Message}", error);
            }
            if (!selected.TryAdd(entry.Id, layout))
                throw new InvalidDataException($"Room-layout manifest {manifestPath} repeats a level.");
        }
        return new RoomLevelArtworks(selected, RoomVisualLayoutSourceDefinitions.All);
    }

    /// <summary>Loads and validates every stock level through <see cref="Load"/> with no overrides, cartridge reads, or file writes.</summary>
    /// <param name="directory">Room-layout family directory containing the manifest and all referenced stock JSON files.</param>
    /// <exception cref="InvalidDataException">Stock provenance, hashes, schema, coverage, or layouts are invalid.</exception>
    public static void ValidateStock(string directory) => _ = Load(directory, null);

    /// <summary>
    /// Moves each layout override still under its name from before #1414, which carried its level's cartridge source, to
    /// its level's name, and rewrites every override that still holds level words (versions one and two, before #1469)
    /// as block numbers and mirroring; a player who holds an override under both names must choose.
    /// </summary>
    /// <param name="overrideDirectory">The room-layout override directory, which need not exist.</param>
    /// <exception cref="IOException">An override exists under both its old and its new name.</exception>
    /// <exception cref="InvalidDataException">An old override is not a document of the level its name gives, or holds a word with collision bits.</exception>
    internal static void MigrateLegacyOverrides(string overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(overrideDirectory);
        foreach ((RoomLevelId level, int source) in RoomLevelSourceDefinitions.Addresses)
        {
            string current = Path.Combine(overrideDirectory, LevelFileName(level));
            string legacy = Path.Combine(overrideDirectory, $"level-{source:X6}.json");
            if (File.Exists(legacy))
            {
                if (File.Exists(current))
                    throw new IOException($"Room layout override {LevelFileName(level)} also exists under its old name {Path.GetFileName(legacy)}; keep one of them.");
                LegacyRoomVisualLayoutDocument old;
                using (FileStream stream = File.OpenRead(legacy))
                    old = JsonAssetDocument.Read<LegacyRoomVisualLayoutDocument>(stream, JsonOptions, $"room layout {legacy}");
                if (old.FormatVersion != LegacyFormatVersion || old.SourceAddress != source)
                    throw new InvalidDataException($"Room layout override {legacy} is not the version-{LegacyFormatVersion} layout of the level its name gives.");
                WriteFromWords(current, level, old.WidthInBlocks, old.HeightInBlocks, old.ForegroundVisualWords, old.BackgroundVisualWords, legacy);
                File.Delete(legacy);
                continue;
            }
            if (!File.Exists(current) || ReadVersion(current) != WordFormatVersion)
                continue;
            WordRoomVisualLayoutDocument words;
            using (FileStream stream = File.OpenRead(current))
                words = JsonAssetDocument.Read<WordRoomVisualLayoutDocument>(stream, JsonOptions, $"room layout {current}");
            if (words.Level != level.Name)
                throw new InvalidDataException($"Room layout override {current} is not the layout of the level its name gives.");
            byte[] json = Serialize(level, words.WidthInBlocks, words.HeightInBlocks, words.ForegroundVisualWords,
                words.BackgroundVisualWords, current);
            File.WriteAllBytes(current, json);
        }
    }

    /// <summary>Writes a version-three override converted from level words to a new file.</summary>
    private static void WriteFromWords(string path, RoomLevelId level, int width, int height, ushort[] foreground,
        ushort[] background, string source)
    {
        byte[] json = Serialize(level, width, height, foreground, background, source);
        using var file = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
        file.Write(json);
    }

    /// <summary>Converts a word layout's planes, which must hold no collision bits, to a version-three document.</summary>
    /// <exception cref="InvalidDataException">A plane is missing or a word holds collision bits.</exception>
    private static byte[] Serialize(RoomLevelId level, int width, int height, ushort[]? foreground, ushort[]? background,
        string source)
    {
        if (foreground is null || background is null ||
            foreground.Any(word => !RoomLevelWordCodec.IsAppearanceWord(word)) ||
            background.Any(word => !RoomLevelWordCodec.IsAppearanceWord(word)))
            throw new InvalidDataException($"Room layout override {source} lacks a plane or holds a collision bit in its artwork.");
        return JsonSerializer.SerializeToUtf8Bytes(new RoomVisualLayoutDocument(FormatVersion, level.Name, width, height,
            WritePlane([.. foreground.Select(RoomLevelWordCodec.DecodeAppearance)]),
            WritePlane([.. background.Select(RoomLevelWordCodec.DecodeAppearance)])), JsonOptions);
    }

    /// <summary>Reads only a layout document's format version.</summary>
    /// <exception cref="InvalidDataException">The document is not JSON or has no numeric format version.</exception>
    private static int ReadVersion(string path)
    {
        try
        {
            using FileStream stream = File.OpenRead(path);
            using JsonDocument document = JsonDocument.Parse(stream);
            foreach (JsonProperty property in document.RootElement.EnumerateObject())
                if (string.Equals(property.Name, nameof(RoomVisualLayoutDocument.FormatVersion), StringComparison.OrdinalIgnoreCase))
                    return property.Value.GetInt32();
        }
        catch (Exception error) when (error is JsonException or InvalidOperationException or FormatException)
        {
            throw new InvalidDataException($"Room layout override {path} is not a layout document: {error.Message}", error);
        }
        throw new InvalidDataException($"Room layout override {path} has no format version.");
    }

    /// <summary>A plane of appearances as a document writes it.</summary>
    private static RoomLayoutPlaneDocument WritePlane(RoomBlockAppearance[] blocks) =>
        new([.. blocks.Select(block => block.Block)], new string(blocks.Select(block => MirroringCodes[block.Mirroring]).ToArray()));

    /// <summary>A document's plane as appearances.</summary>
    /// <exception cref="InvalidDataException">The plane's arrays differ in length or hold an unknown mirroring code.</exception>
    private static RoomBlockAppearance[] ReadPlane(RoomLayoutPlaneDocument plane)
    {
        if (plane.Blocks is null || plane.Mirroring is null || plane.Blocks.Length != plane.Mirroring.Length)
            throw new InvalidDataException("A layout plane needs one mirroring code per block.");
        var blocks = new RoomBlockAppearance[plane.Blocks.Length];
        for (int index = 0; index < blocks.Length; index++)
        {
            if (!MirroringByCode.TryGetValue(plane.Mirroring[index], out BlockMirroring mirroring))
                throw new InvalidDataException($"Mirroring code '{plane.Mirroring[index]}' is not one of {string.Concat(MirroringByCode.Keys)}.");
            blocks[index] = new RoomBlockAppearance(plane.Blocks[index], mirroring);
        }
        return blocks;
    }

    /// <summary>Finds each distinct room level and confirms the states sharing it agree on its block row stride.</summary>
    private static ReadOnlyDictionary<RoomLevelId, int> BuildRetailLevels()
    {
        var levels = new Dictionary<RoomLevelId, int>();
        foreach (RoomHeaderDefinition header in RoomHeaderDefinitions.All)
        {
            int width = header.Size.ToRoomBlocks().X;
            foreach (RoomStateSettings state in RoomStateSelection.GetStates(header))
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

    /// <summary>Decompresses and decodes one room level and keeps what its blocks show.</summary>
    private static RoomVisualLayoutDocument Decode(ISnesAddressSpace bus,
        RoomLevelId level, int widthInBlocks)
    {
        byte[] stream = RomDataReader.Decompress(CartridgeImportSource.Require(bus), RoomLevelSourceDefinitions.Addresses[level]);
        InstalledRoomLevel decoded = RoomLevelStreamDecoder.Decode(level, stream, widthInBlocks);
        return new RoomVisualLayoutDocument(FormatVersion, level.Name, widthInBlocks, decoded.Size.Y,
            WritePlane([.. decoded.Foreground.Select(block => block.Appearance)]), WritePlane([.. decoded.Background]));
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
    /// <summary>Editable initial foreground and background appearances with fixed level identity and block dimensions.</summary>
    [SpatialComponents("External format: per-level room-layout JSON stores widthInBlocks/heightInBlocks.")]
    private sealed record RoomVisualLayoutDocument(int FormatVersion, string Level,
        int WidthInBlocks, int HeightInBlocks, RoomLayoutPlaneDocument Foreground,
        RoomLayoutPlaneDocument Background)
    {
        /// <summary>Converts document dimensions into the room-block extent used for validation.</summary>
        [JsonIgnore]
        public Extent16<RoomBlocks> Size => new(checked((ushort)WidthInBlocks), checked((ushort)HeightInBlocks));
    }
    /// <summary>One plane of a layout document: each block's number, row by row, and one mirroring code per block.</summary>
    private sealed record RoomLayoutPlaneDocument(int[] Blocks, string Mirroring);
    /// <summary>A version-two layout override, which held twelve-bit level words.</summary>
    [SpatialComponents("External format: version-two room-layout JSON stores widthInBlocks/heightInBlocks.")]
    private sealed record WordRoomVisualLayoutDocument(int FormatVersion, string Level,
        int WidthInBlocks, int HeightInBlocks, ushort[] ForegroundVisualWords,
        ushort[] BackgroundVisualWords);
    /// <summary>A version-one layout override, which named its level by the level's cartridge source.</summary>
    [SpatialComponents("External format: version-one room-layout JSON stores widthInBlocks/heightInBlocks.")]
    private sealed record LegacyRoomVisualLayoutDocument(int FormatVersion, int SourceAddress,
        int WidthInBlocks, int HeightInBlocks, ushort[] ForegroundVisualWords,
        ushort[] BackgroundVisualWords);
}

using SuperMetroid.Core;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.AssetExtraction.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Installs what each room level's blocks initially show (#1469), without terrain, collision or behaviours. A player's
/// in-place edit of a level's JSON may alter only those appearances; gameplay keeps the installed terrain and all later
/// room-object writes.
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
    private const int FormatVersion = 4;

    /// <summary>The character a document writes for each mirroring.</summary>
    private static readonly Dictionary<BlockMirroring, char> MirroringCodes = new()
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

    /// <summary>Formats the stable per-level installed JSON filename without validating the level.</summary>
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

    /// <summary>Checks the complete retail-level manifest, then loads every level's installed initial room visual layout as the player left it.</summary>
    /// <param name="directory">Family directory containing every per-level JSON file and the required provenance/dimension manifest.</param>
    /// <returns>A ROM-independent catalog of the installed initial foreground and background appearances, leaving terrain, behaviours and later room-object writes alone.</returns>
    /// <remarks>Strict JSON must retain the level, original block dimensions, array extents, valid block numbers and mirroring codes; the documents are not compared with cartridge bytes.</remarks>
    /// <exception cref="ArgumentException"><paramref name="directory"/> is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Provenance, coverage, strict JSON, level identity, dimensions, block numbers or mirroring codes are invalid.</exception>
    /// <exception cref="IOException">The manifest or a level file cannot be read.</exception>
    public static RoomLevelArtworks Load(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string manifestPath = Path.Combine(directory, ManifestFileName);
        using var manifestStream = File.OpenRead(manifestPath);
        LayoutFileManifest manifest = JsonAssetDocument.Read<LayoutFileManifest>(
            manifestStream, JsonOptions, $"room-layout manifest {manifestPath}");
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase) ||
            manifest.Entries is null || manifest.Entries.Count != RetailLevels.Count)
            throw new InvalidDataException(
                $"Room-layout manifest {manifestPath} does not cover the pinned retail levels.");

        var layouts = new Dictionary<RoomLevelId, RoomLevelArtwork>();
        foreach ((string name, LayoutFileEntry entry) in manifest.Entries)
        {
            if (entry?.Level is null || name != LevelFileName(entry.Id) ||
                !RetailLevels.TryGetValue(entry.Id, out int width) ||
                entry.Size.X != width || entry.Size.Y <= 0)
                throw new InvalidDataException(
                    $"Room-layout manifest {manifestPath} has an invalid level entry.");
            string path = Path.Combine(directory, name);
            using PooledFileBytes json = PooledFileBytes.Read(path);
            using MemoryStream jsonStream = json.OpenRead();
            RoomVisualLayoutDocument document = JsonAssetDocument.Read<RoomVisualLayoutDocument>(
                jsonStream, JsonOptions, $"room layout {path}");
            if (document.FormatVersion != FormatVersion ||
                document.Level != entry.Level ||
                document.Size != entry.Size ||
                document.Foreground is null ||
                document.Background is null)
                throw new InvalidDataException($"Room layout {path} has incompatible dimensions or identity.");
            RoomLevelArtwork layout;
            try
            {
                layout = new RoomLevelArtwork(entry.Id, document.Size,
                    ReadPlane(document.Foreground), ReadPlane(document.Background));
            }
            catch (Exception error) when (error is InvalidDataException or ArgumentException)
            {
                throw new InvalidDataException($"Invalid room layout {path}: {error.Message}", error);
            }
            if (!layouts.TryAdd(entry.Id, layout))
                throw new InvalidDataException($"Room-layout manifest {manifestPath} repeats a level.");
        }
        return new RoomLevelArtworks(layouts, RoomVisualLayoutSourceDefinitions.All);
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
        foreach (RoomDefinition header in RetailRooms.All)
        {
            int width = header.Size.ToRoomBlocks().X;
            foreach (RoomStateSettings state in RoomStateLists.GetStates(header))
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
}

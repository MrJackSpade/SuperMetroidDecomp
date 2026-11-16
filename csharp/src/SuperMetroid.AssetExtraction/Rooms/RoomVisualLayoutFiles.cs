using SuperMetroid.Core;
using SuperMetroid.Core.Progression;
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
    /// <summary>
    /// Schema revision of the per-level layout documents; version 3 holds block numbers and mirroring instead of level words.
    /// No manifest lists the documents (#1693): the levels are the retail rooms', and each document names its level and size.
    /// </summary>
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

    /// <summary>Strict camel-case JSON encoding for the layout documents.</summary>
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

    /// <summary>
    /// The areas of the retail rooms using each distinct level, in the game's area order: a save or map room's level is
    /// shared by rooms of several areas, which give the area tables' reactions their meaning (#1805).
    /// </summary>
    public static IReadOnlyDictionary<RoomLevelId, IReadOnlyList<AreaId>> RetailLevelAreas { get; } =
        RetailRooms.All
            .SelectMany(header => RoomStateLists.GetStates(header).Select(state => new LevelUse(state.Level, header.Area)))
            .GroupBy(use => use.Level, use => use.Area)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<AreaId>)[.. AreaIds.All.Where(group.Contains)]);

    /// <summary>A retail room state's level and its room's area.</summary>
    /// <param name="Level">The level the state uses.</param>
    /// <param name="Area">The area of the state's room.</param>
    private sealed record LevelUse(RoomLevelId Level, AreaId Area);

    /// <summary>Formats the stable per-level installed JSON filename without validating the level.</summary>
    /// <param name="level">The level, shared by all room states using it; membership validation occurs during loading.</param>
    /// <returns><c>level-</c> followed by the level's name and <c>.json</c>.</returns>
    public static string LevelFileName(RoomLevelId level) => $"level-{level.Name}.json";

    /// <summary>Decompresses every distinct retail room level and exports what its foreground and background blocks show, without terrain or behaviours.</summary>
    /// <param name="bus">Non-null import-capable cartridge source for the compressed level streams identified by <see cref="RetailLevels"/>.</param>
    /// <param name="directory">Family directory, created if absent; receives new per-level JSON files and no manifest (#1693).</param>
    /// <remarks>Widths and heights use 16-pixel blocks. The foreground grid determines height. Create-new writes refuse existing files and do not roll back earlier outputs.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory is blank or the address space lacks cartridge import access.</exception>
    /// <exception cref="InvalidDataException">A compressed level, its foreground or behaviour layer, row stride, or background is invalid.</exception>
    /// <exception cref="IOException">An output exists or filesystem access fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Directory.CreateDirectory(directory);
        foreach ((RoomLevelId level, int widthInBlocks) in RetailLevels.OrderBy(item => item.Key.Name, StringComparer.Ordinal))
        {
            byte[] json = JsonSerializer.SerializeToUtf8Bytes(Decode(bus, level, widthInBlocks), JsonOptions);
            using var file = new FileStream(Path.Combine(directory, LevelFileName(level)), FileMode.CreateNew, FileAccess.Write);
            file.Write(json);
        }
    }

    /// <summary>Loads every retail level's installed initial room visual layout as the player left it.</summary>
    /// <param name="directory">Family directory containing every retail level's JSON file.</param>
    /// <returns>A ROM-independent catalog of the installed initial foreground and background appearances, leaving terrain, behaviours and later room-object writes alone.</returns>
    /// <remarks>Strict JSON must retain the level, its rooms' block width, a positive height, array extents, valid block numbers and mirroring codes; the documents are not compared with cartridge bytes.</remarks>
    /// <exception cref="ArgumentException"><paramref name="directory"/> is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">A level file is missing, or its strict JSON, level identity, dimensions, block numbers or mirroring codes are invalid.</exception>
    /// <exception cref="IOException">A level file cannot be read.</exception>
    public static RoomLevelArtworks Load(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        var layouts = new Dictionary<RoomLevelId, RoomLevelArtwork>();
        // The levels are the retail rooms' (#1693): no manifest lists them.
        foreach ((RoomLevelId level, int width) in RetailLevels.OrderBy(item => item.Key.Name, StringComparer.Ordinal))
        {
            string path = Path.Combine(directory, LevelFileName(level));
            if (!File.Exists(path))
                throw new InvalidDataException($"Room layout {path} is missing.");
            using PooledFileBytes json = PooledFileBytes.Read(path);
            using MemoryStream jsonStream = json.OpenRead();
            RoomVisualLayoutDocument document = JsonAssetDocument.Read<RoomVisualLayoutDocument>(
                jsonStream, JsonOptions, $"room layout {path}");
            if (document.FormatVersion != FormatVersion ||
                document.Level != level.Name ||
                document.Size.X != width || document.Size.Y <= 0 ||
                document.Foreground is null ||
                document.Background is null)
                throw new InvalidDataException($"Room layout {path} has incompatible dimensions or identity.");
            RoomLevelArtwork layout;
            try
            {
                layout = new RoomLevelArtwork(level, document.Size,
                    ReadPlane(document.Foreground), ReadPlane(document.Background));
            }
            catch (Exception error) when (error is InvalidDataException or ArgumentException)
            {
                throw new InvalidDataException($"Invalid room layout {path}: {error.Message}", error);
            }
            layouts.Add(level, layout);
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
        InstalledRoomLevel decoded = RoomLevelStreamDecoder.Decode(level, stream, widthInBlocks, RetailLevelAreas[level]);
        return new RoomVisualLayoutDocument(FormatVersion, level.Name, widthInBlocks, decoded.Size.Y,
            WritePlane([.. decoded.Foreground.Select(block => block.Appearance)]), WritePlane([.. decoded.Background]));
    }

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

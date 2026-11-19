using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Installs where every retail room's door blocks lead (#2090): for each room and each level its states use, each door its
/// door blocks lead through, by name, with the blocks leading through it. The cartridge numbers a door block's door by its
/// position in the room's door list (<see cref="RoomDoorListDefinitions"/>); the import resolves that position once, so
/// nothing the game loads holds a list or a position in one.
/// </summary>
/// <remarks>
/// A level is the room's own, but a save or map room's level is shared by several rooms, each leading the same door
/// blocks through its own doors, so the document names each room's door blocks in each of its levels. A door block whose
/// position lies past its room's list leads nowhere and is not named, as no door of the room is at that position; Samus
/// meeting one fails loudly (<see cref="RoomLevelData.DoorExitAt"/>).
/// </remarks>
internal static class RoomDoorBlockFiles
{
    /// <summary>The document's file name in the room-level family's directory.</summary>
    private const string FileName = "room-door-blocks.json";

    /// <summary>Document schema version this loader accepts.</summary>
    private const int FormatVersion = 1;

    /// <summary>The name the document gives the elevator ride, which is no door definition.</summary>
    private const string ElevatorRideName = "ElevatorPseudoDoor";

    /// <summary>Serializer settings shared by the document's writes and reads.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectRequiredConstructorParameters = true,
        WriteIndented = true,
    };

    /// <summary>
    /// Every retail room's use of each level its states use, the complete domain of installed door blocks: the document
    /// names each, and the catalog requires each (<see cref="InstalledRoomLevelStreams"/>).
    /// </summary>
    internal static IEnumerable<RoomLevelUse> RetailUses => RetailRooms.All.SelectMany(room =>
        RoomStateLists.GetStates(room).Select(state => state.Level).Distinct().Select(level => new RoomLevelUse(room, level)));

    /// <summary>
    /// Writes where every retail room's door blocks lead in each level its states use, from each level's door blocks and the
    /// position in the room's door list each leads through.
    /// </summary>
    /// <param name="directory">The family directory, which receives the new document.</param>
    /// <param name="doorLinks">Each retail level's door blocks and their door-list positions.</param>
    /// <exception cref="InvalidDataException">A room with door blocks has no door list.</exception>
    /// <exception cref="IOException">The document exists or filesystem access fails.</exception>
    internal static void Extract(string directory, IReadOnlyDictionary<RoomLevelId, IReadOnlyDictionary<Position16<RoomBlocks>, int>> doorLinks)
    {
        Ensure.NotNull(doorLinks);
        var rooms = new SortedDictionary<string, SortedDictionary<string, SortedDictionary<string, DoorBlockDocument[]>>>(StringComparer.Ordinal);
        foreach (IGrouping<RoomDefinition, RoomLevelUse> room in RetailUses.GroupBy(use => use.Room))
        {
            var levels = new SortedDictionary<string, SortedDictionary<string, DoorBlockDocument[]>>(StringComparer.Ordinal);
            foreach (RoomLevelUse use in room)
                levels.Add(use.Level.Name, Exits(room.Key, use.Level, doorLinks[use.Level]));
            rooms.Add(room.Key.Name, levels);
        }
        using var output = new FileStream(Path.Combine(directory, FileName), FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(output, new RoomDoorBlocksDocument(FormatVersion, rooms), JsonOptions);
    }

    /// <summary>The blocks leading through each exit of <paramref name="room"/> in one of its levels, by the exit's name.</summary>
    private static SortedDictionary<string, DoorBlockDocument[]> Exits(RoomDefinition room, RoomLevelId level,
        IReadOnlyDictionary<Position16<RoomBlocks>, int> links)
    {
        var exits = new SortedDictionary<string, DoorBlockDocument[]>(StringComparer.Ordinal);
        if (links.Count == 0)
            return exits;
        IReadOnlyList<DoorBlockExit> list = RoomDoorListDefinitions.ByRoom.TryGetValue(room, out IReadOnlyList<DoorBlockExit>? doors)
            ? doors
            : throw new InvalidDataException($"Room {room.Name} has door blocks in level {level} but no door list.");
        foreach (IGrouping<DoorBlockExit, Position16<RoomBlocks>> exit in links
                     .Where(link => link.Value < list.Count)
                     .OrderBy(link => link.Key.Y).ThenBy(link => link.Key.X)
                     .GroupBy(link => list[link.Value], link => link.Key))
            exits.Add(NameOf(exit.Key), [.. exit.Select(block => new DoorBlockDocument(block.X, block.Y))]);
        return exits;
    }

    /// <summary>
    /// Reads the document: each retail room's door blocks in each level it names. The catalog it is loaded into requires
    /// exactly <see cref="RetailUses"/>.
    /// </summary>
    /// <param name="directory">The family directory holding the document.</param>
    /// <returns>Each room's door blocks, keyed by the room and the level.</returns>
    /// <exception cref="InvalidDataException">
    /// The document is missing, of another version, or names an unknown room or door, or names a block twice; the message
    /// names the file.
    /// </exception>
    /// <exception cref="IOException">The document cannot be read.</exception>
    internal static IReadOnlyDictionary<RoomLevelUse, IReadOnlyList<RoomDoorBlock>> Load(string directory)
    {
        string path = Path.Combine(directory, FileName);
        if (!File.Exists(path))
            throw new InvalidDataException($"Room door blocks {path} are missing.");
        RoomDoorBlocksDocument document;
        using (FileStream stream = File.OpenRead(path))
            document = JsonAssetDocument.Read<RoomDoorBlocksDocument>(stream, JsonOptions, $"room door blocks {path}");
        if (document.Version != FormatVersion)
            throw new InvalidDataException($"Room door blocks {path} have unsupported version {document.Version}.");
        Dictionary<string, RoomDefinition> roomsByName = RetailRooms.All.ToDictionary(room => room.Name, StringComparer.Ordinal);
        Dictionary<string, DoorBlockExit> exitsByName = DoorDefinitions.Definitions
            .Select(DoorBlockExit.Physical)
            .Append(DoorBlockExit.ElevatorRide)
            .ToDictionary(NameOf, StringComparer.Ordinal);

        var doorBlocks = new Dictionary<RoomLevelUse, IReadOnlyList<RoomDoorBlock>>();
        foreach ((string roomName, SortedDictionary<string, SortedDictionary<string, DoorBlockDocument[]>> levels) in
                 document.Rooms ?? throw new InvalidDataException($"Room door blocks {path} name no rooms."))
        {
            RoomDefinition room = roomsByName.TryGetValue(roomName, out RoomDefinition? named)
                ? named
                : throw new InvalidDataException($"Room door blocks {path} name unknown room '{roomName}'.");
            foreach ((string levelName, SortedDictionary<string, DoorBlockDocument[]> exits) in levels)
            {
                var level = new RoomLevelId(levelName);
                var blocks = new List<RoomDoorBlock>();
                foreach ((string exitName, DoorBlockDocument[] positions) in exits)
                {
                    DoorBlockExit exit = exitsByName.TryGetValue(exitName, out DoorBlockExit found)
                        ? found
                        : throw new InvalidDataException($"Room door blocks {path} name unknown door '{exitName}' in {roomName}.");
                    blocks.AddRange(positions.Select(position => new RoomDoorBlock(position.Block(path), exit)));
                }
                if (blocks.Select(block => block.Block).Distinct().Count() != blocks.Count)
                    throw new InvalidDataException($"Room door blocks {path} name a block of {roomName} in level '{levelName}' twice.");
                doorBlocks.Add(new RoomLevelUse(room, level), blocks);
            }
        }
        return doorBlocks;
    }

    /// <summary>The name the document gives an exit: its door's, or the elevator ride's.</summary>
    private static string NameOf(DoorBlockExit exit) => exit.Door?.Name ?? ElevatorRideName;

    /// <summary>Each retail room's door blocks in each level its states use, by the room's, the level's and each exit's name.</summary>
    private sealed record RoomDoorBlocksDocument(int Version,
        SortedDictionary<string, SortedDictionary<string, SortedDictionary<string, DoorBlockDocument[]>>>? Rooms);

    /// <summary>A door block: its column and row in its level's grid.</summary>
    [SpatialComponents("External format: the room door-blocks JSON stores each door block as its x/y block in the level's grid.")]
    private sealed record DoorBlockDocument(int X, int Y)
    {
        /// <summary>The block, which must be a 16-bit column and row.</summary>
        /// <exception cref="InvalidDataException">The column or row is negative or past 16 bits.</exception>
        public Position16<RoomBlocks> Block(string path) =>
            X is >= 0 and <= ushort.MaxValue && Y is >= 0 and <= ushort.MaxValue
                ? new Position16<RoomBlocks>(X, Y)
                : throw new InvalidDataException($"Room door blocks {path} name block ({X}, {Y}), which lies outside any level.");
    }
}

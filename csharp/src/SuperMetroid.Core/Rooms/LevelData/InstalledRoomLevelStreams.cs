namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The installed terrain of every room level (#1469): each level's grid of terrain blocks, behaviours and
/// background appearances, and the terrain past its grid, decoded once at installation, and where each room's door blocks
/// in each level its states use lead (#2090). The catalog is stock-only: replaceable initial appearances are selected
/// separately by <see cref="RoomLevelArtworks"/> and never change it.
/// </summary>
public sealed class InstalledRoomLevelStreams
{
    /// <summary>Installed levels keyed by their room level.</summary>
    private readonly Dictionary<RoomLevelId, InstalledRoomLevel> levels;

    /// <summary>Each room's door blocks in each level its states use.</summary>
    private readonly Dictionary<RoomLevelUse, IReadOnlyList<RoomDoorBlock>> doorBlocks;

    /// <summary>
    /// Copies a validated installation and requires exactly every room level, and the door blocks of exactly every room's use
    /// of a level.
    /// </summary>
    /// <param name="levels">The installed levels keyed by their room level.</param>
    /// <param name="requiredLevels">Every room level, each of which the installation must hold.</param>
    /// <param name="doorBlocks">
    /// Each room's door blocks in each level its states use, keyed by the room and the level; every block lies in the level's
    /// grid.
    /// </param>
    /// <param name="requiredUses">Every room's use of a level, whose door blocks the installation must hold.</param>
    /// <exception cref="InvalidDataException">
    /// A level is null, missing or unknown, a use's door blocks are missing or unknown, or a door block lies outside its level's
    /// grid.
    /// </exception>
    internal InstalledRoomLevelStreams(IReadOnlyDictionary<RoomLevelId, InstalledRoomLevel> levels, IEnumerable<RoomLevelId> requiredLevels,
        IReadOnlyDictionary<RoomLevelUse, IReadOnlyList<RoomDoorBlock>> doorBlocks, IEnumerable<RoomLevelUse> requiredUses)
    {
        Ensure.NotNull(levels);
        var required = Ensure.NotNull(requiredLevels).ToHashSet();
        this.levels = new Dictionary<RoomLevelId, InstalledRoomLevel>(levels.Count);
        foreach ((RoomLevelId level, InstalledRoomLevel installed) in levels)
        {
            if (!required.Contains(level))
                throw new InvalidDataException($"Installed room levels contain unknown level {level}.");
            this.levels.Add(level, installed ?? throw new InvalidDataException($"Installed room level {level} is null."));
        }
        foreach (RoomLevelId level in required)
            if (!this.levels.ContainsKey(level))
                throw new InvalidDataException($"Installed room levels lack required level {level}.");
        var requiredDoorBlocks = Ensure.NotNull(requiredUses).ToHashSet();
        this.doorBlocks = new Dictionary<RoomLevelUse, IReadOnlyList<RoomDoorBlock>>(Ensure.NotNull(doorBlocks).Count);
        foreach ((RoomLevelUse use, IReadOnlyList<RoomDoorBlock> blocks) in doorBlocks)
        {
            if (!requiredDoorBlocks.Contains(use))
                throw new InvalidDataException($"Installed door blocks name level {use.Level} for {use.Room.Name}, whose states do not use it.");
            if (blocks is null)
                throw new InvalidDataException($"Installed door blocks of {use.Room.Name} in level {use.Level} are null.");
            Extent16<RoomBlocks> size = Get(use.Level).Size;
            foreach (RoomDoorBlock door in blocks)
                if (!size.Contains(door.Block))
                    throw new InvalidDataException(
                        $"{use.Room.Name}'s door block {door.Block} lies outside level {use.Level}'s {size.X}x{size.Y}-block grid.");
            this.doorBlocks.Add(use, blocks);
        }
        foreach (RoomLevelUse use in requiredDoorBlocks)
            if (!this.doorBlocks.ContainsKey(use))
                throw new InvalidDataException($"Installed door blocks lack {use.Room.Name}'s level {use.Level}.");
    }

    /// <summary>Where <paramref name="room"/>'s door blocks in <paramref name="level"/> lead (#2090).</summary>
    /// <param name="room">The room whose state names the level.</param>
    /// <param name="level">The level the room's state names.</param>
    /// <exception cref="InvalidDataException">The installation holds no door blocks of the room in the level; room loads never fall back to game data.</exception>
    public IReadOnlyList<RoomDoorBlock> DoorBlocks(RoomDefinition room, RoomLevelId level) =>
        doorBlocks.TryGetValue(new RoomLevelUse(Ensure.NotNull(room), level), out IReadOnlyList<RoomDoorBlock>? blocks)
            ? blocks
            : throw new InvalidDataException($"Installed room-level data lacks {room.Name}'s door blocks in level {level}.");

    /// <summary>Reads one installed level.</summary>
    /// <param name="level">The level the room state names.</param>
    /// <returns>The level's installed terrain.</returns>
    /// <exception cref="InvalidDataException">The requested level is not installed; room loads never fall back to game data.</exception>
    public InstalledRoomLevel Get(RoomLevelId level) =>
        levels.TryGetValue(level, out InstalledRoomLevel? installed)
            ? installed
            : throw new InvalidDataException($"Installed room-level data lacks level {level}.");
}

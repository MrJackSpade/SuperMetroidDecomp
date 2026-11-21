namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>One block of a room's terrain grid as collision reads it: where it is, its terrain and its behaviour.</summary>
public readonly record struct RoomCollisionBlock
{
    /// <summary>A block the read found in the room, at <paramref name="position"/>.</summary>
    /// <param name="position">
    /// Where the block lies, in the room's columns: a block of the grid, or one past the grid's last row that a room
    /// object or an enemy terrain read reaches.
    /// </param>
    /// <param name="terrain">What the block shows and how it collides.</param>
    /// <param name="behavior">
    /// The block's behaviour parameter. Its collision kind determines which of its named views applies; it is not a
    /// universal terrain identifier.
    /// </param>
    public RoomCollisionBlock(Position16<RoomBlocks> position, RoomTerrainBlock terrain, RoomBlockBehavior behavior)
        : this(position, (Position16<RoomBlocks>?)position, terrain, behavior)
    {
    }

    /// <summary>A block the read found at <paramref name="at"/>, which lies in the room at <paramref name="position"/>, or outside it.</summary>
    private RoomCollisionBlock(Position16<RoomBlocks> at, Position16<RoomBlocks>? position, RoomTerrainBlock terrain, RoomBlockBehavior behavior)
    {
        At = at;
        Position = position;
        Terrain = terrain;
        Behavior = behavior;
    }

    /// <summary>The edge-solid block a probe outside the room's width or height reads, where the probe named <paramref name="probed"/>.</summary>
    /// <param name="probed">The block the probe named, outside the room.</param>
    /// <param name="terrain">The edge-solid terrain.</param>
    /// <param name="behavior">The edge-solid block's behaviour parameter.</param>
    internal static RoomCollisionBlock OutsideRoom(Position16<RoomBlocks> probed, RoomTerrainBlock terrain, RoomBlockBehavior behavior) =>
        new(probed, null, terrain, behavior);

    /// <summary>
    /// The block the read found, where a reaction to it takes place: for the edge-solid block outside the room, the block
    /// the probe named, where no reaction to the edge-solid block takes place.
    /// </summary>
    public Position16<RoomBlocks> At { get; }

    /// <summary>Where the block lies in the room, or none for the edge-solid block a probe outside the room reads.</summary>
    public Position16<RoomBlocks>? Position { get; }

    /// <summary>What the block shows and how it collides.</summary>
    public RoomTerrainBlock Terrain { get; init; }

    /// <summary>The block's behaviour parameter, whose named view its collision kind selects.</summary>
    public RoomBlockBehavior Behavior { get; }

    /// <summary>How the block collides.</summary>
    public RoomCollisionType CollisionType => Terrain.Collision;

    /// <summary>
    /// The reaction the block's kind reads from its behaviour (#2554), typed by its family; none for a kind that reads no
    /// reaction. A room's level holds no block of a reacting kind without its reaction: it checks its blocks as the room
    /// loads and refuses a write that would leave one (<see cref="RoomLevelData"/>), so a block of a reacting family always
    /// carries its family's reaction.
    /// </summary>
    public BlockReaction? Reaction => Behavior.ReactionUnder(CollisionType);

    /// <summary>
    /// The columns a horizontal extension or the rows a vertical one leads a lookup on by, from this block in the room's
    /// reading order (<see cref="RoomReadingOrder"/>).
    /// </summary>
    /// <exception cref="InvalidOperationException">The block is not an extension.</exception>
    internal Offset16<RoomBlocks> ExtensionLink =>
        CollisionType == RoomCollisionType.HorizontalExtension ? Offset16<RoomBlocks>.Of(Behavior.ExtensionOffset, 0)
        : CollisionType == RoomCollisionType.VerticalExtension ? Offset16<RoomBlocks>.Of(0, Behavior.ExtensionOffset)
        : throw new InvalidOperationException($"A {CollisionType} block has no extension link.");
}

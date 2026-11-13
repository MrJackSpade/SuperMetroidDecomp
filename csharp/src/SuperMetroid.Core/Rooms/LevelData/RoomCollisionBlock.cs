namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>One block of a room's terrain grid as collision reads it: where it is, its terrain and its behaviour.</summary>
/// <param name="Position">
/// Where the block lies, in the room's columns: a block of the grid, or one past the grid's last row that a room object
/// or an enemy terrain read reaches; none for the edge-solid block a probe outside the room's width or height reads.
/// </param>
/// <param name="Terrain">What the block shows and how it collides.</param>
/// <param name="Behavior">
/// The block's behaviour parameter. Its collision kind determines which of its named views applies; it is not a universal
/// terrain identifier.
/// </param>
public readonly record struct RoomCollisionBlock(Position16<RoomBlocks>? Position, RoomTerrainBlock Terrain, RoomBlockBehavior Behavior)
{
    /// <summary>How the block collides.</summary>
    public RoomCollisionType CollisionType => Terrain.Collision;

    /// <summary>The block the read found, where a reaction to it takes place.</summary>
    /// <exception cref="InvalidOperationException">The read found the edge-solid block outside the room, which lies at no block.</exception>
    public Position16<RoomBlocks> At =>
        Position ?? throw new InvalidOperationException("The collision read found no block of the room: it read the solid terrain outside the room.");

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

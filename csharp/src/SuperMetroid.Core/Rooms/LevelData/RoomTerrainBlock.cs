namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// One block of a room's terrain: what it shows and how it collides. Its behaviour parameter is kept beside it in the
/// room's grid (<see cref="RoomBlockBehavior"/>), because a block whose collision kind changes keeps its parameter.
/// </summary>
/// <param name="Appearance">What the block shows.</param>
/// <param name="Collision">How the block collides.</param>
public readonly record struct RoomTerrainBlock(RoomBlockAppearance Appearance, RoomCollisionType Collision)
{
    /// <summary>The block showing <paramref name="block"/> mirrored by <paramref name="mirroring"/> and colliding as <paramref name="collision"/>.</summary>
    /// <param name="block">The block's number in the tileset's block set, 0 through <see cref="RoomBlockAppearance.LastBlock"/>.</param>
    /// <param name="mirroring">How the block is mirrored.</param>
    /// <param name="collision">How the block collides.</param>
    public static RoomTerrainBlock Of(int block, BlockMirroring mirroring, RoomCollisionType collision) =>
        new(RoomBlockAppearance.Of(block, mirroring), collision);

    /// <summary>
    /// Whether the block stops a probe that only asks whether something may attach to terrain: true for every solid
    /// kind. The probe ignores slope geometry and where an extension leads.
    /// </summary>
    public bool StopsAttachment => Collision.IsSolidKind();

    /// <summary>This block colliding as <paramref name="collision"/>, showing the same picture.</summary>
    public RoomTerrainBlock WithCollision(RoomCollisionType collision) => this with { Collision = collision };

    /// <summary>This block showing <paramref name="appearance"/>, colliding the same way.</summary>
    public RoomTerrainBlock WithAppearance(RoomBlockAppearance appearance) => this with { Appearance = appearance };

    /// <summary>This block showing <paramref name="block"/> with the same mirroring, colliding the same way.</summary>
    public RoomTerrainBlock WithBlock(int block) => this with { Appearance = Appearance.WithBlock(block) };

    /// <summary>
    /// The block a deactivated room object leaves behind: a block of a solid kind becomes a plain solid block, any other
    /// becomes air, and the picture stays.
    /// </summary>
    public RoomTerrainBlock Deactivated() =>
        WithCollision(Collision.IsSolidKind() ? RoomCollisionType.SolidBlock : RoomCollisionType.Air);
}

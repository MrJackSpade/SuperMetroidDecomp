using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// A terrain of air, solid blocks and slopes alone (#2553), as the opening cinematic's collision rooms are: no block of it
/// reacts, so nothing Samus or her shots meet in it reaches a room object, and it has none.
/// </summary>
[RoomScopeBoundary("A cinematic scene's terrain, kept for the scene's lifetime; no gameplay room is loaded.")]
internal sealed partial class PlainTerrain : SamusTerrain
{
    /// <summary>The collision each plain block has.</summary>
    private static IdentityTable<PlainTerrainBlock, RoomCollisionType> Collision { get; } = new(new Dictionary<PlainTerrainBlock, RoomCollisionType>
    {
        [PlainTerrainBlock.Air] = RoomCollisionType.Air,
        [PlainTerrainBlock.Solid] = RoomCollisionType.SolidBlock,
        [PlainTerrainBlock.Slope] = RoomCollisionType.Slope,
    });

    /// <summary>Creates a plain terrain of <paramref name="size"/> holding <paramref name="blocks"/> row by row.</summary>
    /// <param name="size">The terrain's size in blocks.</param>
    /// <param name="blocks">Each block, row by row.</param>
    internal PlainTerrain(Extent16<RoomBlocks> size, ReadOnlySpan<PlainTerrainBlock> blocks)
        : base(RoomLevelData.CollisionOnly(size, CollisionOf(blocks)))
    {
    }

    /// <summary>A plain block, air, solid or a slope, reaches nothing: the met block carries no reaction.</summary>
    /// <param name="standing">The block a met cell stands for.</param>
    private protected override MetBlock Met(StandingBlock standing) => new(standing, null);

    /// <summary>The room collision of <paramref name="blocks"/>, block by block.</summary>
    private static RoomCollisionType[] CollisionOf(ReadOnlySpan<PlainTerrainBlock> blocks)
    {
        var collision = new RoomCollisionType[blocks.Length];
        for (int block = 0; block < blocks.Length; block++)
            collision[block] = Collision[blocks[block]];
        return collision;
    }
}

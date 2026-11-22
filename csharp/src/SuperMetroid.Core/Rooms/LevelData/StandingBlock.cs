namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The block a read stands for once any continuing cell has led to it (#2553): the block, and how it collides, which is no
/// extension (<see cref="RoomLevelData.StandingBlockFor"/>).
/// </summary>
/// <param name="Block">The block.</param>
/// <param name="CollisionType">How it collides.</param>
internal readonly partial record struct StandingBlock(RoomCollisionBlock Block, StandingCollisionType CollisionType)
{
    /// <summary>Where the block lies (<see cref="RoomCollisionBlock.Position"/>).</summary>
    internal Position16<RoomBlocks>? Position => Block.Position;

    /// <summary>The block the read found, where a reaction to it takes place (<see cref="RoomCollisionBlock.At"/>).</summary>
    /// <exception cref="InvalidOperationException">The read found the edge-solid block outside the room, which lies at no block.</exception>
    internal Position16<RoomBlocks> At => Block.At;

    /// <summary>The block's behaviour parameter (<see cref="RoomCollisionBlock.Behavior"/>).</summary>
    internal RoomBlockBehavior Behavior => Block.Behavior;

    /// <summary>What the block shows and how it collides (<see cref="RoomCollisionBlock.Terrain"/>).</summary>
    internal RoomTerrainBlock Terrain => Block.Terrain;
}

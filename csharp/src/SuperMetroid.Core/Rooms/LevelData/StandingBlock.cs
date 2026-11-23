namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The block a read stands for once any continuing cell has led to it (#2553): the block, and how it collides, which is no
/// extension (<see cref="RoomLevelData.StandingBlockFor"/>).
/// </summary>
/// <param name="Block">The block.</param>
/// <param name="CollisionType">How it collides.</param>
internal readonly partial record struct StandingBlock(RoomCollisionBlock Block, StandingCollisionType CollisionType)
{
}

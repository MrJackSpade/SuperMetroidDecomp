using SuperMetroid.Core.Rooms.LevelData;
namespace SuperMetroid.Verification.Enemies;

/// <summary>A block an enemy terrain block scan fixture places in a room of air.</summary>
/// <param name="Column">The block's column.</param>
/// <param name="Row">The block's row.</param>
/// <param name="Collision">The block's collision type.</param>
/// <param name="Behavior">The block's behaviour.</param>
internal sealed record EnemyTerrainScanBlock(int Column, int Row, RoomCollisionType Collision, RoomBlockBehavior Behavior);

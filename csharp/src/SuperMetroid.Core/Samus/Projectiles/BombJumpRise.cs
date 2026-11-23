using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The result of moving up under gravity, and whether the move went down.</summary>
/// <param name="Result">The upward move's block collision.</param>
/// <param name="MovedDown">Whether the move went down.</param>
public readonly partial record struct BombJumpRise(BlockMoveResult Result, bool MovedDown);

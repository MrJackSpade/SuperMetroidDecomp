using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The result of moving up under gravity, and whether the move went down.</summary>
/// <param name="Result">The result.</param>
/// <param name="MovedDown">The moved down.</param>
public readonly record struct BombJumpRise(BlockMoveResult Result, bool MovedDown);

using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Both bank-$94 scans performed by one grounded bank-$90 movement handler.</summary>
public readonly record struct GroundedMovementResult(
    BlockMoveResult Horizontal,
    BlockMoveResult Vertical);

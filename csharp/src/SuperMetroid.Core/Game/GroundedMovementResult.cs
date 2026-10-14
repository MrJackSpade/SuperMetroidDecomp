namespace SuperMetroid.Core.Game;

/// <summary>Both bank-$94 scans performed by one grounded bank-$90 movement handler.</summary>
public readonly record struct GroundedMovementResult(
    BlockMoveResult Horizontal,
    BlockMoveResult Vertical);

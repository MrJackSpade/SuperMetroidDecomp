namespace SuperMetroid.Core.Game;

/// <summary>Observable output of one special bomb-jump handler frame.</summary>
public readonly record struct BombJumpMovementResult(
    BlockMoveResult? Vertical,
    bool FellWithoutFloor = false);

using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Observable output of one special bomb-jump handler frame.</summary>
public readonly record struct BombJumpMovementResult(
    BlockMoveResult? Vertical,
    bool FellWithoutFloor = false);

using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Observable output of one frame of bomb-jump movement.</summary>
public readonly partial record struct BombJumpMovementResult(
    BlockMoveResult? Vertical,
    bool FellWithoutFloor = false);

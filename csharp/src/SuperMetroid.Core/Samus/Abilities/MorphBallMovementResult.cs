using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Collision and direction result produced by one ordinary Morph-Ball frame.</summary>
public readonly record struct MorphBallMovementResult(
    BlockMoveResult Vertical,
    bool Landed,
    bool HitCeiling);

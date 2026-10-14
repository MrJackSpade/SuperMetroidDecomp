namespace SuperMetroid.Core.Game;

/// <summary>Collision and direction result produced by one ordinary Morph-Ball frame.</summary>
public readonly record struct MorphBallMovementResult(
    BlockMoveResult Vertical,
    bool Landed,
    bool HitCeiling);

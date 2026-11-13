namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>
/// The behaviour Bull's main AI runs each frame.
/// Its values are arbitrary.
/// </summary>
public enum BullEnemyFunction
{
    /// <summary>Waits for the sixteen-call activation countdown to reach zero, reloads it, and selects Samus targeting.</summary>
    MovementDelay = 1,
    /// <summary>Captures the angle toward Samus, sets acceleration delta 24, and starts accelerating without moving that call.</summary>
    TargetSamus,
    /// <summary>Increases speed on configured interval expiries while below the maximum threshold, moves along the captured angle, and checks the 48-unit off-target deceleration.</summary>
    Accelerating,
    /// <summary>Moves before reducing speed on interval expiries, then clears speed/acceleration and returns to the movement delay when either magnitude is zero or signed-negative.</summary>
    Decelerating,
}

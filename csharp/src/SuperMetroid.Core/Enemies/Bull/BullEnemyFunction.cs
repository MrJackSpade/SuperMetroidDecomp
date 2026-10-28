namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>
/// The routine Bull's main AI runs each frame.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum BullEnemyFunction
{
    /// <summary>$A8:D92B, Function_Bull_MovementDelay: waits for the sixteen-call activation countdown to reach zero, reloads it, and selects Samus targeting.</summary>
    MovementDelay = 1,
    /// <summary>$A8:D940, Function_Bull_TargetSamus: captures the byte angle toward Samus, sets acceleration delta $18, and starts accelerating without moving that call.</summary>
    TargetSamus,
    /// <summary>$A8:D963, Function_Bull_Accelerating: increases speed on configured interval expiries while below the maximum threshold, moves along the captured angle, and checks native $30-unit off-target deceleration.</summary>
    Accelerating,
    /// <summary>$A8:D97C, Function_Bull_Decelerating: moves before reducing speed on interval expiries, then clears speed/acceleration and returns to the movement delay when either magnitude is zero or signed-negative.</summary>
    Decelerating,
}

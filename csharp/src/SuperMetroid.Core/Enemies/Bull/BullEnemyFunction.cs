namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>The literal bank-$A8 function word stored by Bull in common enemy variable A.</summary>
public enum BullEnemyFunction : ushort
{
    /// <summary>$A8:D92B, Function_Bull_MovementDelay: waits for the sixteen-call activation countdown to reach zero, reloads it, and selects Samus targeting.</summary>
    MovementDelay = 0xd92b,
    /// <summary>$A8:D940, Function_Bull_TargetSamus: captures the byte angle toward Samus, sets acceleration delta $18, and starts accelerating without moving that call.</summary>
    TargetSamus = 0xd940,
    /// <summary>$A8:D963, Function_Bull_Accelerating: increases speed on configured interval expiries while below the maximum threshold, moves along the captured angle, and checks native $30-unit off-target deceleration.</summary>
    Accelerating = 0xd963,
    /// <summary>$A8:D97C, Function_Bull_Decelerating: moves before reducing speed on interval expiries, then clears speed/acceleration and returns to the movement delay when either magnitude is zero or signed-negative.</summary>
    Decelerating = 0xd97c,
}

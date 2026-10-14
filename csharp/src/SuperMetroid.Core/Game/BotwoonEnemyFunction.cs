namespace SuperMetroid.Core.Game;

/// <summary>
/// Botwoon's main function, held in its variable D. Each member names the bank-$B3 routine it ports;
/// the slot stores the member, not the routine's address. Members start at one so a cleared slot is
/// never mistaken for a function.
/// </summary>
public enum BotwoonEnemyFunction : ushort
{
    /// <summary><c>$B3:9878 Function_Botwoon_Initial</c>: decrements the 256-update opening delay and selects hole traversal when it reaches zero.</summary>
    InitialDelay = 1,
    /// <summary><c>$B3:989D Function_Botwoon_GoThroughHole</c>: moves toward the target hole, detects crossing, and chooses an authored path or stationary spit action.</summary>
    ChooseNextAction,
    /// <summary><c>$B3:99A4 Function_Botwoon_MovingAround</c>: advances an authored movement path and body history until its terminator selects direct hole movement.</summary>
    FollowAuthoredPath,
    /// <summary><c>$B3:99E4 Function_Botwoon_Spitting</c>: holds position for the aimed spit animation/cooldown, then chooses another path and inside-hole state.</summary>
    SpitWhileHidden,
    /// <summary><c>$B3:9A46 Function_Botwoon_DeathSequence_PreDeathDelay</c>: increments the head's death timer from 240 to 256 while body actors begin their staggered death delays.</summary>
    DeathDelay,
    /// <summary><c>$B3:9A5E Function_Botwoon_DeathSequence_FallingToGround</c>: accelerates through the quadratic speed table until head Y reaches 200, then hides the landed head.</summary>
    HeadFalling,
    /// <summary><c>$B3:9ACA Function_Botwoon_DeathSequence_WaitForBodyToFallToGround</c>: waits for the final body point's landing flag before requesting drops and wall crumbling.</summary>
    WaitForBody,
    /// <summary><c>$B3:9AF9 Function_Botwoon_DeathSequence_CrumblingWall</c>: runs the 192-update wall-explosion phase, then deletes the head, sets the miniboss bit, and queues track three.</summary>
    WallExplosions,
}

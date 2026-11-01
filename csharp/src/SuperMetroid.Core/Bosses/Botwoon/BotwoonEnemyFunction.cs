namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// Botwoon's main function, held in its variable D. Each member names the routine it ports;
/// the slot stores the member, not the routine's address. Members start at one so a cleared slot is
/// never mistaken for a function.
/// </summary>
public enum BotwoonEnemyFunction
{
    /// <summary>Decrements the 256-update opening delay and selects hole traversal when it reaches zero.</summary>
    InitialDelay = 1,
    /// <summary>Moves toward the target hole, detects crossing, and chooses an authored path or stationary spit action.</summary>
    ChooseNextAction,
    /// <summary>Advances an authored movement path and body history until its terminator selects direct hole movement.</summary>
    FollowAuthoredPath,
    /// <summary>Holds position for the aimed spit animation/cooldown, then chooses another path and inside-hole state.</summary>
    SpitWhileHidden,
    /// <summary>Increments the head's death timer from 240 to 256 while body actors begin their staggered death delays.</summary>
    DeathDelay,
    /// <summary>Accelerates through the quadratic speed table until head Y reaches 200, then hides the landed head.</summary>
    HeadFalling,
    /// <summary>Waits for the final body point's landing flag before requesting drops and wall crumbling.</summary>
    WaitForBody,
    /// <summary>Runs the 192-update wall-explosion phase, then deletes the head, sets the miniboss bit, and queues track three.</summary>
    WallExplosions,
}

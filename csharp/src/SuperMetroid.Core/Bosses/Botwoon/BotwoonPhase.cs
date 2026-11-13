namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// The phase of Botwoon's fight he runs each update. Each member names what that phase of the fight does.
/// </summary>
public enum BotwoonPhase
{
    /// <summary>Decrements the 256-update opening delay and selects hole traversal when it reaches zero.</summary>
    InitialDelay = 1,
    /// <summary>Moves toward the target hole, detects crossing, and chooses an authored path or stationary spit action.</summary>
    ChooseNextAction,
    /// <summary>Advances an authored movement path and body history until the path ends and selects direct hole movement.</summary>
    FollowAuthoredPath,
    /// <summary>Holds position for the aimed spit animation/cooldown, then chooses another path and inside-hole state.</summary>
    SpitWhileHidden,
    /// <summary>Increments the head's death timer from 240 to 256 while body actors begin their staggered death delays.</summary>
    DeathDelay,
    /// <summary>Accelerates through the quadratic speed table until head Y reaches 200, then hides the landed head.</summary>
    HeadFalling,
    /// <summary>Waits for the final body point's landing flag before requesting drops and wall crumbling.</summary>
    WaitForBody,
    /// <summary>Runs the 192-update wall-explosion phase, then deletes the head, sets the miniboss bit, and queues the elevator song.</summary>
    WallExplosions,
}

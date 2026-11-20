namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// The phase of Botwoon's fight he runs each update. Each member names what that phase of the fight does.
/// </summary>
public enum BotwoonPhase
{
    /// <summary>Waits out the opening delay (<see cref="BotwoonFightDefinitions.OpeningDelay"/>), then starts Botwoon moving between his holes.</summary>
    InitialDelay,
    /// <summary>Moves toward the target hole, detects crossing, and chooses an authored path or stationary spit action.</summary>
    ChooseNextAction,
    /// <summary>Advances an authored movement path and body history until the path ends and selects direct hole movement.</summary>
    FollowAuthoredPath,
    /// <summary>Holds position for the aimed spit and its cooldown, then on the next update chooses another path and inside-hole state.</summary>
    SpitWhileHidden,
    /// <summary>Waits the head's pre-fall wait while the body segments begin their staggered death delays.</summary>
    DeathDelay,
    /// <summary>Speeds the head's fall along the quadratic speed curve until it lands on the floor (<see cref="BotwoonFightDefinitions.FloorY"/>), then hides it.</summary>
    HeadFalling,
    /// <summary>Waits for his tail segment to land before leaving his drops and crumbling the wall.</summary>
    WaitForBody,
    /// <summary>Runs the wall-explosion phase (<see cref="BotwoonDeathDefinitions.WallExplosionDuration"/>), then deletes the head, records the mini-boss's defeat, and queues the elevator song.</summary>
    WallExplosions,
}

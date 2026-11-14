namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>
/// The way a Cacatac starts. The restore-patrol cue that begins its idle animation sends every direction but left to the
/// right, so a Cacatac placed to stop at first patrols right from its first idle step.
/// </summary>
public enum CacatacDirection
{
    /// <summary>Patrols left, and returns to patrolling left whenever the restore-patrol cue runs, as after an attack.</summary>
    Left,
    /// <summary>Patrols right, and returns to patrolling right whenever the restore-patrol cue runs, as after an attack.</summary>
    Right,
    /// <summary>Placed to stop at first: it stands still until the restore-patrol cue sends it right.</summary>
    InitialStop,
}

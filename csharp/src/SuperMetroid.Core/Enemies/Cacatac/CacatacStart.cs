namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>How a Cacatac starts (#2369): patrolling at once, or standing still until its first idle step sends it patrolling.</summary>
internal enum CacatacStart
{
    /// <summary>Patrols its direction from the start.</summary>
    Patrolling,

    /// <summary>Stands still until the restore-patrol cue of its first idle step sends it patrolling.</summary>
    Stopped,
}

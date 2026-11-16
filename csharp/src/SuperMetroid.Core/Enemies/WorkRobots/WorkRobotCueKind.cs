namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>What a signalling cue of the Work Robot's animations does: count the laser cooldown down, or play its footstep.</summary>
internal enum WorkRobotCueKind
{
    /// <summary>Counts the laser cooldown down.</summary>
    CountDownLaserCooldown,
    /// <summary>Plays the footstep sound when the Work Robot is on screen.</summary>
    Footstep,
}

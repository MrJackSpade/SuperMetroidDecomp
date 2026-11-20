namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>What a signalling cue of an enemy drop's animations does: delete the drop, or respawn the enemy it came from.</summary>
internal enum EnemyDeathAndDropCueKind
{
    /// <summary>Removes the pickup or the drop's respawn pause.</summary>
    Delete,
    /// <summary>Respawns the killed enemy when its room keeps it.</summary>
    RespawnEnemy,
}

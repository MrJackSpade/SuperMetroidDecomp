namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>What one of the Bomb Torizo hand's cues does (#1296).</summary>
internal enum BombTorizoHandCueKind
{
    /// <summary>Installs the wake-if-Samus-has-Bombs pre-instruction.</summary>
    ArmBombWake,
    /// <summary>Copies the Chozo debris tiles to VRAM.</summary>
    CopyDebrisTiles,
    /// <summary>Spawns one statue-breaking fragment.</summary>
    SpawnFragment,
    /// <summary>Queues the boss music.</summary>
    QueueMusic,
}

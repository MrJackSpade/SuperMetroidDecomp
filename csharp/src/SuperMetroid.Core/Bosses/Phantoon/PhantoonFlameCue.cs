namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The gameplay cues Phantoon's flame programs publish.</summary>
internal enum PhantoonFlameCue
{
    /// <summary>Requests the shot flame's drop (<c>Instruction_EnemyProjectile_SpawnPhantoonDrop</c>).</summary>
    SpawnDrop,
    /// <summary>Deletes the flame (<c>Instruction_EnemyProjectile_Delete</c>).</summary>
    Delete,
}
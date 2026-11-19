using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// Spore Spawn asked for its ceiling's room object (#2562): its setup in a defeated room asks for the clear ceiling, and
/// its live death the animated crumble, on the exact collision update.
/// </summary>
/// <param name="Mutation">The ceiling's change.</param>
public sealed record SporeSpawnCeilingChange(SporeSpawnCeilingMutation Mutation) : EnemyPassEvent;

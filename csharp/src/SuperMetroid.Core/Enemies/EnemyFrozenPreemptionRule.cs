namespace SuperMetroid.Core.Enemies;

/// <summary>A frozen-state preemption and the enemies it applies to.</summary>
/// <param name="Matches">The matches.</param>
/// <param name="Preemption">The preemption.</param>
internal readonly record struct EnemyFrozenPreemptionRule(Func<RoomEnemySlot, bool> Matches, EnemyFrozenPreemption Preemption);

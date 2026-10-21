namespace SuperMetroid.Core.Enemies;

/// <summary>A hurt AI handler and the enemies it applies to.</summary>
/// <param name="Matches">The matches.</param>
/// <param name="Handler">The handler.</param>
internal readonly record struct EnemyHurtRule(Func<RoomEnemySlot, bool> Matches, EnemyHurtAiHandler Handler);

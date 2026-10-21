namespace SuperMetroid.Core.Enemies;

/// <summary>A frozen handler and the enemies it applies to.</summary>
/// <param name="Matches">The matches.</param>
/// <param name="Handler">The handler.</param>
internal readonly record struct EnemyFrozenRule(Func<RoomEnemySlot, bool> Matches, Action<RoomEnemySlot> Handler);

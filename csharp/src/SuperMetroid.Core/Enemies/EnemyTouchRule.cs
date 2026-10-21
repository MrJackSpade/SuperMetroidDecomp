namespace SuperMetroid.Core.Enemies;

/// <summary>A touch handler and the enemies it applies to.</summary>
/// <param name="Matches">The matches.</param>
/// <param name="Handler">The handler.</param>
internal readonly record struct EnemyTouchRule(Func<RoomEnemySlot, bool> Matches, EnemyTouchHandler Handler);

namespace SuperMetroid.Core.Enemies;

/// <summary>A frozen enemy's private tail, run before the common thaw; true when it consumed the slot's whole frame.</summary>
internal delegate bool EnemyFrozenPreemption(RoomEnemySlot slot);

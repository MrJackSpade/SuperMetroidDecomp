namespace SuperMetroid.Core.Enemies;

/// <summary>One hurt AI, run instead of main AI while the slot's hurt flag is set.</summary>
internal delegate void EnemyHurtAiHandler(RoomEnemySlot slot, RoomEnemyFrame frame);

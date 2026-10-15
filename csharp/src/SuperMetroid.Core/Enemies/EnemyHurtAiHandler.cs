namespace SuperMetroid.Core.Enemies;

/// <summary>One translated hurt AI, run instead of main AI while the slot's hurt bit (AI handler bit 1) is set.</summary>
internal delegate void EnemyHurtAiHandler(RoomEnemySlot slot, RoomEnemyFrame frame);

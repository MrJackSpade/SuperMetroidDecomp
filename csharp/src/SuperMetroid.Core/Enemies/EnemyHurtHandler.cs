namespace SuperMetroid.Core.Enemies;

/// <summary>One enemy's behaviour while it is hurt, which replaces its main AI for the frame.</summary>
internal delegate void EnemyHurtHandler(RoomEnemySlot slot, RoomEnemyFrame frame);

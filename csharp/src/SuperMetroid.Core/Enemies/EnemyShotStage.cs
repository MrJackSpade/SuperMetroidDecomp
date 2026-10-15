namespace SuperMetroid.Core.Enemies;

/// <summary>A species stage of the shot pass; returns true when it consumed the hit (the enemy's pass ends).</summary>
internal delegate bool EnemyShotStage(in EnemyShotContext context);

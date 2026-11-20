namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy stopped the room's liquid surface in the pass (#2571); the next frame shows it still.</summary>
public sealed record LiquidSurfaceStilled : EnemyPassEvent;

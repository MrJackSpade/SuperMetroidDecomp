namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>One immutable Oum rectangle with its native touch and shot callbacks.</summary>
internal readonly record struct MaridiaLargeSnailCollisionHitbox(
    int Left, int Top, int Right, int Bottom, EnemyInteractionCallback TouchAi, EnemyInteractionCallback ShotAi);

namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>One immutable Oum rectangle with its native touch and shot callbacks.</summary>
internal readonly record struct MaridiaLargeSnailCollisionHitbox(
    short Left, short Top, short Right, short Bottom, EnemyInteractionCallback TouchAi, EnemyInteractionCallback ShotAi);

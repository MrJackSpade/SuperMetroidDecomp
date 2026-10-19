namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy that may react to Samus touching it before the common contact path.</summary>
internal interface IEnemyTouchReaction : IEnemyEntity
{
    /// <summary>The touch handler the enemy uses now, or null for the common path.</summary>
    EnemyTouchHandler? FindTouch();
}

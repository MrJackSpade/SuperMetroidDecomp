namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy whose animation may be a typed program the shared enemy animation yields to.</summary>
internal interface IEnemyTypedAnimation : IEnemyEntity
{
    /// <summary>The typed animation step the enemy uses now, or null when its animation is not typed.</summary>
    Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation();
}

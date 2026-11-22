namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>What the owner of a <see cref="EnemyProjectileFrameStep"/> does on each of its cases (#2553).</summary>
/// <param name="Show">Reacts to a <see cref="EnemyProjectileFrameStep.Show"/>.</param>
/// <param name="ShowForGood">Reacts to a <see cref="EnemyProjectileFrameStep.ShowForGood"/>.</param>
/// <param name="Hide">Reacts to a <see cref="EnemyProjectileFrameStep.Hide"/>.</param>
/// <param name="Rest">Reacts to a <see cref="EnemyProjectileFrameStep.Rest"/>.</param>
/// <param name="Delete">Reacts to a <see cref="EnemyProjectileFrameStep.Delete"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed partial record EnemyProjectileFrameStepReactions<TContext>(
    Action<EnemyProjectileFrameStep.Show, TContext> Show,
    Action<EnemyProjectileFrameStep.ShowForGood, TContext> ShowForGood,
    Action<EnemyProjectileFrameStep.Hide, TContext> Hide,
    Action<EnemyProjectileFrameStep.Rest, TContext> Rest,
    Action<EnemyProjectileFrameStep.Delete, TContext> Delete);

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>What the owner of a <see cref="EnemyProjectileCommonCue"/> does on each of its cases (#2553).</summary>
/// <param name="Scatter">Reacts to a <see cref="EnemyProjectileCommonCue.Scatter"/>.</param>
/// <param name="Signal">Reacts to a <see cref="EnemyProjectileCommonCue.Signal"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed record EnemyProjectileCommonCueReactions<TContext>(Action<EnemyProjectileCommonCue.Scatter, TContext> Scatter, Action<EnemyProjectileCommonCue.Signal, TContext> Signal);

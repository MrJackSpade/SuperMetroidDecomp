namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>What the owner of a <see cref="DownwardGateProjectileCue"/> does on each of its cases (#2553).</summary>
/// <param name="SetYVelocity">Reacts to a <see cref="DownwardGateProjectileCue.SetYVelocity"/>.</param>
/// <param name="Signal">Reacts to a <see cref="DownwardGateProjectileCue.Signal"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed partial record DownwardGateProjectileCueReactions<TContext>(Action<DownwardGateProjectileCue.SetYVelocity, TContext> SetYVelocity, Action<DownwardGateProjectileCue.Signal, TContext> Signal);

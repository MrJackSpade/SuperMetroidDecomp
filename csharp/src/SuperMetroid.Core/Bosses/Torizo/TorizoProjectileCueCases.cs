namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>What each case of a <see cref="TorizoProjectileCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the torizo projectile cue is made into.</typeparam>
/// <param name="PlaySound">Makes a <see cref="TorizoProjectileCue.PlaySound"/> into the result.</param>
/// <param name="Scatter">Makes a <see cref="TorizoProjectileCue.Scatter"/> into the result.</param>
/// <param name="SetRadius">Makes a <see cref="TorizoProjectileCue.SetRadius"/> into the result.</param>
/// <param name="Signal">Makes a <see cref="TorizoProjectileCue.Signal"/> into the result.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed record TorizoProjectileCueCases<TContext, TResult>(Func<TorizoProjectileCue.PlaySound, TContext, TResult> PlaySound, Func<TorizoProjectileCue.Scatter, TContext, TResult> Scatter, Func<TorizoProjectileCue.SetRadius, TContext, TResult> SetRadius, Func<TorizoProjectileCue.Signal, TContext, TResult> Signal);

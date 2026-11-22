namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>What each case of a <see cref="KiHunterCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the cue is made into.</typeparam>
/// <param name="Spit">Makes a <see cref="KiHunterCue.Spit"/> into the result.</param>
/// <param name="Signal">Makes a <see cref="KiHunterCue.Signal"/> into the result.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed record KiHunterCueCases<TContext, TResult>(Func<KiHunterCue.Spit, TContext, TResult> Spit, Func<KiHunterCue.Signal, TContext, TResult> Signal);

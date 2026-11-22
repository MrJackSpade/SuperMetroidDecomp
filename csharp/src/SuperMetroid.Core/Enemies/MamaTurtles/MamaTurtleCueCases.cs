namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>What each case of a <see cref="MamaTurtleCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the cue is made into.</typeparam>
/// <param name="RiseToHover">Makes a <see cref="MamaTurtleCue.RiseToHover"/> into the result.</param>
/// <param name="Signal">Makes a <see cref="MamaTurtleCue.Signal"/> into the result.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed record MamaTurtleCueCases<TContext, TResult>(Func<MamaTurtleCue.RiseToHover, TContext, TResult> RiseToHover, Func<MamaTurtleCue.Signal, TContext, TResult> Signal);

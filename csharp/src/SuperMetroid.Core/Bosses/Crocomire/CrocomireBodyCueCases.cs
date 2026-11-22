namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>What each case of a <see cref="CrocomireBodyCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the crocomire body cue is made into.</typeparam>
/// <param name="Step">Makes a <see cref="CrocomireBodyCue.Step"/> into the result.</param>
/// <param name="Dust">Makes a <see cref="CrocomireBodyCue.Dust"/> into the result.</param>
/// <param name="Signal">Makes a <see cref="CrocomireBodyCue.Signal"/> into the result.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed partial record CrocomireBodyCueCases<TContext, TResult>(Func<CrocomireBodyCue.Step, TContext, TResult> Step, Func<CrocomireBodyCue.Dust, TContext, TResult> Dust, Func<CrocomireBodyCue.Signal, TContext, TResult> Signal);

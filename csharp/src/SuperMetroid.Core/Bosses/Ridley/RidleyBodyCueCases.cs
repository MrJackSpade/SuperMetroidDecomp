namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>What each case of a <see cref="RidleyBodyCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the ridley body cue is made into.</typeparam>
/// <param name="Move">Makes a <see cref="RidleyBodyCue.Move"/> into the result.</param>
/// <param name="SetClawHeight">Makes a <see cref="RidleyBodyCue.SetClawHeight"/> into the result.</param>
/// <param name="Flip">Makes a <see cref="RidleyBodyCue.Flip"/> into the result.</param>
/// <param name="Signal">Makes a <see cref="RidleyBodyCue.Signal"/> into the result.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed record RidleyBodyCueCases<TContext, TResult>(Func<RidleyBodyCue.Move, TContext, TResult> Move, Func<RidleyBodyCue.SetClawHeight, TContext, TResult> SetClawHeight, Func<RidleyBodyCue.Flip, TContext, TResult> Flip, Func<RidleyBodyCue.Signal, TContext, TResult> Signal);

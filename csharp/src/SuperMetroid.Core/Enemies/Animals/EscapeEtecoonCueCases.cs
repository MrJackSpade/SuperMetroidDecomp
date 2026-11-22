namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>What each case of a <see cref="EscapeEtecoonCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the escape etecoon cue is made into.</typeparam>
/// <param name="LowTideRunStarts">Makes a <see cref="EscapeEtecoonCue.LowTideRunStarts"/> into the result.</param>
/// <param name="Move">Makes a <see cref="EscapeEtecoonCue.Move"/> into the result.</param>
/// <param name="Signal">Makes a <see cref="EscapeEtecoonCue.Signal"/> into the result.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed record EscapeEtecoonCueCases<TContext, TResult>(Func<EscapeEtecoonCue.LowTideRunStarts, TContext, TResult> LowTideRunStarts, Func<EscapeEtecoonCue.Move, TContext, TResult> Move, Func<EscapeEtecoonCue.Signal, TContext, TResult> Signal);

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>What each case of a <see cref="EscapeDachoraCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the escape dachora cue is made into.</typeparam>
/// <param name="LowTideLapEnds">Makes a <see cref="EscapeDachoraCue.LowTideLapEnds"/> into the result.</param>
/// <param name="LapEnds">Makes a <see cref="EscapeDachoraCue.LapEnds"/> into the result.</param>
/// <param name="Move">Makes a <see cref="EscapeDachoraCue.Move"/> into the result.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed record EscapeDachoraCueCases<TContext, TResult>(Func<EscapeDachoraCue.LowTideLapEnds, TContext, TResult> LowTideLapEnds, Func<EscapeDachoraCue.LapEnds, TContext, TResult> LapEnds, Func<EscapeDachoraCue.Move, TContext, TResult> Move);

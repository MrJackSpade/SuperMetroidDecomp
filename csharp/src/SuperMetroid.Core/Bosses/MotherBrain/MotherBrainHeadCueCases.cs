namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What each case of a <see cref="MotherBrainHeadCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the cue is made into.</typeparam>
/// <param name="PlaySound">Makes a <see cref="MotherBrainHeadCue.PlaySound"/> into the result.</param>
/// <param name="SpawnBomb">Makes a <see cref="MotherBrainHeadCue.SpawnBomb"/> into the result.</param>
/// <param name="Signal">Makes a <see cref="MotherBrainHeadCue.Signal"/> into the result.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed record MotherBrainHeadCueCases<TContext, TResult>(Func<MotherBrainHeadCue.PlaySound, TContext, TResult> PlaySound, Func<MotherBrainHeadCue.SpawnBomb, TContext, TResult> SpawnBomb, Func<MotherBrainHeadCue.Signal, TContext, TResult> Signal);

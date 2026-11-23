namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>What each case of a <see cref="AlcoonCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the cue is made into.</typeparam>
/// <param name="Fire">Makes a <see cref="AlcoonCue.Fire"/> into the result.</param>
/// <param name="Signal">Makes a <see cref="AlcoonCue.Signal"/> into the result.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed partial record AlcoonCueCases<TContext, TResult>(Func<AlcoonCue.Fire, TContext, TResult> Fire, Func<AlcoonCue.Signal, TContext, TResult> Signal);

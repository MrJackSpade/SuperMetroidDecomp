namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>What each case of a <see cref="YardCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the cue is made into.</typeparam>
/// <param name="MoveBy">Makes a <see cref="YardCue.MoveBy"/> into the result.</param>
/// <param name="SetDirection">Makes a <see cref="YardCue.SetDirection"/> into the result.</param>
/// <param name="SetHidingAnimation">Makes a <see cref="YardCue.SetHidingAnimation"/> into the result.</param>
/// <param name="BeginCrawling">Makes a <see cref="YardCue.BeginCrawling"/> into the result.</param>
/// <param name="BeginFlight">Makes a <see cref="YardCue.BeginFlight"/> into the result.</param>
/// <param name="Signal">Makes a <see cref="YardCue.Signal"/> into the result.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed partial record YardCueCases<TContext, TResult>(Func<YardCue.MoveBy, TContext, TResult> MoveBy, Func<YardCue.SetDirection, TContext, TResult> SetDirection, Func<YardCue.SetHidingAnimation, TContext, TResult> SetHidingAnimation, Func<YardCue.BeginCrawling, TContext, TResult> BeginCrawling, Func<YardCue.BeginFlight, TContext, TResult> BeginFlight, Func<YardCue.Signal, TContext, TResult> Signal);

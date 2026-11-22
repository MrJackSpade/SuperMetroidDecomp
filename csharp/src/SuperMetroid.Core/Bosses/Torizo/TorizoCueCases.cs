namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>What each case of a <see cref="TorizoCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the torizo cue is made into.</typeparam>
/// <param name="Branch">Makes a <see cref="TorizoCue.Branch"/> into the result.</param>
/// <param name="Interrupt">Makes a <see cref="TorizoCue.Interrupt"/> into the result.</param>
/// <param name="Choice">Makes a <see cref="TorizoCue.Choice"/> into the result.</param>
/// <param name="LandIn">Makes a <see cref="TorizoCue.LandIn"/> into the result.</param>
/// <param name="WalkOn">Makes a <see cref="TorizoCue.WalkOn"/> into the result.</param>
/// <param name="Turn">Makes a <see cref="TorizoCue.Turn"/> into the result.</param>
/// <param name="Posture">Makes a <see cref="TorizoCue.Posture"/> into the result.</param>
/// <param name="Walk">Makes a <see cref="TorizoCue.Walk"/> into the result.</param>
/// <param name="Swipe">Makes a <see cref="TorizoCue.Swipe"/> into the result.</param>
/// <param name="LowHealthExplosions">Makes a <see cref="TorizoCue.LowHealthExplosions"/> into the result.</param>
/// <param name="SetPhase">Makes a <see cref="TorizoCue.SetPhase"/> into the result.</param>
/// <param name="ShowArtwork">Makes a <see cref="TorizoCue.ShowArtwork"/> into the result.</param>
/// <param name="Stepped">Makes a <see cref="TorizoCue.Stepped"/> into the result.</param>
/// <param name="Signal">Makes a <see cref="TorizoCue.Signal"/> into the result.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed record TorizoCueCases<TContext, TResult>(Func<TorizoCue.Branch, TContext, TResult> Branch, Func<TorizoCue.Interrupt, TContext, TResult> Interrupt, Func<TorizoCue.Choice, TContext, TResult> Choice, Func<TorizoCue.LandIn, TContext, TResult> LandIn, Func<TorizoCue.WalkOn, TContext, TResult> WalkOn, Func<TorizoCue.Turn, TContext, TResult> Turn, Func<TorizoCue.Posture, TContext, TResult> Posture, Func<TorizoCue.Walk, TContext, TResult> Walk, Func<TorizoCue.Swipe, TContext, TResult> Swipe, Func<TorizoCue.LowHealthExplosions, TContext, TResult> LowHealthExplosions, Func<TorizoCue.SetPhase, TContext, TResult> SetPhase, Func<TorizoCue.ShowArtwork, TContext, TResult> ShowArtwork, Func<TorizoCue.Stepped, TContext, TResult> Stepped, Func<TorizoCue.Signal, TContext, TResult> Signal);

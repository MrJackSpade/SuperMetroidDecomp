namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>What the owner of a <see cref="BotwoonHeadCue"/> does on each of its cases (#2553).</summary>
/// <param name="ApplyFacingRadius">Reacts to a <see cref="BotwoonHeadCue.ApplyFacingRadius"/>.</param>
/// <param name="Signal">Reacts to a <see cref="BotwoonHeadCue.Signal"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed partial record BotwoonHeadCueReactions<TContext>(Action<BotwoonHeadCue.ApplyFacingRadius, TContext> ApplyFacingRadius, Action<BotwoonHeadCue.Signal, TContext> Signal);

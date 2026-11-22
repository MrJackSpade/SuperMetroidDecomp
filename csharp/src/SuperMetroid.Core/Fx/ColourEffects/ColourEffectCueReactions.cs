namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>What the owner of a <see cref="ColourEffectCue"/> does on each of its cases (#2553).</summary>
/// <param name="QueueSound">Reacts to a <see cref="ColourEffectCue.QueueSound"/>.</param>
/// <param name="PublishHeatPhase">Reacts to a <see cref="ColourEffectCue.PublishHeatPhase"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
public sealed record ColourEffectCueReactions<TContext>(Action<ColourEffectCue.QueueSound, TContext> QueueSound, Action<ColourEffectCue.PublishHeatPhase, TContext> PublishHeatPhase);

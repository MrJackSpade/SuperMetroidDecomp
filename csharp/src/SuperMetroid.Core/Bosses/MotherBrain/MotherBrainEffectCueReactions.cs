namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What the owner of a <see cref="MotherBrainEffectCue"/> does on each of its cases (#2553).</summary>
/// <param name="SetRadius">Reacts to a <see cref="MotherBrainEffectCue.SetRadius"/>.</param>
/// <param name="Signal">Reacts to a <see cref="MotherBrainEffectCue.Signal"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed record MotherBrainEffectCueReactions<TContext>(Action<MotherBrainEffectCue.SetRadius, TContext> SetRadius, Action<MotherBrainEffectCue.Signal, TContext> Signal);

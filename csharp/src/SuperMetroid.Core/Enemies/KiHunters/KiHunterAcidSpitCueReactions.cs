namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>What the owner of a <see cref="KiHunterAcidSpitCue"/> does on each of its cases (#2553).</summary>
/// <param name="Launch">Reacts to a <see cref="KiHunterAcidSpitCue.Launch"/>.</param>
/// <param name="Signal">Reacts to a <see cref="KiHunterAcidSpitCue.Signal"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed record KiHunterAcidSpitCueReactions<TContext>(Action<KiHunterAcidSpitCue.Launch, TContext> Launch, Action<KiHunterAcidSpitCue.Signal, TContext> Signal);

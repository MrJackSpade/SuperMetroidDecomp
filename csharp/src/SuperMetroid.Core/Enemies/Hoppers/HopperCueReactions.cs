namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>What the owner of a <see cref="HopperCue"/> does on each of its cases (#2553).</summary>
/// <param name="PlaySound">Reacts to a <see cref="HopperCue.PlaySound"/>.</param>
/// <param name="Signal">Reacts to a <see cref="HopperCue.Signal"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed record HopperCueReactions<TContext>(Action<HopperCue.PlaySound, TContext> PlaySound, Action<HopperCue.Signal, TContext> Signal);

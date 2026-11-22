namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>What the owner of a <see cref="FirefleaFlightState"/> does on each of its cases (#2553).</summary>
/// <param name="BobbingFirefleaState">Reacts to a <see cref="BobbingFirefleaState"/>.</param>
/// <param name="CirclingFirefleaState">Reacts to a <see cref="CirclingFirefleaState"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
public sealed record FirefleaFlightStateReactions<TContext>(Action<BobbingFirefleaState, TContext> BobbingFirefleaState, Action<CirclingFirefleaState, TContext> CirclingFirefleaState);

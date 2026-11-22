namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>What the owner of a <see cref="GerutaCue"/> does on each of its cases (#2553).</summary>
/// <param name="PlaceFlames">Reacts to a <see cref="GerutaCue.PlaceFlames"/>.</param>
/// <param name="SwoopStartFinished">Reacts to a <see cref="GerutaCue.SwoopStartFinished"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed partial record GerutaCueReactions<TContext>(Action<GerutaCue.PlaceFlames, TContext> PlaceFlames, Action<GerutaCue.SwoopStartFinished, TContext> SwoopStartFinished);

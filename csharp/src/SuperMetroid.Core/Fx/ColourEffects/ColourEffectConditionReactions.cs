namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>What the owner of a <see cref="ColourEffectCondition"/> does on each of its cases (#2553).</summary>
/// <param name="WatchesNothing">Reacts to a <see cref="ColourEffectCondition.WatchesNothing"/>.</param>
/// <param name="RestartsWhileSamusIsHigh">Reacts to a <see cref="ColourEffectCondition.RestartsWhileSamusIsHigh"/>.</param>
/// <param name="EndsWhenTorizoDies">Reacts to a <see cref="ColourEffectCondition.EndsWhenTorizoDies"/>.</param>
/// <param name="EndsWhenAreaMiniBossDies">Reacts to a <see cref="ColourEffectCondition.EndsWhenAreaMiniBossDies"/>.</param>
/// <param name="BurnsSamusInHeat">Reacts to a <see cref="ColourEffectCondition.BurnsSamusInHeat"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
public sealed record ColourEffectConditionReactions<TContext>(Action<ColourEffectCondition.WatchesNothing, TContext> WatchesNothing, Action<ColourEffectCondition.RestartsWhileSamusIsHigh, TContext> RestartsWhileSamusIsHigh, Action<ColourEffectCondition.EndsWhenTorizoDies, TContext> EndsWhenTorizoDies, Action<ColourEffectCondition.EndsWhenAreaMiniBossDies, TContext> EndsWhenAreaMiniBossDies, Action<ColourEffectCondition.BurnsSamusInHeat, TContext> BurnsSamusInHeat);

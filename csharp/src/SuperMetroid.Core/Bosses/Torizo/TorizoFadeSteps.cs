namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The steps a Torizo's colour fade has taken (#1512): the gradual colour change counter's values at the first and the
/// latest step it took from the colouring his body last showed whole.
/// </summary>
/// <param name="First">The counter value of the fade's first step.</param>
/// <param name="Last">The counter value of the fade's latest step.</param>
internal readonly record struct TorizoFadeSteps(int First, int Last);

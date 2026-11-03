namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// The fade running on Phantoon's body (#1512): the colouring it fades toward, the number of steps it takes, and the
/// values of the first and last steps it took from the colouring his body last showed whole.
/// </summary>
/// <param name="To">The colouring the fade runs toward.</param>
/// <param name="Denominator">The number of steps the fade takes to reach <paramref name="To"/>.</param>
/// <param name="First">The value of the first step the fade took from the colouring shown.</param>
/// <param name="Last">The value of the latest step the fade took.</param>
internal readonly record struct PhantoonBodyFade(PhantoonBodyLook To, int Denominator, int First, int Last);

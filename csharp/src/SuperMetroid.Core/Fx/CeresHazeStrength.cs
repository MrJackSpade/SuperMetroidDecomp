namespace SuperMetroid.Core.Fx;

/// <summary>
/// How strongly the Ceres haze shows (#2482), in steps of its fade: 0 its faintest and
/// <see cref="CeresHazeDefinitions.FadeSteps"/> less one, full, as it fades in, and on the fade-out's first update one step past
/// full before it falls back to 1.
/// </summary>
/// <param name="FadeSteps">The steps of the fade the haze shows.</param>
public readonly record struct CeresHazeStrength(int FadeSteps);

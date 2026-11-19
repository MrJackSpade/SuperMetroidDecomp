namespace SuperMetroid.Core.Fx;

/// <summary>
/// How strongly the Ceres haze shows (#2482), in steps of its fade: 0 its faintest and
/// <see cref="CeresHazeDefinitions.FadeSteps"/> less one, full (#2573).
/// </summary>
/// <param name="FadeSteps">The steps of the fade the haze shows.</param>
public readonly record struct CeresHazeStrength(int FadeSteps);

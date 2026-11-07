using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>The Ceres haze's two tints: blue while Ridley lives, red once he is dead.</summary>
/// <param name="WhileRidleyLives">The tint while Ceres Ridley is alive.</param>
/// <param name="AfterRidleyDies">The tint once Ceres Ridley is dead.</param>
public sealed record CeresHazeTints(RgbColor WhileRidleyLives, RgbColor AfterRidleyDies)
{
    /// <summary>The stock tints, at the final amplitude the haze fades in to.</summary>
    internal static CeresHazeTints Stock { get; } = new(
        new RgbColor(0, 0, CeresHazeDefinitions.FadeSteps - 1),
        new RgbColor(CeresHazeDefinitions.FadeSteps - 1, 0, 0));

    /// <summary>The tint the haze shows for Ridley's state.</summary>
    internal RgbColor For(bool ridleyIsDead) => ridleyIsDead ? AfterRidleyDies : WhileRidleyLives;
}

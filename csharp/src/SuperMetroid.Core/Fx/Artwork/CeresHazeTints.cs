using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// The Ceres haze's two installed tints: blue while Ridley lives, red once he is dead. The scene colours the player edits in
/// place are their only source; Core holds no compiled copy (#1596).
/// </summary>
/// <param name="WhileRidleyLives">The tint while Ceres Ridley is alive.</param>
/// <param name="AfterRidleyDies">The tint once Ceres Ridley is dead.</param>
public sealed record CeresHazeTints(RgbColor WhileRidleyLives, RgbColor AfterRidleyDies)
{
    /// <summary>The tint the haze shows for Ridley's state.</summary>
    internal RgbColor For(bool ridleyIsDead) => ridleyIsDead ? AfterRidleyDies : WhileRidleyLives;
}

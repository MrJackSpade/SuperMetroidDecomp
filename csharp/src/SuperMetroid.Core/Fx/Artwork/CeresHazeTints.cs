using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// The Ceres haze's two installed tints: blue while Ridley lives, red once he is dead. The scene colours the player edits in
/// place are their only source; Core holds no copy of its own (#1596).
/// </summary>
/// <param name="WhileRidleyLives">The tint while Ceres Ridley is alive.</param>
/// <param name="AfterRidleyDies">The tint once Ceres Ridley is dead.</param>
public sealed partial record CeresHazeTints(RgbColor WhileRidleyLives, RgbColor AfterRidleyDies)
{
    /// <summary>The installed colour of <paramref name="tint"/>.</summary>
    /// <exception cref="InvalidOperationException">The tint is undefined.</exception>
    internal RgbColor For(CeresHazeTint tint) => tint switch
    {
        CeresHazeTint.WhileRidleyLives => WhileRidleyLives,
        CeresHazeTint.AfterRidleyDies => AfterRidleyDies,
        _ => throw new InvalidOperationException($"Undefined {nameof(CeresHazeTint)} {tint}."),
    };
}

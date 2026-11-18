using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Value-only render input for the Ceres haze: which tint it adds and how strongly it shows. The display capture
/// describes it as a colour strengthening in bands down the screen (#1468, #2482).
/// </summary>
/// <param name="Tint">The tint the haze adds.</param>
/// <param name="Strength">How strongly the haze shows.</param>
public readonly record struct CeresHazeRenderSnapshot(CeresHazeTint Tint, CeresHazeStrength Strength)
{
    /// <summary>Copies the room's haze, or null when the room has none.</summary>
    public static CeresHazeRenderSnapshot? Capture(CeresHazeState? haze) =>
        haze is { } shown ? new(shown.Tint, shown.Strength) : null;
}

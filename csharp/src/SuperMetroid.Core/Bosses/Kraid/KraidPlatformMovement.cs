namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Movement data used by Kraid's belly-platform firing routine.</summary>
internal static class KraidPlatformMovement
{
    /// <summary>
    /// Negative 16.16 displacement from the lint's horizontal speed,
    /// subtracted by the lint's firing function and the riding branch.
    /// </summary>
    public const int FiringDisplacement = -229376;

    /// <summary>
    /// Minimum whole-pixel extra Samus X displacement. The game
    /// clamps the whole word to -16 without changing the fractional word.
    /// </summary>
    public const int MinimumCarryWholePixels = -16;
}

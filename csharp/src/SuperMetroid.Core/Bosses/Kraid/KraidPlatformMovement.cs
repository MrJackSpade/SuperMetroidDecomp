namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Movement data used by Kraid's belly-platform firing.</summary>
internal static class KraidPlatformMovement
{
    /// <summary>
    /// The lint's horizontal displacement each firing update, three and a half pixels leftward, which also carries Samus
    /// while she rides it.
    /// </summary>
    public static SignedSixteenSixteen FiringDisplacement { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(-56);

    /// <summary>
    /// Minimum whole-pixel extra Samus X displacement. The game
    /// clamps the whole pixels to -16 without changing the fraction of a pixel.
    /// </summary>
    public const int MinimumCarryWholePixels = -16;
}

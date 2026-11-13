namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Movement data used by Kraid's belly-platform firing routine.</summary>
internal static class KraidPlatformMovement
{
    /// <summary>
    /// The lint's horizontal displacement each firing update, three and a half pixels leftward, which the riding
    /// branch also carries Samus by.
    /// </summary>
    public static SignedSixteenSixteen FiringDisplacement { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(-56);

    /// <summary>
    /// Minimum whole-pixel extra Samus X displacement. The game
    /// clamps the whole pixels to -16 without changing the subpixels.
    /// </summary>
    public const int MinimumCarryWholePixels = -16;
}

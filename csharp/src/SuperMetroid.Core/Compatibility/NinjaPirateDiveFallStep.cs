namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How far a Ninja Space Pirate's divekick falls in one update (#1607): its speed's whole pixels, and its speed's 256ths
/// of a pixel counted only as that many 65536ths. The fall's fraction therefore builds up 256 times slower than its speed
/// says, so the pirate's position below the whole pixel, and the update on which the fraction overflows into a whole pixel, follow the
/// game's rather than the speed's true distance.
/// </summary>
/// <remarks>
/// Consulted once per update of a divekick's descent, before the vertical move that tests for the floor. Evidence: the
/// original descent hands the vertical mover the speed's whole byte as the step's whole pixels and the speed's fraction
/// byte as the low byte of the step's fraction, leaving the fraction's upper byte clear.
/// </remarks>
[CompatibilityService("a Ninja Space Pirate's divekick falls each update by its speed's whole pixels and its speed's 256ths counted as 65536ths of a pixel")]
public static class NinjaPirateDiveFallStep
{
    /// <summary>The fall of one descending update at <paramref name="speed"/>.</summary>
    public static SignedSixteenSixteen Of(SignedEightEight speed) =>
        SignedSixteenSixteen.FromParts(speed.UnsignedWholePixels, speed.FractionSubpixels);
}

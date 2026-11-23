namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What a quicksand surface leaves of Samus's horizontal base speed (#2116): her run stops, her whole pixels of base speed
/// are taken away, and of the fraction only the part below half a pixel is kept, so a Samus that entered the surface at a
/// speed whose fraction passed half a pixel keeps moving by the rest, and her position's fraction goes on gathering it.
/// </summary>
/// <remarks>
/// Consulted each update Samus's body overlaps a quicksand surface block, at each of the bottom, centre and top samples of
/// her inside-block reactions, before the surface limits her vertical speed. A rule with no state of its own. Evidence:
/// the original surface reaction clears the base speed's whole word and masks its fraction word with the half-pixel
/// bit cleared, rather than clearing both words.
/// </remarks>
internal static partial class QuicksandSurfaceSpeedLeftover
{
    /// <summary>Half a pixel in fraction steps: the surface keeps only the part of the fraction below it.</summary>
    private const int HalfPixelFraction = FixedCoordinate.HalfPixelFraction;

    /// <summary>The base speed a quicksand surface leaves of <paramref name="baseSpeed"/>: no whole pixels, and the part of its fraction below half a pixel.</summary>
    /// <param name="baseSpeed">Samus's horizontal base speed as she meets the surface.</param>
    internal static UnsignedSixteenSixteen Leftover(UnsignedSixteenSixteen baseSpeed) =>
        new(0, baseSpeed.Fraction % HalfPixelFraction);
}

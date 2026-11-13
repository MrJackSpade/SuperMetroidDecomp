namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How the enemies' short circle product places a point at a radius along a turn (#1607): the radius counts only its
/// place within 256, so a radius of 256 or more continues from zero, and a point in the turn's second half is reversed by
/// negating its whole pixels and its fraction each on their own, so a reversed point with a fraction lies one pixel further
/// along the unreversed direction than a true reversal. Fireflea circles, Ki-Hunter swoops, Nuclear Waffle orbits and Evir
/// spit therefore move along exactly the game's points. Phantoon's circling flames keep only the radius rule: their
/// radius grows past 255 as a plain count (#1627), and their own product negates its whole result.
/// </summary>
/// <remarks>
/// Consulted whenever an enemy reads a sine, cosine or negative-sine product by a short radius during its update.
/// Evidence: the original short multiplication hands the multiply unit only the radius's low eight bits and negates the
/// product's two halves separately with no borrow between them; the compiled enemy-sine check sweeps every angle and radius
/// with poisoned upper bits against the pinned cartridge's sine table and requires both behaviours.
/// </remarks>
[CompatibilityService("an enemy's short circle product: a radius of 256 or more continues from zero, and a point in the turn's second half reverses its whole pixels and its fraction each on their own")]
public static class ShortCircleProduct
{
    /// <summary>The span of radii the product tells apart: a radius past it continues from zero.</summary>
    public const int RadiusSpan = 256;

    /// <summary>
    /// The radius, 0 through 255, the product uses for <paramref name="radius"/> pixels, zero or more: its place within
    /// <see cref="RadiusSpan"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="radius"/> is negative.</exception>
    public static int RadiusInUse(int radius) => Ensure.AtLeastZero(radius) % RadiusSpan;

    /// <summary>
    /// <paramref name="point"/> reversed for the turn's second half: its whole pixels and its fraction negated each on
    /// their own, the fraction's borrow never reaching the whole pixels.
    /// </summary>
    public static SignedSixteenSixteen Reversed(SignedSixteenSixteen point) => point.NegatedPartsSeparately();
}

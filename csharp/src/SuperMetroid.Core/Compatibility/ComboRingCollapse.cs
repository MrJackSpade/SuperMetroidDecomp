namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// A charged combo ring's radius holds 0 through 255 pixels, and a ring that grows past 255 collapses onto Samus and grows
/// outward again from the radius that lies past it (#1795): an Ice ring flying outward eight pixels an update, or a Plasma
/// ring in its final expansion, that is still on screen when its radius passes 255 orbits Samus closely again instead of
/// leaving, so it can hit enemies near her a second time. A shrinking ring never passes below zero.
/// </summary>
/// <remarks>
/// Consulted on every update of the Ice ring's outward flight and of the Plasma ring's three phases, after the particle
/// is placed at its orbit and tested against the screen and before the next update reads the radius. A rule with no
/// state: each particle keeps its own radius. Evidence: the original combo routines keep the orbit radius as one byte of
/// the particle's state and add the growth to that byte alone; the combo ring check
/// (<c>VerifyComboRingCollapsesOntoSamus</c>) grows an on-screen Plasma ring past 255 through the production update and
/// finds it orbiting at the radius past the span.
/// </remarks>
[CompatibilityService("a combo ring grown past 255 pixels collapses onto Samus and grows outward again")]
public static class ComboRingCollapse
{
    /// <summary>The radii a combo ring tells apart: one grown past the largest continues from zero.</summary>
    public const int RadiusSpan = 256;

    /// <summary>The largest radius a combo ring holds, in pixels.</summary>
    public const int LargestRadius = RadiusSpan - 1;

    /// <summary>
    /// The ring's radius after growing <paramref name="radius"/> by <paramref name="step"/> pixels: the grown radius, or,
    /// past <see cref="LargestRadius"/>, the part of it past <see cref="RadiusSpan"/>.
    /// </summary>
    /// <param name="radius">The current radius, 0 through <see cref="LargestRadius"/>.</param>
    /// <param name="step">The signed growth of one update; a shrink must not pass below zero.</param>
    /// <exception cref="ArgumentOutOfRangeException">The radius is out of range, or the shrink passes below zero.</exception>
    public static int Grown(int radius, int step)
    {
        int grown = Ensure.BetweenInclusive(0, LargestRadius, radius) + step;
        return grown > LargestRadius ? grown - RadiusSpan : Ensure.AtLeastZero(grown);
    }
}

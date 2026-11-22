namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Where a shinespark crash's released echo is placed as it flies out (#2051): its radius grows by eight pixels on every
/// update, but the echo is placed at that radius taken within 256 pixels. An echo still on the screen when its radius
/// reaches 256 is placed back on Samus and flies out again, and while the first echo stays on the screen its height is
/// what the next crash's orbit counts on from (<see cref="CrashOrbitTravelCarryover"/>), so the return can shorten that
/// orbit and move the update on which Samus regains control. Where the echoes are drawn is not otherwise observable.
/// </summary>
/// <remarks>
/// Consulted on every update of a released echo, in the projectile pass, before the echo is tested against the screen.
/// Lifetime: none; the placement keeps no state of its own. Evidence: the original places each echo from the projectile
/// sine table with only the radius's low byte, so a radius of 256 reads as zero; a vertical crash's echo ending in a
/// tall room stays on the screen past 248 pixels.
/// </remarks>
[CompatibilityService("a shinespark crash's released echo is placed at its radius taken within 256 pixels, so an echo still on the screen at 256 pixels returns to Samus and its height feeds the next crash's orbit")]
public static partial class ShinesparkEchoReturn
{
    /// <summary>The radii an echo is placed at before it comes back to Samus.</summary>
    private const int PlacementRadii = 256;

    /// <summary>The radius, in pixels, an echo that has flown <paramref name="travelled"/> pixels out is placed at.</summary>
    /// <param name="travelled">How far the echo has flown out, zero or more.</param>
    public static int PlacementRadius(int travelled) => Ensure.AtLeastZero(travelled) % PlacementRadii;
}

using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// The aims an X-ray scope facing one way takes across its angles (#2557): each band's aim below its angle, in rising
/// order, and the aim at and past the last band.
/// </summary>
/// <param name="Below">Each band: the angle it ends below and the aim it shows, in rising angle order.</param>
/// <param name="AtAndPast">The aim at and past the last band's angle.</param>
internal sealed record XrayAimBands(IReadOnlyList<XrayAimBand> Below, SamusAnimationFrame AtAndPast)
{
    /// <summary>The aim a scope at <paramref name="angle"/> shows: the first band's it lies below, otherwise the last aim.</summary>
    internal SamusAnimationFrame AimAt(RotationAngle angle)
    {
        foreach (XrayAimBand band in Below)
        {
            if (angle < band.Below)
                return band.Aim;
        }
        return AtAndPast;
    }
}

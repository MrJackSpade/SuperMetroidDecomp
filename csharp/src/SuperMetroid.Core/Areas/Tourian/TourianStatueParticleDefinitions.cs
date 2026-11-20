namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// How the particles the Tourian statue scene throws off choose their direction, how they fall and splash, and where they
/// end (#1548).
/// </summary>
internal static class TourianStatueParticleDefinitions
{
    /// <summary>
    /// The random selection of a particle's launch angle, before it is centred on the zero angle: 64 angles, a quarter
    /// turn in all.
    /// </summary>
    internal static readonly RandomSampleField LaunchAngle = new(0, 6);

    /// <summary>
    /// The angle steps a selected launch angle is turned back by, half the selection's range, centring the launch on the zero
    /// angle.
    /// </summary>
    internal const int LaunchAngleCentring = 32;

    /// <summary>The pixels an update a particle is thrown across at its launch angle's sine's peak (#2482).</summary>
    internal const int LaunchAcrossPixels = 1;

    /// <summary>The pixels an update a particle is thrown up or down at its launch angle's negative cosine's peak (#2482).</summary>
    internal const int LaunchVerticalPixels = 4;

    /// <summary>The downward speed a falling particle gains each update: a sixteenth of a pixel per update.</summary>
    internal static readonly SignedEightEight Gravity = SignedEightEight.FromPixelFraction(1, 16);

    /// <summary>How many pixels above the water surface a particle's splash sits.</summary>
    internal const int SplashHeightAboveSurface = 4;

    /// <summary>
    /// The room line, the bottom of the statues' screen beneath the water, a falling particle is deleted at once it sinks
    /// to it (#1956).
    /// </summary>
    internal const int SunkDepth = 256;
}

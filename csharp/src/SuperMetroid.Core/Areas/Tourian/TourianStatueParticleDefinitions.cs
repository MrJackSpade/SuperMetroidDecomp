namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>How the particles the Tourian statue scene throws off choose their direction and where they end (#1548).</summary>
internal static class TourianStatueParticleDefinitions
{
    /// <summary>
    /// The random selection of a particle's launch angle, before it is centred on the zero angle: 64 angles, a quarter
    /// turn in all.
    /// </summary>
    internal static readonly RandomSampleField LaunchAngle = new(0, 6);

    /// <summary>
    /// The room line, the bottom of the statues' screen beneath the water, a falling particle is deleted at once it sinks
    /// to it (#1956).
    /// </summary>
    internal const int SunkDepth = 256;
}

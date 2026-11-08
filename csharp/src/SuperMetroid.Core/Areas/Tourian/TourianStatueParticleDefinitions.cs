namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>How the particles the Tourian statue scene throws off choose their direction (#1548).</summary>
internal static class TourianStatueParticleDefinitions
{
    /// <summary>
    /// The random selection of a particle's launch angle, before it is centred on the zero angle: 64 angles, a quarter
    /// turn in all.
    /// </summary>
    internal static readonly RandomSampleField LaunchAngle = new(0, 6);
}

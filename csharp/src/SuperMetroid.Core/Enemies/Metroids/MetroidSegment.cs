namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// The Metroid's chasing and draining pulse loops (#641).
/// Their steps are <see cref="MetroidAnimations"/>.
/// </summary>
internal enum MetroidSegment
{
    /// <summary>
    /// Its nuclei pulsing once from dark to brightest and back while it chases Samus; its animation pulses over (<see cref="MetroidAnimations.PulsesBetweenCries"/>)
    /// before each cry.
    /// </summary>
    ChasingSamus,
    /// <summary>The chasing Metroid's cry, chosen at random, after its pulses.</summary>
    ChasingCry,
    /// <summary>
    /// Its nuclei pulsing from dark to brightest and back with the draining sound while it drains Samus, repeating.
    /// </summary>
    DrainingSamus,
}

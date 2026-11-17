namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// The Metroid's chasing and draining pulse loops (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="MetroidAnimations"/>.
/// </summary>
internal enum MetroidSegment
{
    /// <summary>
    /// Its nuclei pulsing from dark to brightest and back four times while it chases Samus, then a cry, repeating.
    /// </summary>
    ChasingSamus,
    /// <summary>
    /// Its nuclei pulsing from dark to brightest and back with the draining sound while it drains Samus, repeating.
    /// </summary>
    DrainingSamus,
}

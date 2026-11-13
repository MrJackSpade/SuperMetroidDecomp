namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// The Metroid's chasing and draining pulse loops as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="MetroidAnimations"/>.
/// </summary>
internal enum MetroidSegment
{
    /// <summary>The chasing Samus animation.</summary>
    ChasingSamus,
    /// <summary>The draining Samus animation.</summary>
    DrainingSamus,
}

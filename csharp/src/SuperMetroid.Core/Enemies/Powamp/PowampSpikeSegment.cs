namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// The Powamp spike's programs (#641), one per animation a spawn, branch or family step enters, in authored order.
/// Their steps are <see cref="PowampSpikeAnimations"/>.
/// </summary>
internal enum PowampSpikeSegment
{
    /// <summary>The initial animation.</summary>
    Initial,
    /// <summary>The delete animation.</summary>
    Delete,
}

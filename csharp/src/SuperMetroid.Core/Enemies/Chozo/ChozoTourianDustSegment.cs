namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The Wrecked Ship Chozo's footstep dust and the Tourian statue's descent dust (#641): one segment per animation a spawn, branch or family step enters, in authored order.
/// Their steps are <see cref="ChozoTourianDustAnimations"/>.
/// </summary>
internal enum ChozoTourianDustSegment
{
    /// <summary>The footsteps animation.</summary>
    Footsteps,
    /// <summary>The Tourian descent dust animation.</summary>
    TourianDescentDust,
    /// <summary>The Tourian descent dust loop animation.</summary>
    TourianDescentDustLoop,
}

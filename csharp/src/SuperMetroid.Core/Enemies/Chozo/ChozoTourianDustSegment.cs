namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The Wrecked Ship Chozo's footstep dust and the Tourian statue's descent dust (#641) animation segments, in authored order.
/// Their steps are <see cref="ChozoTourianDustAnimations"/>.
/// </summary>
internal enum ChozoTourianDustSegment
{
    /// <summary>The footsteps animation.</summary>
    Footsteps,
    /// <summary>One puff of the Tourian statue's descent dust, repeated, then the deletion.</summary>
    TourianDescentDust,
}

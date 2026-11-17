namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The Wrecked Ship Chozo's footstep dust and the Tourian statue's descent dust (#641) animation segments, in authored order.
/// Their steps are <see cref="ChozoTourianDustAnimations"/>.
/// </summary>
internal enum ChozoTourianDustSegment
{
    /// <summary>A footstep's dust scattering as a puff, a pair, a trio and a dense cloud before it vanishes.</summary>
    Footsteps,
    /// <summary>One puff of the Tourian statue's descent dust, which its animation repeats.</summary>
    TourianDescentDust,
    /// <summary>The descent dust's deletion once its puffs are done.</summary>
    TourianDescentDustEnd,
}

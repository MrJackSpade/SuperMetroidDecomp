namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Where a charge-flare component stands among its pictures (#2285): a beat of one of its runs.</summary>
/// <param name="Segment">The run of the component's pictures the place lies in.</param>
/// <param name="Beat">The beat of the run, counted from its first.</param>
public readonly record struct ChargeFlarePlace(ChargeFlareSegment Segment, int Beat);

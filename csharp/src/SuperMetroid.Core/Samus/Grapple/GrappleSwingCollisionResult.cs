namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>
/// Result of the six-point angular body sweep. DistanceFromFeet retains the native countdown:
/// six is the point nearest the hand and one is the point furthest beyond Samus.
/// </summary>
[IndependentFlag("whether the swing probe collided, a fact of the probe")]
internal readonly record struct GrappleSwingCollisionResult(bool Collided, [property: IntegerQuantity("pixel distance from Samus's feet to the collision")] int DistanceFromFeet);

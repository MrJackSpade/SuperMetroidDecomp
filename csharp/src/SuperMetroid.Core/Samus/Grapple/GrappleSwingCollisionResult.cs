namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>
/// Result of the six-point angular body sweep. DistanceFromFeet retains the native countdown:
/// six is the point nearest the hand and one is the point furthest beyond Samus.
/// </summary>
internal readonly record struct GrappleSwingCollisionResult(bool Collided, int DistanceFromFeet);

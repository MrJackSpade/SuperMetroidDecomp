using SuperMetroid.Core.Rooms.LevelData;
namespace SuperMetroid.Verification.Samus.Grapple;

/// <summary>The collision kind, expected cancellation and expected roomObjects of one samus grapple verify samus grapple swing and release case2.</summary>
/// <param name="CollisionKind">The collision kind.</param>
/// <param name="ExpectedCancellation">The expected cancellation.</param>
/// <param name="ExpectedObjects">The expected roomObjects.</param>
internal readonly record struct SamusGrappleVerifySamusGrappleSwingAndReleaseCase2(RoomCollisionType CollisionKind, bool ExpectedCancellation, int ExpectedObjects);

using SuperMetroid.Core.Rooms.LevelData;
namespace SuperMetroid.Verification.Samus.Grapple;

/// <summary>The collision kind, expected cancellation and expected plms of one samus grapple verify samus grapple swing and release case2.</summary>
/// <param name="CollisionKind">The collision kind.</param>
/// <param name="ExpectedCancellation">The expected cancellation.</param>
/// <param name="ExpectedPlms">The expected plms.</param>
internal readonly record struct SamusGrappleVerifySamusGrappleSwingAndReleaseCase2(RoomCollisionType CollisionKind, bool ExpectedCancellation, int ExpectedPlms);

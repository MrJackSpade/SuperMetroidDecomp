using SuperMetroid.Core.Rooms.LevelData;
namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>The collision kind and expected collision of one samus terrain movement verify samus block collision case2.</summary>
/// <param name="CollisionKind">The collision kind.</param>
/// <param name="ExpectedCollision">The expected collision.</param>
internal readonly record struct SamusTerrainMovementVerifySamusBlockCollisionCase2(RoomCollisionType CollisionKind, bool ExpectedCollision);

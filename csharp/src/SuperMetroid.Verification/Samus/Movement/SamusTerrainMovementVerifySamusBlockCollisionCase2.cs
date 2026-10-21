namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>The collision word and expected collision of one samus terrain movement verify samus block collision case2.</summary>
/// <param name="CollisionWord">The collision word.</param>
/// <param name="ExpectedCollision">The expected collision.</param>
internal readonly record struct SamusTerrainMovementVerifySamusBlockCollisionCase2(ushort CollisionWord, bool ExpectedCollision);

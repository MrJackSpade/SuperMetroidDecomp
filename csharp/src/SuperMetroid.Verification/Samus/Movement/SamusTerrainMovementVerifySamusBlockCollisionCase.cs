namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>The spike behavior and expected damage of one samus terrain movement verify samus block collision case.</summary>
/// <param name="SpikeBehavior">The spike behavior.</param>
/// <param name="ExpectedDamage">The expected damage.</param>
internal readonly record struct SamusTerrainMovementVerifySamusBlockCollisionCase(byte SpikeBehavior, ushort ExpectedDamage);

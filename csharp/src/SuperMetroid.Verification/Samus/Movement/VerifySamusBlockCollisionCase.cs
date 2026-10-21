namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>The boss defeated and expected damage of one verify samus block collision case.</summary>
/// <param name="BossDefeated">The boss defeated.</param>
/// <param name="ExpectedDamage">The expected damage.</param>
internal readonly record struct VerifySamusBlockCollisionCase(bool BossDefeated, ushort ExpectedDamage);

namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>The behavior, projectile type and family name of one verify samus morph ball movement case.</summary>
/// <param name="Behavior">The behavior.</param>
/// <param name="ProjectileType">The projectile type.</param>
/// <param name="FamilyName">The family name.</param>
internal readonly record struct VerifySamusMorphBallMovementCase(byte Behavior, ushort ProjectileType, string FamilyName);

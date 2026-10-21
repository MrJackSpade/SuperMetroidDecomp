namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>The beam type and expected reload of one samus projectiles verify samus power beam projectiles case.</summary>
/// <param name="BeamType">The beam type.</param>
/// <param name="ExpectedReload">The expected reload.</param>
internal readonly record struct SamusProjectilesVerifySamusPowerBeamProjectilesCase(ushort BeamType, ushort ExpectedReload);

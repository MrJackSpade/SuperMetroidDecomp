namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Where a Samus projectile program's native list begins and its prefix.</summary>
/// <param name="Program">The projectile program.</param>
/// <param name="Entry">The list's entry address.</param>
/// <param name="Prefix">The prefix address.</param>
internal readonly record struct SamusProjectileProgramAddressDefinition(SamusProjectileProgram Program, ushort Entry, ushort Prefix);

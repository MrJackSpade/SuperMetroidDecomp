namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A selection the port makes and the cartridge selector word that makes it.</summary>
/// <param name="Address">The bank-$93 address of the selector word.</param>
/// <param name="Kind">The projectile kind the port selects there.</param>
internal readonly record struct ProjectileSelectorWord(int Address, SamusProjectileKind Kind);

using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>The command and list of one verify samus power beam projectiles case.</summary>
/// <param name="Command">The command.</param>
/// <param name="List">The list.</param>
internal readonly record struct VerifySamusPowerBeamProjectilesCase(ProjectileTrailInstruction Command, ushort List);

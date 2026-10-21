using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>The power bomb and fast power bomb of one verify power bomb fuse case.</summary>
/// <param name="PowerBomb">The power bomb.</param>
/// <param name="FastPowerBomb">The fast power bomb.</param>
internal readonly record struct VerifyPowerBombFuseCase(SamusProjectileProgram PowerBomb, SamusProjectileProgram FastPowerBomb);

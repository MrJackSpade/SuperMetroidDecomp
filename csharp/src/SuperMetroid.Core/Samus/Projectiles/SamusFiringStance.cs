
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A firing direction and whether Samus is running.</summary>
/// <param name="Running">The running.</param>
/// <param name="Direction">The direction.</param>
internal readonly record struct SamusFiringStance(bool Running, SamusProjectileDirection Direction);


namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A firing direction and whether Samus is running.</summary>
/// <param name="Running">Whether Samus is running.</param>
/// <param name="Direction">The direction she fires in.</param>
internal readonly record struct SamusFiringStance(bool Running, SamusProjectileDirection Direction);

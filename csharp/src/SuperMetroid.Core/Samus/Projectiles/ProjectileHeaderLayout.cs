
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The program fields of a projectile header and its empty field count.</summary>
/// <param name="Fields">The fields.</param>
/// <param name="Empty">The empty.</param>
internal readonly record struct ProjectileHeaderLayout(SamusProjectileProgram?[] Fields, int Empty);

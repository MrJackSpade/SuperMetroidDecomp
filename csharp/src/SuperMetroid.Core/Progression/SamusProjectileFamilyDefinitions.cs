
namespace SuperMetroid.Core.Progression;

/// <summary>The game's projectile family order.</summary>
internal static class SamusProjectileFamilyDefinitions
{
    /// <summary>
    /// The families in the order the game ranks them: fired shots, then bombs, then the explosions shots leave. The
    /// game's ordered family comparisons read this order.
    /// </summary>
    internal static readonly SamusProjectileFamily[] Order =
    [
        SamusProjectileFamily.Beam, SamusProjectileFamily.Missile, SamusProjectileFamily.SuperMissile,
        SamusProjectileFamily.PowerBomb, SamusProjectileFamily.Bomb, SamusProjectileFamily.BeamExplosion,
        SamusProjectileFamily.MissileExplosion,
    ];
}


namespace SuperMetroid.Core.Progression;

/// <summary>The game's projectile family order and each family's row in the per-family projectile tables.</summary>
internal static class SamusProjectileFamilyDefinitions
{
    /// <summary>
    /// The families in the order the game ranks them: fired shots, then bombs, then the explosions shots leave. The
    /// cartridge's ordered family comparisons read this order.
    /// </summary>
    internal static readonly SamusProjectileFamily[] Order =
    [
        SamusProjectileFamily.Beam, SamusProjectileFamily.Missile, SamusProjectileFamily.SuperMissile,
        SamusProjectileFamily.PowerBomb, SamusProjectileFamily.Bomb, SamusProjectileFamily.BeamExplosion,
        SamusProjectileFamily.MissileExplosion,
    ];

    /// <summary>
    /// Each family's row in the per-family projectile tables (the non-beam selector and cooldown rows), in
    /// <see cref="Order"/>; rows four and six hold no family any producer makes.
    /// </summary>
    internal static readonly int[] TableRows = [0, 1, 2, 3, 5, 7, 8];
}

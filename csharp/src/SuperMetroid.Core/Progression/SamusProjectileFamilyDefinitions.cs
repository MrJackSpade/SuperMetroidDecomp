namespace SuperMetroid.Core.Progression;

/// <summary>What each family of Samus's projectiles is to the rules that test it (#2556).</summary>
internal static partial class SamusProjectileFamilyDefinitions
{
    /// <summary>
    /// Each family's role: beams, missiles and Super Missiles are fired shots, bombs and Power Bombs are bombs, and the
    /// beam and missile explosions are the explosions shots leave.
    /// </summary>
    internal static IdentityTable<SamusProjectileFamily, SamusProjectileFamilyRole> Roles { get; } =
        new(new Dictionary<SamusProjectileFamily, SamusProjectileFamilyRole>
        {
            [SamusProjectileFamily.Beam] = SamusProjectileFamilyRole.FiredShot,
            [SamusProjectileFamily.Missile] = SamusProjectileFamilyRole.FiredShot,
            [SamusProjectileFamily.SuperMissile] = SamusProjectileFamilyRole.FiredShot,
            [SamusProjectileFamily.PowerBomb] = SamusProjectileFamilyRole.Bomb,
            [SamusProjectileFamily.Bomb] = SamusProjectileFamilyRole.Bomb,
            [SamusProjectileFamily.BeamExplosion] = SamusProjectileFamilyRole.Explosion,
            [SamusProjectileFamily.MissileExplosion] = SamusProjectileFamilyRole.Explosion,
        });
}

namespace SuperMetroid.ResourceAudit.Reports;

/// <summary>Bank ownership of resource identities, derived from production catalogs.</summary>
internal static class ResourceBanks
{
    /// <summary>$86: instruction operands selecting enemy-projectile presentation frames.</summary>
    public const byte EnemyProjectilePrograms = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCodePointers.BankBase >> 16;
    /// <summary>$8D: enemy-projectile OAM compositions and palette-FX colors.</summary>
    public const byte ProjectileOamAndPaletteFx = SuperMetroid.Core.Fx.RoomFxRomData.Banks.PaletteFx >> 16;
    /// <summary>$93: Samus projectile animation compositions.</summary>
    public const byte SamusProjectiles = SuperMetroid.Core.Samus.Projectiles.SamusProjectileToolingRomData.ProjectileNumber;
}

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Verification's native view of a Ceres elevator arrival projectile's spritemap word.</summary>
internal static class CeresElevatorProjectileAccess
{
    /// <summary>$8000, the word a projectile holds before its first frame.</summary>
    internal const ushort NoSpritemap = 0x8000;

    /// <summary>Native views of a projectile.</summary>
    extension(CeresElevatorArrivalState.CeresElevatorProjectile self)
    {
        /// <summary>The bank-$8D spritemap the projectile shows, or <see cref="NoSpritemap"/> before its first frame.</summary>
        internal ushort SpritemapPointer => self.Pose is { } pose ? CeresElevatorArrivalDefinitions.Spritemap(pose) : NoSpritemap;
    }
}

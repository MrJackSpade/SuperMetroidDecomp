using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>Verification access to <see cref="CrocomireColorCatalog"/> members production does not use.</summary>
internal static class CrocomireColorCatalogAccess
{
    /// <summary>Reads independently editable Crocomire paint bands through the private selector.</summary>
    extension(CrocomireColorCatalog self)
    {
        /// <summary>Reads body colors used during the active fight.</summary>
        internal RgbColor ResolveFightBody(int color) => self.Get(CrocomireColorCatalog.Band.FightBody, color);

        /// <summary>Reads the initial wall's Crocomire color band.</summary>
        internal RgbColor ResolveInitialWall(int color) => self.Get(CrocomireColorCatalog.Band.InitialWall, color);

        /// <summary>Reads the initial projectile's Crocomire color band.</summary>
        internal RgbColor ResolveInitialProjectile(int color) => self.Get(CrocomireColorCatalog.Band.InitialProjectile, color);

        /// <summary>Reads the exposed skeleton-arm color band.</summary>
        internal RgbColor ResolveSkeletonArm(int color) => self.Get(CrocomireColorCatalog.Band.SkeletonArm, color);

        /// <summary>Reads the wall-spike color band for the Crocomire room.</summary>
        internal RgbColor ResolveWallSpikes(int color) => self.Get(CrocomireColorCatalog.Band.WallSpikes, color);
    }
}

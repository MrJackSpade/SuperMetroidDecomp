using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="CrocomireColorCatalog"/> members production does not use.</summary>
internal static class CrocomireColorCatalogAccess
{
    /// <summary>Reads independently editable Crocomire paint bands through the private selector.</summary>
    extension(CrocomireColorCatalog self)
    {
        /// <summary>Reads body colors used during the active fight.</summary>
        internal Bgr555 ResolveFightBody(int color) => ((Bgr555)(self.Get(CrocomireColorCatalog.Band.FightBody, (int)(color))));

        /// <summary>Reads the initial wall's Crocomire color band.</summary>
        internal Bgr555 ResolveInitialWall(int color) => ((Bgr555)(self.Get(CrocomireColorCatalog.Band.InitialWall, (int)(color))));

        /// <summary>Reads the initial projectile's Crocomire color band.</summary>
        internal Bgr555 ResolveInitialProjectile(int color) => ((Bgr555)(self.Get(CrocomireColorCatalog.Band.InitialProjectile, (int)(color))));

        /// <summary>Reads the exposed skeleton-arm color band.</summary>
        internal Bgr555 ResolveSkeletonArm(int color) => ((Bgr555)(self.Get(CrocomireColorCatalog.Band.SkeletonArm, (int)(color))));

        /// <summary>Reads the wall-spike color band for the Crocomire room.</summary>
        internal Bgr555 ResolveWallSpikes(int color) => ((Bgr555)(self.Get(CrocomireColorCatalog.Band.WallSpikes, (int)(color))));
    }
}

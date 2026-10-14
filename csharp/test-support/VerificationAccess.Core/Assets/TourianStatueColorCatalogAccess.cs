using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="TourianStatueColorCatalog"/> members production does not use.</summary>
internal static class TourianStatueColorCatalogAccess
{
    /// <summary>Inspects the installed Tourian statue color definition.</summary>
    extension(TourianStatueColorCatalog self)
    {
        /// <summary>Reads one statue color through its private paint definition.</summary>
        internal Bgr555 ResolveStatue(int color) => ((Bgr555)(self.statueColors.Read((int)(color))));
    }
}

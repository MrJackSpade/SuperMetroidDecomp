using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Areas.Tourian.Artwork;

/// <summary>Verification access to <see cref="TourianStatuePalettes"/> members production does not use.</summary>
internal static class TourianStatueColorCatalogAccess
{
    /// <summary>Inspects the installed Tourian statue color definition.</summary>
    extension(TourianStatuePalettes self)
    {
        /// <summary>Reads one statue color through its private paint definition.</summary>
        internal RgbColor ResolveStatue(int color) => self.statueColors.Read(color);
    }
}

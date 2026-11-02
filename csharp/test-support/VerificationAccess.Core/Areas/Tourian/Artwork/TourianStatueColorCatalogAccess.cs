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

        /// <summary>Reads base-decoration colour 0–15, including its retained transparent-slot word.</summary>
        internal RgbColor ResolveBase(int color) => self.baseColors.Read(color);

        /// <summary>Reads colour 0–7 of the statue's grey-transition band.</summary>
        internal RgbColor ResolveGrey(int color) => self.greyColors.Read(color);
    }
}

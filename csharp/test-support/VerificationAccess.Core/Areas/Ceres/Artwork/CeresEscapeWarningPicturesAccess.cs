using SuperMetroid.AssetExtraction.Areas.Ceres;
namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>Verification access to <see cref="CeresEscapeWarningPictures"/> members production does not use.</summary>
internal static class CeresEscapeWarningPicturesAccess
{
    /// <summary>Provides range checks against all authored Ceres escape overlay pages.</summary>
    extension(CeresEscapeWarningPictures)
    {
        /// <summary>Checks whether a byte falls within any page's source-word interval.</summary>
        internal static bool ContainsByteAddress(int address)
        {
            foreach (CeresEscapeWarningPicture page in CeresEscapeOverlayPages.All)
                if (address >= CeresEscapeNativeSources.OverlaySource(page) &&
                    address < CeresEscapeNativeSources.OverlaySource(page) + page.WordCount * sizeof(ushort))
                    return true;
            return false;
        }
    }
}

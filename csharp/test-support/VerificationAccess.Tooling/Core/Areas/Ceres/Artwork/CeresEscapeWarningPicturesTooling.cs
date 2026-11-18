using SuperMetroid.AssetExtraction.Areas.Ceres;
namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>Development-tool members of <see cref="CeresEscapeWarningPictures"/>; never linked by player hosts.</summary>
internal static class CeresEscapeWarningPicturesTooling
{
    /// <summary>
    /// Accepts only a known Ceres overlay row's source whose native byte length matches the requested range: the
    /// cartridge copies each row of a page on its own (#2425).
    /// </summary>
    internal static bool IsSource(int sourceAddress, int byteCount)
    {
        foreach (CeresEscapeWarningPicture page in CeresEscapeOverlayPages.All)
        {
            for (int row = 0; row < page.RowCount; row++)
            {
                if (CeresEscapeNativeSources.RowSource(page, row) == sourceAddress && page.Width * sizeof(ushort) == byteCount)
                    return true;
            }
        }
        return false;
    }
}

using SuperMetroid.AssetExtraction.Areas.Ceres;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>Cartridge values of the Ceres warning text's pages that only verification reads.</summary>
internal static class CeresWarningTextPageConstants
{
    /// <summary>
    /// The warning page a native escape transfer from <paramref name="sourceAddress"/> shows: the page whose installed
    /// characters the transfer copies exactly, or null for one that copies no page whole.
    /// </summary>
    /// <param name="sourceAddress">The transfer's native source.</param>
    /// <param name="byteCount">Bytes the transfer moves.</param>
    internal static int? ForTransfer(int sourceAddress, int byteCount)
    {
        int offset = sourceAddress - CeresEscapeNativeSources.PageSource(CeresWarningTextPageDefinitions.InstalledPage);
        for (int page = 0; page < CeresWarningTextPageDefinitions.PageCount; page++)
        {
            if (CeresWarningTextPages.ByteOffset(page) == offset && CeresWarningTextPages.ByteCount(page) == byteCount)
                return page;
        }
        return null;
    }
}

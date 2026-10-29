using SuperMetroid.AssetExtraction.Areas.Ceres;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>Cartridge values and helpers of <see cref="CeresWarningTextPage"/> that only verification reads.</summary>
internal static class CeresWarningTextPageConstants
{
    /// <summary>
    /// The warning page look a native escape transfer from <paramref name="sourceAddress"/> shows: the page whose installed
    /// characters the transfer copies exactly, or null for one that copies no page whole.
    /// </summary>
    /// <param name="sourceAddress">The transfer's native source.</param>
    /// <param name="byteCount">Bytes the transfer moves.</param>
    internal static CeresWarningTextPage? ForTransfer(int sourceAddress, int byteCount)
    {
        int offset = sourceAddress - CeresEscapeNativeSources.PageSource(CeresWarningTextPageDefinitions.InstalledPage);
        for (int page = 0; page < CeresEscapePageDefinitions.WarningPageCount; page++)
        {
            var look = new CeresWarningTextPage(page);
            if (CeresWarningTextPages.ByteOffset(look) == offset && CeresWarningTextPages.ByteCount(look) == byteCount)
                return look;
        }
        return null;
    }
}

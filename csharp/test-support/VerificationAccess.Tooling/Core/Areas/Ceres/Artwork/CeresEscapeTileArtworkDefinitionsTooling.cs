using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>Development-tool members of <see cref="CeresEscapeTileArtworkDefinitions"/>; never linked by player hosts.</summary>
internal static class CeresEscapeTileArtworkDefinitionsTooling
{
    /// <summary>Accepts only a positive byte range wholly inside one of the two Ceres escape DMA pages.</summary>
    internal static bool Contains(int sourceAddress, int byteCount)
    {
        foreach (TileSheetDefinition page in CeresEscapeTileArtworkDefinitions.All)
        {
            int offset = sourceAddress - page.SourceAddress;
            if (offset >= 0 && byteCount > 0 && offset <= page.ByteCount - byteCount)
                return true;
        }
        return false;
    }
}

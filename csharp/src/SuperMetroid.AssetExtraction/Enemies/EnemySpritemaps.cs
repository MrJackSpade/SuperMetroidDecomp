using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The installed enemy OAM compositions: the composition displayed for the frame gameplay shows, and their identity.
/// </summary>
internal static class EnemySpritemaps
{

    /// <summary>Resolves an editable presentation binding, or <see cref="EnemySpritemapParts.Empty"/> when no bound frame is installed.</summary>
    internal static bool TryGetDisplay(EnemySpritemapCatalog catalog, EnemyFrameId frame, out EnemySpritemapParts parts)
    {
        if (catalog.selection.DisplayFrames.TryGetValue(frame, out EnemyFrameId selected) &&
            catalog.selection.Frames.TryGetValue(selected, out EnemySpritemapParts? found))
        {
            parts = found;
            return true;
        }
        parts = EnemySpritemapParts.Empty;
        return false;
    }
}

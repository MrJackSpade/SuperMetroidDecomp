using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The installed enemy OAM compositions: the composition displayed for the frame gameplay shows, and their identity.
/// </summary>
internal static class EnemySpritemaps
{
    /// <summary>Hashes each frame's parts, then each display binding, in frame-key order.</summary>
    internal static string ContentIdentity(EnemySpritemapCatalog catalog) =>
        SelectedPresentationHash.Create("enemy-oam-v2", content =>
        {
            foreach ((EnemyFrameId frame, EnemySpritemapParts parts) in catalog.selection.Frames.OrderBy(pair => EnemyFrameSheetKeys.Of(pair.Key), StringComparer.Ordinal))
            {
                content.AppendName("frame", EnemyFrameSheetKeys.Of(frame));
                content.AppendEnemyParts(parts);
            }
            foreach ((EnemyFrameId stock, EnemyFrameId selected) in catalog.selection.DisplayFrames.OrderBy(pair => EnemyFrameSheetKeys.Of(pair.Key), StringComparer.Ordinal))
            {
                content.AppendName("native-binding", EnemyFrameSheetKeys.Of(stock));
                content.AppendName("selected-binding", EnemyFrameSheetKeys.Of(selected));
            }
        });

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

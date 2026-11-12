using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Cartridge values and helpers of <see cref="EnemySpritemaps"/> that only verification reads.</summary>
internal static class EnemySpritemapsConstants
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
}

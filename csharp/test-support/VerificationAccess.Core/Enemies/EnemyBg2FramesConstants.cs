using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Cartridge values and helpers of <see cref="EnemyBg2Frames"/> that only verification reads.</summary>
internal static class EnemyBg2FramesConstants
{
    /// <summary>Canonical frames retain command order, destinations and each tile-run boundary.</summary>
    internal static string ContentIdentity(Dictionary<EnemyFrameId, EnemyBg2TilemapWrite[]> frames) =>
        SelectedPresentationHash.Create("enemy-bg2-frames-v3", content =>
        {
            foreach ((EnemyFrameId frame, EnemyBg2TilemapWrite[] writes) in frames.OrderBy(pair => EnemyFrameSheetKeys.Of(pair.Key), StringComparer.Ordinal))
            {
                content.AppendName("frame", EnemyFrameSheetKeys.Of(frame));
                content.Append("writes", writes.Length);
                foreach (EnemyBg2TilemapWrite write in writes)
                {
                    content.Append("destination", write.FirstCell);
                    content.AppendTiles("tiles", write.Tiles.Span);
                }
            }
        });
}
